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
    [DebuggerDisplay("MONGO State Repository {s_entityTraits} : {_storageName}")]
    internal class MongoStateStorageReadRepository<TEntity> : MongoReadOnlyBaseRepository<GrainStateContainer<TEntity>, TEntity>
    {
        #region Fields

        private static readonly Type s_entityTraits = typeof(TEntity);

        private static readonly AggregateExpressionDefinition<GrainStateContainer<TEntity>, TEntity> s_newRootEntity;
        private static readonly FilterDefinition<GrainStateContainer<TEntity>> s_existFilter;
        
        private readonly string _storageName;
        private readonly string _stateName;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoStateStorageReadRepository{TEntity}"/> class.
        /// </summary>
        static MongoStateStorageReadRepository()
        {
            s_existFilter = Builders<GrainStateContainer<TEntity>>.Filter.Exists(f => f.State) &
                            Builders<GrainStateContainer<TEntity>>.Filter.Ne(new StringFieldDefinition<GrainStateContainer<TEntity>, BsonNull>(nameof(GrainStateContainer<int>.State)), BsonNull.Value);
#pragma warning disable CS8603 // Possible null reference return.
            s_newRootEntity = new ExpressionAggregateExpressionDefinition<GrainStateContainer<TEntity>, TEntity>(c => c.State, new ExpressionTranslationOptions() { StringTranslationMode = AggregateStringTranslationMode.Bytes });
#pragma warning restore CS8603 // Possible null reference return.

            BsonClassMap.TryRegisterClassMap<GrainState<TEntity>>();
            BsonClassMap.TryRegisterClassMap<TEntity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoStateStorageReadRepository{TEntity}"/> class.
        /// </summary>
        public MongoStateStorageReadRepository(string stateName,
                                               string storageName,
                                               IMongoClientFactory mongoClientFactory,
                                               IServiceProvider serviceProvider,
                                               IOptions<MongoDBOptions>? mongoDBOptions = null) 
            : base(mongoClientFactory, serviceProvider, stateName, mongoDBOptions)
        {
            this._stateName = stateName;
            this._storageName = storageName;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override AggregateExpressionDefinition<GrainStateContainer<TEntity>, TEntity>? HowToGoToStoredEntity()
        {
            return s_newRootEntity;
        }

        /// <inheritdoc />
        protected override FilterDefinition<GrainStateContainer<TEntity>>? PreEntityFilter()
        {
            return s_existFilter;
        }

        /// <inheritdoc />
        protected override string BuildCollectionName(string? collectionPrefix, string? collectionName)
        {
            if (string.IsNullOrEmpty(collectionPrefix))
                collectionPrefix = "Grains";

            if (string.Equals(this._stateName, "state", StringComparison.OrdinalIgnoreCase))
                collectionName = typeof(TEntity).Name;

            if (string.IsNullOrEmpty(collectionName))
                collectionName = this._stateName;

            return base.BuildCollectionName(collectionPrefix, collectionName);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken token)
        {
            return await base.GetValuesAsync(filter, token);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TEntity>> FindAllAsync(CancellationToken token)
        {
            return await base.GetValuesAsync((TEntity _) => true, token);
        }

        #endregion
    }

    [DebuggerDisplay("MONGO State Repository {s_entityTraits}[{s_entityIdTraits}] : {_storageName}")]
    internal sealed class MongoStorageReadRepository<TEntity, TEntityId> : MongoStateStorageReadRepository<TEntity>, IReadOnlyRepository<TEntity, TEntityId>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        #region Fields

        private static readonly Type s_entityIdTraits = typeof(TEntityId);

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoStorageReadRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        public MongoStorageReadRepository(string stateName,
                                          string storageName,
                                          IMongoClientFactory mongoClientFactory,
                                          IServiceProvider serviceProvider,
                                          IOptions<MongoDBOptions>? mongoDBOptions = null)
            : base(stateName, storageName, mongoClientFactory, serviceProvider, mongoDBOptions)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TEntity>> GetByIdsValueAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            return await base.GetValuesAsync((TEntity g) => entityIds.Contains(g.Uid), token);
        }

        /// <inheritdoc />
        public async ValueTask<TEntity?> GetByIdValueAsync([NotNull] TEntityId entityId, CancellationToken token)
        {
            return await base.GetFirstValueAsync((TEntity g) => g.Uid.Equals(entityId), token);
        }

        #endregion
    }
}
