// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Repositories
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using MongoDB.Driver;

    using Orleans.Providers.MongoDB.Utils;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate string BuildCollectionNameDelegate(string? collectionPrefix, string configurationName, string storageName, string originalCollectionName);

    /// <summary>
    /// Mongo repository with a container gestion
    /// </summary>
    internal class MongoContainerRepository<TContainer, TEntity, TEntityId, TContainerId> : MongoBaseRepository<TContainer, TContainerId>, IRepository<TEntity, TEntityId>
            where TEntity : IEntityWithId<TEntityId>
            where TContainer : IEntityWithId<TContainerId>
            where TEntityId : notnull, IEquatable<TEntityId>
            where TContainerId : notnull, IEquatable<TContainerId>
    {
        #region Fields

        private static readonly AggregateOptions s_defaultAggregateOptions;
        private static readonly FilterDefinition<TEntity> s_allFilter;

        private readonly StringFieldDefinition<TContainer, TEntityId> _containedDiscriminatorAccess;
        private readonly FieldDefinition<TContainer, TEntityId> _containedIdAccess;
        private readonly Expression<Func<TContainer, TEntity>> _containedAccess;
        private readonly BuildCollectionNameDelegate _collectionNameDelegate;
        private readonly Func<TEntity, TContainer> _containerFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoContainerRepository{TContainer, TEntity, TEntityId, TContainerId}"/> class.
        /// </summary>
        static MongoContainerRepository()
        {
            s_defaultAggregateOptions = new AggregateOptions()
            {
                BatchSize = 10000,
                AllowDiskUse = true,
                //MaxAwaitTime = TimeSpan.FromSeconds(10)
            };

            s_allFilter = Builders<TEntity>.Filter.Where(e => true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoContainerRepository{TContainer, TEntity, TEntityId}"/> class.
        /// </summary>
        public MongoContainerRepository(IMongoClientFactory mongoClientFactory,
                                        IServiceProvider serviceProvider,
                                        string configurationName,
                                        string storageName,
                                        Expression<Func<TContainer, TEntity>> containedAccess,
                                        Func<TEntity, TContainer> containerFactory,
                                        BuildCollectionNameDelegate collectionNameDelegate,
                                        bool isReadOnly = false,
                                        bool preventAnyKindOfDiscriminatorUsage = false)
            : base(mongoClientFactory, serviceProvider, configurationName, storageName, isReadOnly, preventAnyKindOfDiscriminatorUsage)
        {
            var fieldName = ((MemberExpression)containedAccess.Body).Member.Name;

            this._containedAccess = containedAccess;
            this._containedIdAccess = new StringFieldDefinition<TContainer, TEntityId>(fieldName + "." + nameof(IEntityWithId<TEntityId>.Uid));
            this._containedDiscriminatorAccess = new StringFieldDefinition<TContainer, TEntityId>(fieldName + "._t");
            this._containerFactory = containerFactory;

            this._collectionNameDelegate = collectionNameDelegate;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> DeleteRecordAsync(IReadOnlyCollection<TEntityId> entityIds, CancellationToken cancellationToken)
        {
            var deleteMany = await this.MongoCollection.DeleteManyAsync(Builders<TContainer>.Filter.In(this._containedIdAccess, entityIds), cancellationToken);
            return deleteMany.DeletedCount > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteRecordAsync(TEntityId uid, CancellationToken cancellationToken)
        {
            var deleteMany = await this.MongoCollection.DeleteOneAsync(Builders<TContainer>.Filter.Eq(this._containedIdAccess, uid), cancellationToken);
            return deleteMany.DeletedCount > 0;
        }

        /// <inheritdoc />
        public async ValueTask<TEntity?> GetFirstValueAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            var aggregatePipeline = PrepareAggregatePipeline();

            var entity = await aggregatePipeline.Match(filterExpression ?? s_allFilter)
                                                .Limit(1)
                                                .FirstOrDefaultAsync(token);

            return entity;
        }

        /// <inheritdoc />
        public async ValueTask<TProjection?> GetFirstValueAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            var aggregatePipeline = PrepareAggregatePipeline();

            var entity = await aggregatePipeline.Match(filterExpression ?? s_allFilter)
                                                .Limit(1)
                                                .Project(GetProjectionDefinition<TEntity, TProjection>(true))
                                                .FirstOrDefaultAsync(token);

            return entity;
        }

        /// <inheritdoc />
        public async ValueTask<TEntity?> GetValueByIdAsync([NotNull] TEntityId entityId, CancellationToken token)
        {
            var entity = await this.MongoCollection.Aggregate(s_defaultAggregateOptions)
                                                   .Match(Builders<TContainer>.Filter.Eq(this._containedIdAccess, entityId))
                                                   .Limit(1)
                                                   .ReplaceRoot(this._containedAccess)
                                                   .FirstOrDefaultAsync(token);

            return entity;
        }

        /// <inheritdoc />
        public async ValueTask<TProjection?> GetValueByIdAsync<TProjection>([NotNull] TEntityId entityId, CancellationToken token)
        {
            var entity = await this.MongoCollection.Aggregate(s_defaultAggregateOptions)
                                                   .Match(Builders<TContainer>.Filter.Eq(this._containedIdAccess, entityId))
                                                   .Limit(1)
                                                   .ReplaceRoot(this._containedAccess)
                                                   .Project(GetProjectionDefinition<TEntity, TProjection>(true))
                                                   .FirstOrDefaultAsync(token);

            return entity;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TProjection>> GetValueByIdAsync<TProjection>([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            var entities = await this.MongoCollection.Aggregate(s_defaultAggregateOptions)
                                                     .Match(Builders<TContainer>.Filter.In(this._containedIdAccess, entityIds))
                                                     .ReplaceRoot(this._containedAccess)
                                                     .Project(GetProjectionDefinition<TEntity, TProjection>(true))
                                                     .ToListAsync(token);

            return entities;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TEntity>> GetValueByIdAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            var entities = await this.MongoCollection.Aggregate(s_defaultAggregateOptions)
                                                     .Match(Builders<TContainer>.Filter.In(this._containedIdAccess, entityIds))
                                                     .ReplaceRoot(this._containedAccess)
                                                     .ToListAsync(token);

            return entities;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            var pipeline = PrepareAggregatePipeline();

            var results = await pipeline.Match(filterExpression ?? s_allFilter)
                                        .Project(GetProjectionDefinition<TEntity, TProjection>(true))
                                        .ToListAsync(token);

            return (IReadOnlyCollection<TProjection>)(results ?? EnumerableHelper<TProjection>.ReadOnly);
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TEntity>> GetValuesAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            var pipeline = PrepareAggregatePipeline();

            if (filterExpression is not null)
                pipeline = pipeline.Match(filterExpression);

#if DEBUG
            //var nbCount = await this.MongoCollection.EstimatedDocumentCountAsync();

            //var all = await base.GetValuesAsync(c => c.Uid != null, token);

            ////var map = BsonClassMap<TEntity>.LookupClassMap(typeof(TEntity));
            //var str = pipeline.ToString();

            //var docs = pipeline.Stages.Select(s => BsonDocument.Parse(s.ToString())).ToArray();

            //var pipelineDocs = PipelineDefinition<BsonDocument, BsonDocument>.Create(docs);
            //var items = await (await this.DEBUGMongoCollection.AggregateAsync<BsonDocument>(pipelineDocs)).ToListAsync(token);
            //      var docResultsCursor = await pipeline.ToListAsync(token);
#endif

            var results = await pipeline.ToListAsync(token);

            return (IReadOnlyCollection<TEntity>)(results ?? EnumerableHelper<TEntity>.ReadOnly);
        }

        /// <inheritdoc />
        public async Task<bool> PushDataRecordAsync(TEntity entity, bool insertIfNew, CancellationToken token)
        {
            var container = this._containerFactory(entity);
            return await base.PushDataRecordAsync(container, insertIfNew, token);
        }

        /// <inheritdoc />
        public async Task<int> PushDataRecordAsync(IReadOnlyCollection<TEntity> entities, bool insertIfNew, CancellationToken token)
        {
            var containers = entities.Select(e => this._containerFactory(e))
                                     .ToArray();

            return await base.PushDataRecordAsync(containers, insertIfNew, token);
        }

        #region Tools

        /// <inheritdoc />
        protected override string BuildCollectionName(string? collectionPrefix, string configurationName, string storageName)
        {
            var classicName = base.BuildCollectionName(collectionPrefix, configurationName, storageName);

            //if (typeof(TContainer).IsAssignableTo(typeof(GrainStateContainer<TEntity>)))
            //{
            //    classicName = "Grains" + classicName;
            //}

            classicName = this._collectionNameDelegate(collectionPrefix, configurationName, storageName, classicName);
            return classicName;
        }

        /// <summary>
        /// Prepares the aggregate pipeline to managed change from <see cref="TContainer"/> <see cref="TEntity"/>
        /// </summary>
        private IAggregateFluent<TEntity> PrepareAggregatePipeline()
        {
            var aggr = this.MongoCollection.Aggregate(s_defaultAggregateOptions);

            var filter = base.EnhanceFilter();

            if (filter is not null)
                aggr = aggr.Match(filter);

            return aggr.ReplaceRoot(this._containedAccess);
        }

        /// <inheritdoc />
        protected override async ValueTask OnInitializingAsync(string? storageName, CancellationToken token)
        {
            await base.OnInitializingAsync(storageName, token);

            // Add specific search index related to optimize the state search

            var indexes = await this.MongoCollection.Indexes.List().ToListAsync();

            var indexedByName = indexes.GroupBy(i => i.GetElement("name").Value.AsString)
                                       .ToImmutableDictionary(k => k.Key);

            if (!indexedByName.ContainsKey("auto_type_uid"))
            {
                var indexDescription = Builders<TContainer>.IndexKeys.Combine(Builders<TContainer>.IndexKeys.Ascending(this._containedDiscriminatorAccess),
                                                                              Builders<TContainer>.IndexKeys.Ascending(this._containedIdAccess));

                await this.MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<TContainer>(indexDescription,
                                                                                                   new CreateIndexOptions()
                                                                                                   {
                                                                                                       Background = true,
                                                                                                       Name = "auto_type_uid"
                                                                                                   }));
            }

            if (!indexedByName.ContainsKey("auto_uid"))
            {
                var indexDescription = Builders<TContainer>.IndexKeys.Ascending(this._containedIdAccess);

                await this.MongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<TContainer>(indexDescription,
                                                                                                   new CreateIndexOptions()
                                                                                                   {
                                                                                                       Background = true,
                                                                                                       Name = "auto_uid"
                                                                                                   }));
            }
        }

        #endregion

        #endregion
    }

    internal sealed class MongoReadonlyContainerRepository<TContainer, TEntity, TEntityId, TContainerId> : MongoContainerRepository<TContainer, TEntity, TEntityId, TContainerId>
        where TEntity : IEntityWithId<TEntityId>
        where TContainer : IEntityWithId<TContainerId>
        where TEntityId : notnull, IEquatable<TEntityId>
        where TContainerId : notnull, IEquatable<TContainerId>
    {
        public MongoReadonlyContainerRepository(IMongoClientFactory mongoClientFactory,
                                                IServiceProvider serviceProvider,
                                                string configurationName,
                                                string storageName,
                                                Expression<Func<TContainer, TEntity>> containedAccess,
                                                Func<TEntity, TContainer> containerFactory,
                                                BuildCollectionNameDelegate collectionNameDelegate,
                                                bool preventAnyKindOfDiscriminatorUsage)
            : base(mongoClientFactory, serviceProvider, configurationName, storageName, containedAccess, containerFactory, collectionNameDelegate, true, preventAnyKindOfDiscriminatorUsage)
        {
        }
    }
}
