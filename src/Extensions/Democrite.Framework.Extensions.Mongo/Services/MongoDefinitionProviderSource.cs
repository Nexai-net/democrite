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
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Options;

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
    internal sealed class MongoDefinitionProviderSource<TDocumentType> : ProviderStrategyBaseSource<TDocumentType, Guid>, IProviderStrategySource<TDocumentType, Guid>, IDefinitionSourceProvider<TDocumentType>
        where TDocumentType : class, IDefinition
    {
        #region Fields

        private static readonly FilterDefinition<DefinitionContainer<TDocumentType>> s_discriminatorFilter;
        private static readonly ProjectionDefinition<DefinitionContainer<TDocumentType>> s_containerProject;
        private static readonly FindOptions s_querySetting;

        //private readonly IMongoClientFactory _mongoClientFactory;

        private readonly IReadOnlyRepository<DefinitionContainer<TDocumentType>, Guid> _repositoryContainer;
        private readonly IReadOnlyRepository<TDocumentType, Guid> _repository;

        //private readonly MongoDBOptions _mongoDBOptions;
        private readonly string _configurationKey;

        private readonly Dictionary<Guid, DefinitionContainer> _cachedContainer;
        private readonly ReaderWriterLockSlim _cacheLock;

        //private IMongoCollection<DefinitionContainer<TDocumentType>>? _collection;
        //private IMongoClient? _client;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoDefinitionProviderSource{TDocumentType}"/> class.
        /// </summary>
        static MongoDefinitionProviderSource()
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
        /// Initializes a new instance of the <see cref="MongoDefinitionProviderSource{TDocumentType}"/> class.
        /// </summary>
        public MongoDefinitionProviderSource(IServiceProvider serviceProvider,
                                             MongoRepositoryDefinitionFactory mongoClientFactory,
                                             string configurationKey)
            : base(serviceProvider)
        {
            this._configurationKey = configurationKey;
            var repository = mongoClientFactory.Get<IReadOnlyRepository<TDocumentType, Guid>, TDocumentType, Guid>(serviceProvider, new Core.Abstractions.Models.RepositoryGetOptions(DemocriteConstants.DefaultDefinitionStorageName, true, configurationKey));

            this._repository = repository;
            this._repositoryContainer = (IReadOnlyRepository<DefinitionContainer<TDocumentType>, Guid>)repository;

            this._cachedContainer = new Dictionary<Guid, DefinitionContainer>();

            this._cacheLock = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override ValueTask OnProviderInitializedAsync(IServiceProvider? _, CancellationToken token)
        {
            if (this._repository is ISupportInitialization<string> initRepo)
                return initRepo.InitializationAsync(DemocriteConstants.DefaultDefinitionStorageName, token);

            throw new NotSupportedException("Repository MUST inherite from " + nameof(ISupportInitialization<string>));
        }

        /// <inheritdoc />
        public override async ValueTask<TDocumentType?> GetFirstValueAsync(Expression<Func<TDocumentType, bool>> filterExpression,
                                                                           Func<TDocumentType, bool> filter,
                                                                           CancellationToken token)
        {
            await EnsureProviderIsInitialized();

            var item = await base.GetFirstValueAsync(filterExpression, filter, token);

            if (item is not null)
                return item;

            item = await this._repository.GetFirstValueAsync(filterExpression, token);

            // call to cached the result
            if (item is not null)
                await TryGetDataAsync(item.Uid, token);

            return item;
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDocumentType>> GetValuesAsync(Expression<Func<TDocumentType, bool>> filterExpression,
                                                                                           Func<TDocumentType, bool> _,
                                                                                           CancellationToken token)
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                await EnsureProviderIsInitialized();
                var docs = await this._repository.GetValuesAsync(filterExpression, token);
                return docs;
            }
            catch (Exception ex)
            {
                throw;
            }
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }

        /// <inheritdoc />
        public override async ValueTask<IReadOnlyCollection<TDocumentType>> GetValuesAsync(IEnumerable<Guid> keys, CancellationToken token)
        {
            await EnsureProviderIsInitialized();

            var values = await base.GetValuesAsync(keys, token);

            var missingKeys = keys.Except(values.Select(v => v.Uid)).ToArray();

            if (!missingKeys.Any())
                return values;

            //var missingItems = await this._collection.Find(f => missingKeys.Contains(f.DeferredId), s_querySetting)
            //                                         .ToListAsync(token);

            var missingItems = (await this._repositoryContainer.GetValuesAsync(f => missingKeys.Contains(f.Uid), token))!;

            SafeCacheItemsImpl(missingItems);

            return values.Concat(missingItems.Where(m => m != null).Select(m => m!.Definition)).ToArray();
        }

        /// <inheritdoc />
        public override async ValueTask<(bool Success, TDocumentType? Result)> TryGetDataAsync(Guid key, CancellationToken token)
        {
            await EnsureProviderIsInitialized();

            var cachedResult = await base.TryGetDataAsync(key, token);

            if (cachedResult.Success)
                return cachedResult;

            var item = await this._repositoryContainer.GetFirstValueAsync(k => k.Uid == key, token);

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
            SafeCacheItemsImpl(items);
        }

        /// <summary>
        /// the cache items.
        /// </summary>
        [ThreadSafe]
        private void SafeCacheItemsImpl(IReadOnlyCollection<DefinitionContainer<TDocumentType>?> items)
        {
            this._cacheLock.EnterWriteLock();
            try
            {
                var formatItems = items.Where(kv => kv != null).Select(kv => (kv!.Uid, kv.Definition)).ToArray();

                base.SafeAddOrReplace(formatItems);

                foreach (var item in items)
                    this._cachedContainer[item!.Uid] = item.ToContainerHeaderOnly();
            }
            finally
            {
                this._cacheLock.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        protected override async ValueTask ForceUpdateAfterInitAsync(CancellationToken token)
        {
            var containers = await this._repositoryContainer.GetValuesAsync<DefinitionContainerProjection>(_ => true, token);

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
