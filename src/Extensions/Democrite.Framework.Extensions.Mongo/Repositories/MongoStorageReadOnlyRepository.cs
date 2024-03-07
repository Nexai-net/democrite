// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Repositories
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Extensions.Mongo.Models;

    using Microsoft.Extensions.Options;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;
    using MongoDB.Driver.Core.Misc;

    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.Utils;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Core.Abstractions.Storages.IStorageReadRepository" />
    [DebuggerDisplay("MONGO ReadOnly Repository {s_entityTraits} : {_storageName}")]
    internal class MongoStorageReadOnlyRepository<TEntity> : MongoReadOnlyBaseRepository<TEntity>
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoStorageReadOnlyRepository{TEntity}"/> class.
        /// </summary>
        static MongoStorageReadOnlyRepository()
        {
            BsonClassMap.TryRegisterClassMap<TEntity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoStorageReadOnlyRepository{TEntity}"/> class.
        /// </summary>
        public MongoStorageReadOnlyRepository(IMongoClientFactory mongoClientFactory,
                                      IServiceProvider serviceProvider,
                                      string? collectionName = null,
                                      IOptions<MongoDBOptions>? mongoDBOptions = null) 
            : base(mongoClientFactory, serviceProvider, collectionName, mongoDBOptions)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override FilterDefinition<TEntity>? PreEntityFilter()
        {
            return null;
        }

        /// <inheritdoc />
        protected override AggregateExpressionDefinition<TEntity, TEntity>? HowToGoToStoredEntity()
        {
            return null;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Core.Abstractions.Storages.IStorageReadRepository" />
    [DebuggerDisplay("MONGO Repository {s_entityTraits} : {_storageName}")]
    internal class MongoStorageRepository<TEntity, TEntityId> : MongoStorageReadOnlyRepository<TEntity>, IRepository<TEntity>, IRepository<TEntity, TEntityId>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoStorageRepository{TEntity}"/> class.
        /// </summary>
        public MongoStorageRepository(IMongoClientFactory mongoClientFactory,
                                      IServiceProvider serviceProvider,
                                      string? collectionName = null,
                                      IOptions<MongoDBOptions>? mongoDBOptions = null)
            : base(mongoClientFactory, serviceProvider, collectionName, mongoDBOptions)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override FilterDefinition<TEntity>? PreEntityFilter()
        {
            return null;
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<TEntity>> GetByIdsValueAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            return base.GetValuesAsync((TEntity e) => entityIds.Contains(e.Uid), token);
        }

        /// <inheritdoc />
        public ValueTask<TEntity?> GetByIdValueAsync([NotNull] TEntityId entityId, CancellationToken token)
        {
            return base.GetFirstValueAsync(e => EqualityComparer<TEntityId>.Default.Equals(e.Uid, entityId), token);
        }

        /// <inheritdoc />
        public async Task<bool> PushDataRecordAsync(TEntity entity, bool insertIfNew, CancellationToken token)
        {
            var result = await this.GetMongoCollection().ReplaceOneAsync(Builders<TEntity>.Filter.Eq(e => e.Uid, entity.Uid), 
                                                                         entity, 
                                                                         new ReplaceOptions() { IsUpsert = insertIfNew }, 
                                                                         token);

            return (result?.ModifiedCount ?? 0) == 1 || result?.UpsertedId is not null;
        }

        /// <inheritdoc />
        public Task<bool> DeleteRecordAsync(TEntityId uid, CancellationToken cancellationToken)
        {
            return DeleteRecordsAsync(cancellationToken, uid.AsEnumerable());
        }

        /// <inheritdoc />
        public async Task<bool> DeleteRecordsAsync(CancellationToken cancellationToken, IEnumerable<TEntityId> uids)
        {
            var result = await this.GetMongoCollection().DeleteManyAsync(e => uids.Contains(e.Uid), cancellationToken);
            return result.DeletedCount != 0;
        }

        #region Tools

        /// <inheritdoc />
        protected override AggregateExpressionDefinition<TEntity, TEntity>? HowToGoToStoredEntity()
        {
            return null;
        }

        #endregion

        #endregion
    }

    [DebuggerDisplay("MONGO ReadOnly Repository {s_entityTraits}[{s_entityIdTraits}] : {_storageName}")]
    internal sealed class MongoStorageReadOnlyRepository<TEntity, TEntityId> : MongoStorageReadOnlyRepository<TEntity>, IReadOnlyRepository<TEntity, TEntityId>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoStorageReadOnlyRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        public MongoStorageReadOnlyRepository(IMongoClientFactory mongoClientFactory,
                                      IServiceProvider serviceProvider,
                                      string? collectionName = null,
                                      IOptions<MongoDBOptions>? mongoDBOptions = null) 
            : base(mongoClientFactory, serviceProvider, collectionName, mongoDBOptions)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<TEntity>> GetByIdsValueAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            return base.GetValuesAsync((TEntity e) => entityIds.Contains(e.Uid), token);
        }

        /// <inheritdoc />
        public ValueTask<TEntity?> GetByIdValueAsync([NotNull] TEntityId entityId, CancellationToken token)
        {
            return base.GetFirstValueAsync(e => EqualityComparer<TEntityId>.Default.Equals(e.Uid, entityId), token);
        }

        #endregion
    }
}
