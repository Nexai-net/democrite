// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Extensions.Mongo.Models;
    using Democrite.Framework.Extensions.Mongo.Repositories;

    using Elvex.Toolbox.Abstractions.Attributes;
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;

    using Orleans.Providers;
    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.Utils;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base Provider source linked to search definition on mongoDB
    /// </summary>
    internal sealed class MongoDefinitionProviderMultiSource<TDocumentType> : ProviderStrategyBaseSource<TDocumentType, Guid>, IProviderStrategySource<TDocumentType, Guid>, IDefinitionSourceProvider<TDocumentType>
        where TDocumentType : class, IDefinition
    {
        #region Fields

        private IReadOnlyCollection<MongoDefinitionProviderSource<TDocumentType>>? _mongoDefinitionProviderSources;

        private readonly IReadOnlyCollection<MongoDBDefinitionConnectionOptions> _connections;
        private readonly ILogger<MongoDefinitionProviderMultiSource<TDocumentType>> _logger;
        private readonly MongoRepositoryDefinitionFactory _mongoClientFactory;
        private readonly IServiceProvider _localServiceProvider;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDefinitionProviderMultiSource{TDocumentType}"/> class.
        /// </summary>
        public MongoDefinitionProviderMultiSource(IServiceProvider serviceProvider,
                                                  MongoRepositoryDefinitionFactory mongoClientFactory,
                                                  IKeyServicesReadOnlyDictionary<string, MongoDBDefinitionConnectionOptions> connections,
                                                  ILogger<MongoDefinitionProviderMultiSource<TDocumentType>>? logger = null)
            : base(serviceProvider)
        {
            this._localServiceProvider = serviceProvider;
            this._timeManager = serviceProvider.GetRequiredService<ITimeManager>();

            this._mongoClientFactory = mongoClientFactory;
            this._connections = connections.OrderBy(o => o.Value.Order)
                                           .Select(o => o.Value)
                                           .ToArray();

            this._logger = logger ?? NullLogger<MongoDefinitionProviderMultiSource<TDocumentType>>.Instance;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async ValueTask OnProviderInitializedAsync(IServiceProvider? _, CancellationToken token)
        {
            var sources = new List<MongoDefinitionProviderSource<TDocumentType>>();

            foreach (var co in this._connections)
            {
                var src = new MongoDefinitionProviderSource<TDocumentType>(this._localServiceProvider,
                                                                           this._mongoClientFactory,
                                                                           co.Key ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

                sources.Add(src);

                await src.InitializationAsync(this._localServiceProvider, token);
            }

            this._mongoDefinitionProviderSources = sources;
        }

        /// <inheritdoc />
        public override async ValueTask<TDocumentType?> GetFirstValueAsync(Expression<Func<TDocumentType, bool>> filterExpression,
                                                                           Func<TDocumentType, bool> filter,
                                                                           CancellationToken token)
        {
            var results = await ApplyMulti<TDocumentType>(async src =>
            {
                var doc = await src.GetFirstValueAsync(filterExpression, filter, token);
                return doc?.AsEnumerable()?.ToArray() ?? EnumerableHelper<TDocumentType>.ReadOnlyArray;
            }, d => d);

            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDocumentType>> GetValuesAsync(Expression<Func<TDocumentType, bool>> filterExpression,
                                                                                           Func<TDocumentType, bool> predicate,
                                                                                           CancellationToken token)
        {
            var results = await ApplyMulti<TDocumentType>(async src =>
            {
                var doc = await src.GetValuesAsync(filterExpression, predicate, token);
                return doc?.ToArray() ?? EnumerableHelper<TDocumentType>.ReadOnlyArray;
            }, d => d);

            return results;
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDocumentType>> GetValuesAsync(IEnumerable<Guid> keys, CancellationToken token)
        {
            var results = await ApplyMulti<TDocumentType>(async src =>
            {
                var doc = await src.GetValuesAsync(keys, token);
                return doc?.ToArray() ?? EnumerableHelper<TDocumentType>.ReadOnlyArray;
            }, d => d);

            return results;
        }

        /// <inheritdoc />
        public override async ValueTask<(bool Success, TDocumentType? Result)> TryGetDataAsync(Guid key, CancellationToken token)
        {
            var results = await ApplyMulti<(bool Success, TDocumentType? Result)>(async src =>
            {
                var doc = await src.TryGetDataAsync(key, token);
                return doc.AsEnumerable().ToArray() ?? EnumerableHelper<(bool Success, TDocumentType? Result)>.ReadOnlyArray;
            }, d => d.Result!);

            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        protected override async ValueTask ForceUpdateAfterInitAsync(CancellationToken token)
        {
            foreach (var src in this._mongoDefinitionProviderSources ?? EnumerableHelper<MongoDefinitionProviderSource<TDocumentType>>.ReadOnly)
            {
                try
                {
                    await src.ForceUpdateAsync(token);
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Error, "[Mongo][Definition] - repository - {exception}", ex);
                }
            }
        }

        /// <inheritdoc />
        private async ValueTask<IReadOnlyCollection<TResult>> ApplyMulti<TResult>(Func<MongoDefinitionProviderSource<TDocumentType>, Task<IReadOnlyCollection<TResult>>> search, Func<TResult, TDocumentType> get)
        {
            await EnsureProviderIsInitialized();

            var resultDocs = new List<TResult>();
            foreach (var src in this._mongoDefinitionProviderSources ?? EnumerableHelper<MongoDefinitionProviderSource<TDocumentType>>.ReadOnly)
            {
                var docs = await search(src);
                resultDocs.AddRange(docs);
            }

            return Merge(resultDocs, get);
        }

        /// <summary>
        /// Merges the specified documents.
        /// </summary>
        private IReadOnlyCollection<TResult> Merge<TResult>(IReadOnlyCollection<TResult> documents, Func<TResult, TDocumentType> get)
        {
            return documents.Select(d => (doc: get(d), source: d))
                            .GroupBy(d => d.doc.Uid)
                            .Select(grp => grp.OrderByDescending(d => d.doc.MetaData?.UTCUpdateTime ?? this._timeManager.UtcNow).First().source)
                            .ToArray();
        }

        #endregion
    }
}
