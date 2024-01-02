// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Mongo.Models;
    using Democrite.Framework.Toolbox.Abstractions.Attributes;
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;

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
    public abstract class DefinitionBaseProviderSource<TDocumentType> : ProviderStrategyBaseSource<TDocumentType, Guid>, IProviderStrategySource<TDocumentType, Guid>
        where TDocumentType : class, IDefinition
    {
        #region Fields

        private static readonly FilterDefinition<DefinitionContainer<TDocumentType>> s_discriminatorFilter;
        private static readonly ProjectionDefinition<DefinitionContainer<TDocumentType>> s_containerProject;
        private static readonly FindOptions s_querySetting;

        private readonly IMongoClientFactory _mongoClientFactory;
        private readonly MongoDBOptions _mongoDBOptions;
        private readonly string _key;

        private readonly Dictionary<Guid, DefinitionContainer> _cachedContainer;
        private readonly ReaderWriterLockSlim _cacheLock;

        private IMongoCollection<DefinitionContainer<TDocumentType>>? _collection;
        private IMongoClient? _client;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DefinitionBaseProviderSource{TDocumentType}"/> class.
        /// </summary>
        static DefinitionBaseProviderSource()
        {
            s_querySetting = new FindOptions()
            {
                AllowDiskUse = true,
                MaxAwaitTime = TimeSpan.FromSeconds(10)
            };

            s_discriminatorFilter = Builders<DefinitionContainer<TDocumentType>>.Filter.Eq(f => f.Discriminator, DefinitionContainer<TDocumentType>.DefaultDiscriminator);

            s_containerProject = Builders<DefinitionContainer<TDocumentType>>.Projection.Include(p => p.Uid)
                                                                                        .Include(p => p.Etag)
                                                                                        .Include(p => p.Discriminator)
                                                                                        .Exclude(p => p.Definition);
            BsonClassMap.TryRegisterClassMap<DefinitionContainer<TDocumentType>>();
            BsonClassMap.TryRegisterClassMap<TDocumentType>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionBaseProviderSource{TDocumentType}"/> class.
        /// </summary>
        protected DefinitionBaseProviderSource(IMongoClientFactory mongoClientFactory,
                                               MongoDBOptions mongoDBOptions,
                                               string key)
        {
            this._key = key;
            this._mongoDBOptions = mongoDBOptions;
            this._mongoClientFactory = mongoClientFactory;

            this._cachedContainer = new Dictionary<Guid, DefinitionContainer>();

            this._cacheLock = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override ValueTask OnInitializedAsync(CancellationToken token)
        {
            this._client = this._mongoClientFactory.Create(this._key);

            var dataBaseName = this._mongoDBOptions.DatabaseName ?? nameof(Democrite).ToLower();
            var database = this._client.GetDatabase(dataBaseName);

            var settings = new MongoCollectionSettings();
            this._mongoDBOptions.CollectionConfigurator?.Invoke(settings);

            this._collection = database.GetCollection<DefinitionContainer<TDocumentType>>(this._mongoDBOptions.CollectionPrefix + "Definitions", settings);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public override async ValueTask<TDocumentType?> GetFirstValueAsync(Expression<Func<TDocumentType, bool>> filterExpression, Func<TDocumentType, bool> filter, CancellationToken token)
        {
            var item = await base.GetFirstValueAsync(filterExpression, filter, token);

            if (item is null)
                return item;

            item = await this._collection.Aggregate().Match(s_discriminatorFilter)
                                                     .ReplaceRoot(i => i.Definition)
                                                     .Match(filterExpression)
                                                     .Limit(1)
                                                     .FirstOrDefaultAsync(token);

            if (item != null)
                await TryGetDataAsync(item.Uid, token);

            return item;
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDocumentType>> GetValuesAsync(Expression<Func<TDocumentType, bool>> filterExpression, Func<TDocumentType, bool> filter, CancellationToken token)
        {
            var pipeline = new EmptyPipelineDefinition<DefinitionContainer<TDocumentType>>().Match(s_discriminatorFilter)
                                                                                            .ReplaceRoot(i => i.Definition)
                                                                                            .Match(filterExpression)
                                                                                            .Limit(1);

            var cursor = await this._collection!.AggregateAsync(pipeline, cancellationToken: token);
            
            var results = await cursor.ToListAsync();
            return results;
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDocumentType>> GetValuesAsync(IEnumerable<Guid> keys, CancellationToken token)
        {
            var values = await base.GetValuesAsync(keys, token);

            var missingKeys = keys.Except(values.Select(v => v.Uid)).ToArray();

            if (!missingKeys.Any())
                return values;

            var missingItems = await this._collection.Find(f => missingKeys.Contains(f.Uid), s_querySetting)
                                                     .ToListAsync(token);

            SafeCacheItems(missingItems.ToArray());

            return values.Concat(missingItems.Select(s => s.Definition)).ToArray();
        }

        /// <inheritdoc />
        public override async ValueTask<(bool Success, TDocumentType? Result)> TryGetDataAsync(Guid key, CancellationToken token)
        {
            var cachedResult = await base.TryGetDataAsync(key, token);

            if (cachedResult.Success)
                return cachedResult;

            var item = await this._collection.Find(k => k.Uid == key, s_querySetting).FirstOrDefaultAsync(token);

            if (item == null)
                return (false, default);

            SafeCacheItems(item);

            return (true, item.Definition);
        }

        /// <summary>
        /// the cache items.
        /// </summary>
        [ThreadSafe]
        private void SafeCacheItems(params DefinitionContainer<TDocumentType>[] items)
        {
            this._cacheLock.EnterWriteLock();
            try
            {
                var formatItems = items.Select(kv => (kv.Uid, kv.Definition)).ToArray();

                base.SafeAddOrReplace(formatItems);

                foreach (var item in items)
                    this._cachedContainer[item.Uid] = item.ToContainerHeaderOnly();
            }
            finally
            {
                this._cacheLock.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public override async ValueTask ForceUpdateAsync(CancellationToken token)
        {
            var containers = await this._collection.Find(s_discriminatorFilter, s_querySetting)
                                                   .Project(Builders<DefinitionContainer<TDocumentType>>.Projection.Expression(p => new { p.Uid, p.Etag, p.Discriminator }))
                                                   .ToListAsync(token);

            this._cacheLock.EnterWriteLock();
            try
            {
                foreach (var container in containers)
                {
                    if (this._cachedContainer.TryGetValue(container.Uid, out var oldContainer) && oldContainer.Etag != container.Etag)
                        this._cachedContainer.Remove(container.Uid);
                }

                var toRemove = this._cachedContainer.Keys
                                                    .Except(containers.Select(c => c.Uid))
                                                    .ToArray();

                base.SafeRemoves(toRemove);
            }
            finally
            {
                this._cacheLock.ExitWriteLock();
            }
        }

        #endregion
    }
}
