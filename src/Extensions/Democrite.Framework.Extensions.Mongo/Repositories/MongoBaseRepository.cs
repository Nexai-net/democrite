// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Repositories
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Extensions.Mongo.Abstractions.Repositories;
    using Democrite.Framework.Extensions.Mongo.Models;

    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Supports;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;

    using Orleans.Providers;
    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.Utils;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base repository about handling mongo storage
    /// </summary>
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    internal abstract class MongoBaseRepository<TEntity, TEntityId> : SupportBaseInitialization<string>, IMongoRepository<TEntity, TEntityId>, ISupportDebugDisplayName
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : notnull, IEquatable<TEntityId>
    {
        #region Fields

        private static readonly Dictionary<Type, BsonDocument> s_projectDefinitionCache;
        private static readonly FilterDefinition<TEntity>? s_discriminatorFilter;
        private static readonly ReaderWriterLockSlim s_projectionLocker;
        private static readonly BsonDocument s_idProjection;

        private readonly IMongoClientFactory _mongoClientFactory;
        private readonly IServiceProvider _serviceProvider;

        private readonly bool _preventAnyKindOfDiscriminatorUsage;
        private readonly string _configurationName;
        private readonly string _storageName;
        private readonly bool _isReadOnly;

        private IMongoClient _client;
        private IOptions<MongoDBOptions>? _mongoDBOptions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoBaseRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        static MongoBaseRepository()
        {
            s_projectionLocker = new ReaderWriterLockSlim();
            s_projectDefinitionCache = new Dictionary<Type, BsonDocument>();

            var entityTraits = typeof(TEntity);
            if (entityTraits.IsAssignableTo(typeof(IContainerWithDiscriminator<TEntity>)) && entityTraits.IsInterface == false && entityTraits.IsAbstract == false)
                s_discriminatorFilter = ((IContainerWithDiscriminator<TEntity>)Activator.CreateInstance<TEntity>()).DiscriminatorFilter;

            // Projection between EntityId and mongo _id
            s_idProjection = new BsonDocument()
            {
                { "_id", new BsonDocument()  {
                    { "$ifNull", new  BsonArray() {
                        "$_id",
                        "$" + nameof(IEntityWithId<Guid>.Uid)
                    } }
                } }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoBaseRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        protected MongoBaseRepository(IMongoClientFactory mongoClientFactory,
                                      IServiceProvider serviceProvider,
                                      string configurationName,
                                      string storageName,
                                      bool isReadOnly = false,
                                      bool preventAnyKindOfDiscriminatorUsage = false)
        {
            this._isReadOnly = isReadOnly;

            this._serviceProvider = serviceProvider;
            this._mongoClientFactory = mongoClientFactory;

            this._storageName = storageName;
            this._configurationName = configurationName;

            this._preventAnyKindOfDiscriminatorUsage = preventAnyKindOfDiscriminatorUsage;

            // Is Initialized before any report
            this.MongoCollection = null!;
            this._client = null!;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IMongoCollection<TEntity> MongoCollection { get; private set; }
        //public IMongoCollection<BsonDocument> DEBUGMongoCollection { get; private set; }

        /// <inheritdoc />
        public virtual bool IsReadOnly
        {
            get { return this._isReadOnly; }
        }

        /// <inheritdoc />
        public virtual bool SupportExpressionFilter
        {
            get { return true; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> DeleteRecordAsync(TEntityId uid, CancellationToken cancellationToken)
        {
            CheckReadOnly();
            await EnsureInitialized(cancellationToken);

            var del = await this.MongoCollection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(e => e.Uid, uid), cancellationToken: cancellationToken);
            return del.DeletedCount == 1;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteRecordAsync(IReadOnlyCollection<TEntityId> entityIds, CancellationToken cancellationToken)
        {
            CheckReadOnly();
            await EnsureInitialized(cancellationToken);

            var deleteCount = await this.MongoCollection.DeleteManyAsync(Builders<TEntity>.Filter.In(e => e.Uid, entityIds), cancellationToken: cancellationToken);
            return deleteCount.DeletedCount > 0;
        }

        /// <inheritdoc />
        public async ValueTask<TEntity?> GetFirstValueAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            await EnsureInitialized(token);

            var filter = filterExpression is not null ? Builders<TEntity>.Filter.Where(filterExpression) : Builders<TEntity>.Filter.Empty;
            filter = EnhanceFilter(filter);

            var entity = await this.MongoCollection.Find<TEntity>(filter)
                                                   .Limit(1)
                                                   .FirstOrDefaultAsync(token);
            return entity;
        }

        /// <inheritdoc />
        public async ValueTask<TProjection?> GetFirstValueAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            await EnsureInitialized(token);

            var filter = filterExpression is not null ? Builders<TEntity>.Filter.Where(filterExpression) : Builders<TEntity>.Filter.Empty;
            filter = EnhanceFilter(filter);

            var entity = await this.MongoCollection.Find<TEntity>(filter)
                                                   .Project(GetProjectionDefinition<TEntity, TProjection>(false))
                                                   .Limit(1)
                                                   .FirstOrDefaultAsync(token);
            return entity;
        }

        /// <inheritdoc />
        public async ValueTask<TEntity?> GetValueByIdAsync([NotNull] TEntityId entityId, CancellationToken token)
        {
            await EnsureInitialized(token);

            var entity = await this.MongoCollection.Find<TEntity>(Builders<TEntity>.Filter.Eq(e => e.Uid, entityId))
                                                   .Limit(1)
                                                   .FirstOrDefaultAsync(token);
            return entity;
        }

        /// <inheritdoc />
        public async ValueTask<TProjection?> GetValueByIdAsync<TProjection>([NotNull] TEntityId entityId, CancellationToken token)
        {
            await EnsureInitialized(token);

            var entity = await this.MongoCollection.Find<TEntity>(Builders<TEntity>.Filter.Eq(e => e.Uid, entityId))
                                                   .Project(GetProjectionDefinition<TEntity, TProjection>(false))
                                                   .Limit(1)
                                                   .FirstOrDefaultAsync(token);
            return entity;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TProjection>> GetValueByIdAsync<TProjection>([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            await EnsureInitialized(token);

            var entities = await this.MongoCollection.Find<TEntity>(Builders<TEntity>.Filter.In(e => e.Uid, entityIds))
                                                     .Project(GetProjectionDefinition<TEntity, TProjection>(false))
                                                     .ToListAsync(token);
            return entities;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TEntity>> GetValueByIdAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            await EnsureInitialized(token);

            var entities = await this.MongoCollection.Find<TEntity>(Builders<TEntity>.Filter.In(e => e.Uid, entityIds))
                                                     .ToListAsync(token);
            return entities;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            await EnsureInitialized(token);

            var filter = filterExpression is not null ? Builders<TEntity>.Filter.Where(filterExpression) : Builders<TEntity>.Filter.Empty;
            filter = EnhanceFilter(filter);

            var entities = await this.MongoCollection.Find<TEntity>(filter)
                                                     .Project(GetProjectionDefinition<TEntity, TProjection>(false))
                                                     .ToListAsync(token);
            return entities;
        }

        public async ValueTask<IReadOnlyCollection<TEntity>> GetValuesAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            await EnsureInitialized(token);

            var filter = filterExpression is not null ? Builders<TEntity>.Filter.Where(filterExpression) : Builders<TEntity>.Filter.Empty;
            filter = EnhanceFilter(filter);

            var entities = await this.MongoCollection.Find<TEntity>(filter)
                                                     .ToListAsync(token);
            return entities;
        }

        /// <inheritdoc />
        public async Task<bool> PushDataRecordAsync(TEntity entity, bool insertIfNew, CancellationToken token)
        {
            CheckReadOnly();
            await EnsureInitialized(token);

            var result = await this.MongoCollection.ReplaceOneAsync(Builders<TEntity>.Filter.Eq(e => e.Uid, entity.Uid),
                                                                    entity,
                                                                    new ReplaceOptions() { IsUpsert = insertIfNew },
                                                                    token);

            return (result?.ModifiedCount ?? 0) == 1 || result?.UpsertedId is not null;
        }

        /// <inheritdoc />
        public async Task<int> PushDataRecordAsync(IReadOnlyCollection<TEntity> entities, bool insertIfNew, CancellationToken token)
        {
            CheckReadOnly();
            await EnsureInitialized(token);

            var bulkInsert = entities.Select(e => new ReplaceOneModel<TEntity>(Builders<TEntity>.Filter.Eq(exist => exist.Uid, e.Uid), e))
                                     .OfType<WriteModel<TEntity>>()
                                     .ToArray();

            var result = await this.MongoCollection.BulkWriteAsync(bulkInsert, cancellationToken: token);

            return (int)((result?.ModifiedCount ?? 0) + (result?.InsertedCount ?? 0));
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"[Repository][MONGO] ConfigurationName '{this._configurationName}' => Collection '{this.MongoCollection?.CollectionNamespace?.CollectionName}'";
        }

        #region Tools

        /// <summary>
        /// Enhances the filter by the discriminator
        /// </summary>
        protected virtual FilterDefinition<TEntity>? EnhanceFilter(FilterDefinition<TEntity>? filter = null)
        {
            if (s_discriminatorFilter is not null && this._preventAnyKindOfDiscriminatorUsage == false)
            {
                if (filter is not null)
                    return s_discriminatorFilter & filter;
                return s_discriminatorFilter;
            }
            return filter;
        }

        /// <summary>
        /// Checks if the repository is initialized.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async Task EnsureInitialized(CancellationToken cancellationToken)
        {
            if (this.MongoCollection is not null)
                return;

            await InitializationAsync(this._storageName, cancellationToken);
        }

        /// <summary>
        /// Checks the read only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckReadOnly([CallerMemberName] string? callerName = null)
        {
            if (this.IsReadOnly)
                throw new InvalidOperationException("Repository in readonly access, {0} is not allowed".WithArguments(callerName));
        }

        /// <summary>
        /// Called when initializing asynchronous.
        /// </summary>
        protected override ValueTask OnInitializingAsync(string? storageName, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(this._configurationName);

            this._client = this._mongoClientFactory.Create(this._configurationName);

            if (this._mongoDBOptions == null)
                this._mongoDBOptions = this._serviceProvider.GetKeyedService<IOptions<MongoDBOptions>>(this._configurationName);

            var dbName = this._mongoDBOptions?.Value?.DatabaseName;

            if (string.IsNullOrEmpty(dbName))
                dbName = nameof(Democrite);

            var database = this._client.GetDatabase(dbName);

            var collection = BuildCollectionName(this._mongoDBOptions?.Value?.CollectionPrefix,
                                                 this._configurationName,
                                                 storageName ?? this._storageName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            var settings = new MongoCollectionSettings();
            this._mongoDBOptions?.Value?.CollectionConfigurator?.Invoke(settings);

            this.MongoCollection = database.GetCollection<TEntity>(collection, settings);
            //this.DEBUGMongoCollection = database.GetCollection<BsonDocument>(collection, settings);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        protected virtual string BuildCollectionName(string? collectionPrefix, string configurationName, string storageName)
        {
            return (collectionPrefix ?? string.Empty) + storageName;
        }

        /// <summary>
        /// Gets the projection definition.
        /// </summary>
        protected ProjectionDefinition<TSource, TProjection> GetProjectionDefinition<TSource, TProjection>(bool fromNested)
        {
            var projectTrait = typeof(TProjection);

            s_projectionLocker.EnterReadLock();
            try
            {
                if (s_projectDefinitionCache.TryGetValue(projectTrait, out var projectDoc))
                    return projectDoc;
            }
            finally
            {
                s_projectionLocker.ExitReadLock();
            }

            IEnumerable<ProjectionDefinition<TSource>> projectProp = EnumerableHelper<ProjectionDefinition<TSource>>.ReadOnly;

            bool removeUid = fromNested;

            //if (fromNested)
            //{
            //    removeUid = true;

            //    if (typeof(TSource) == typeof(TP))
            //    projectProp = ((ProjectionDefinition<TSource>)s_idProjection).AsEnumerable();
            //}

            projectProp = projectProp.Concat(projectTrait.GetRuntimeProperties()
                                                         .Where(p => removeUid == false || !string.Equals(p.Name, nameof(IEntityWithId<Guid>.Uid)))
                                                         .Select(prop => Builders<TSource>.Projection.Include(new StringFieldDefinition<TSource>(prop.Name))));

            var project = Builders<TSource>.Projection.Combine(projectProp);

            s_projectionLocker.EnterWriteLock();
            try
            {
                if (s_projectDefinitionCache.TryGetValue(projectTrait, out var projectDoc))
                    return projectDoc;

                s_projectDefinitionCache.Add(projectTrait, project.Render(BsonSerializer.SerializerRegistry.GetSerializer<TSource>(), BsonSerializer.SerializerRegistry));

                return project;
            }
            finally
            {
                s_projectionLocker.ExitWriteLock();
            }
        }

        #endregion

        #endregion
    }
}
