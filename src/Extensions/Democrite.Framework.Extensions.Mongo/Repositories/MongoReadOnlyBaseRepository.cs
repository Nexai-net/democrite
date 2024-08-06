// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Repositories
{
    //using Democrite.Framework.Core.Abstractions.Repositories;

    //using Elvex.Toolbox.Abstractions.Supports;
    //using Elvex.Toolbox.Supports;

    //using Microsoft.Extensions.DependencyInjection;
    //using Microsoft.Extensions.Options;

    //using MongoDB.Bson;
    //using MongoDB.Bson.Serialization;
    //using MongoDB.Driver;

    //using Orleans.Providers.MongoDB.Configuration;
    //using Orleans.Providers.MongoDB.Utils;

    //using System;
    //using System.Collections.Generic;
    //using System.Diagnostics.CodeAnalysis;
    //using System.Linq.Expressions;
    //using System.Reflection;
    //using System.Threading;
    //using System.Threading.Tasks;

    ///// <summary>
    ///// Base repository with read funcions managed container
    ///// </summary>
    //public abstract class MongoReadOnlyBaseRepository<TContainer, TEntity> : SupportBaseInitialization<string>, IReadOnlyRepository<TEntity>
    //{
    //    #region Fields

    //    private static readonly Type s_entityTrait = typeof(TEntity);
    //    private static readonly Type s_containerTrait = typeof(TContainer);

    //    private static readonly Dictionary<Type, BsonDocument> s_projectDefinitionCache;
    //    private static readonly ReaderWriterLockSlim s_projectionLocker;
    //    private static readonly BsonDocument s_idProjection;

    //    private static readonly AggregateOptions s_defaultAggregateOptions;
    //    private static readonly bool s_noContainer;

    //    private readonly IMongoClientFactory _mongoClientFactory;
    //    private readonly IServiceProvider _serviceProvider;
    //    private readonly string? _collectionName;
    //    private IOptions<MongoDBOptions>? _mongoDBOptions;

    //    private IMongoCollection<TContainer>? _collection;
    //    private IMongoClient? _client;

    //    #endregion

    //    #region Ctor

    //    /// <summary>
    //    /// Initializes the <see cref="MongoReadOnlyBaseRepository{TContainer, TEntity, TEntityId}"/> class.
    //    /// </summary>
    //    static MongoReadOnlyBaseRepository()
    //    {
    //        s_noContainer = typeof(TContainer) == typeof(TEntity);
    //        s_defaultAggregateOptions = new AggregateOptions()
    //        {
    //            BatchSize = 10000,
    //            AllowDiskUse = true,
    //            //MaxAwaitTime = TimeSpan.FromSeconds(10)
    //        };

    //        s_projectionLocker = new ReaderWriterLockSlim();
    //        s_projectDefinitionCache = new Dictionary<Type, BsonDocument>();

    //        // Projection between EntityId and mongo _id
    //        s_idProjection = new BsonDocument()
    //        {
    //            { "_id", new BsonDocument()  {
    //                { "$ifNull", new  BsonArray() {
    //                    "$_id",
    //                    "$" + nameof(IEntityWithId<Guid>.Uid)
    //                } }
    //            } }
    //        };
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ReadOnlyBaseRepository{TEntity}"/> class.
    //    /// </summary>
    //    protected MongoReadOnlyBaseRepository(IMongoClientFactory mongoClientFactory,
    //                                          IServiceProvider serviceProvider,
    //                                          string? collectionName = null,
    //                                          IOptions<MongoDBOptions>? mongoDBOptions = null)
    //    {
    //        if (s_entityTrait.GetTypeInfoExtension().IsCSharpScalarType)
    //            throw new InvalidDataException("Direct scalar type as state is not managed by this kind of repository.");

    //        this._mongoClientFactory = mongoClientFactory;
    //        this._mongoDBOptions = mongoDBOptions;
    //        this._serviceProvider = serviceProvider;

    //        this._collectionName = collectionName;

    //        if (string.IsNullOrEmpty(collectionName))
    //            this._collectionName = typeof(TEntity).Name;
    //    }

    //    #endregion

    //    #region Methods

    //    #region Entity

    //    /// <inheritdoc />
    //    public async ValueTask<TEntity?> GetFirstValueAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
    //    {
    //        var entityPipeline = PrepareAggregatePipeline();

    //        return await AppendMatchFromExpressionFilter(entityPipeline, filterExpression)
    //                                   .Limit(1)
    //                                   .FirstOrDefaultAsync(token);
    //    }

    //    /// <inheritdoc />
    //    public async ValueTask<TProjection?> GetFirstValueAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
    //    {
    //        var entityPipeline = PrepareAggregatePipeline();

    //        return await AppendMatchFromExpressionFilter(entityPipeline, filterExpression)
    //                                   .Limit(1)
    //                                   .Project(GetProjectionDefinition<TEntity, TProjection>())
    //                                   .FirstOrDefaultAsync(token);
    //    }

    //    /// <inheritdoc />
    //    public async ValueTask<IReadOnlyCollection<TEntity>> GetValuesAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
    //    {
    //        var entityPipeline = PrepareAggregatePipeline();

    //        var query = AppendMatchFromExpressionFilter(entityPipeline, filterExpression);

    //        var str = query.ToString();

    //        var results = await query.ToListAsync(token);

    //        return results;
    //    }

    //    /// <inheritdoc />
    //    public async ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
    //    {
    //        var entityPipeline = PrepareAggregatePipeline();

    //        return await AppendMatchFromExpressionFilter(entityPipeline, filterExpression)
    //                                   .Project(GetProjectionDefinition<TEntity, TProjection>())
    //                                   .ToListAsync(token);
    //    }

    //    /// <inheritdoc />
    //    public async ValueTask<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken token)
    //    {
    //        var entityPipeline = PrepareAggregatePipeline();

    //        return await entityPipeline.ToListAsync(token);
    //    }

    //    /// <inheritdoc />
    //    public async ValueTask<IReadOnlyCollection<TProjection>> GetAllAsync<TProjection>(CancellationToken token)
    //    {
    //        var entityPipeline = PrepareAggregatePipeline();

    //        return await entityPipeline.Project(GetProjectionDefinition<TEntity, TProjection>())
    //                                   .ToListAsync(token);
    //    }

    //    #endregion

    //    #region Containers

    //    /// <inheritdoc />
    //    public async ValueTask<TContainer?> GetFirstContainerAsync(Expression<Func<TContainer, bool>>? filterExpression, CancellationToken token)
    //    {
    //        var containerPipeline = CreateContainerSearchPipeline();
    //        return await AppendMatchFromExpressionFilter(containerPipeline, filterExpression)
    //                                      .Limit(1)
    //                                      .FirstOrDefaultAsync(token);
    //    }

    //    /// <inheritdoc />
    //    public async ValueTask<IReadOnlyCollection<TContainer>> GetContainersAsync(Expression<Func<TContainer, bool>>? filterExpression, CancellationToken token)
    //    {
    //        var containerPipeline = CreateContainerSearchPipeline();
    //        return await AppendMatchFromExpressionFilter(containerPipeline, filterExpression).ToListAsync();
    //    }

    //    /// <inheritdoc />
    //    public async ValueTask<TProjection?> GetFirstContainerAsync<TProjection>(Expression<Func<TContainer, bool>>? filterExpression, CancellationToken token)
    //    {
    //        var containerPipeline = CreateContainerSearchPipeline();
    //        return await AppendMatchFromExpressionFilter(containerPipeline, filterExpression)
    //                                      .Limit(1)
    //                                      .Project(GetProjectionDefinition<TContainer, TProjection>())
    //                                      .FirstOrDefaultAsync(token);
    //    }

    //    /// <inheritdoc />
    //    public async ValueTask<IReadOnlyCollection<TProjection>> GetContainersAsync<TProjection>(Expression<Func<TContainer, bool>>? filterExpression, CancellationToken token)
    //    {
    //        var containerPipeline = CreateContainerSearchPipeline();
    //        return await AppendMatchFromExpressionFilter(containerPipeline, filterExpression)
    //                                      .Project(GetProjectionDefinition<TContainer, TProjection>())
    //                                      .ToListAsync();
    //    }

    //    /// <inheritdoc />
    //    protected async ValueTask<IReadOnlyCollection<TContainer?>> GetAllContainersAsync(CancellationToken token)
    //    {
    //        var containerPipeline = CreateContainerSearchPipeline();
    //        return await containerPipeline.ToListAsync(token);
    //    }

    //    /// <inheritdoc />
    //    protected async ValueTask<IReadOnlyCollection<TProjection?>> GetAllContainersAsync<TProjection>(CancellationToken token)
    //    {
    //        var containerPipeline = CreateContainerSearchPipeline();
    //        return await containerPipeline.Project(GetProjectionDefinition<TContainer, TProjection>())
    //                                      .ToListAsync(token);
    //    }

    //    #endregion

    //    /// <summary>
    //    ///     Expect configuration key pass in argument<br />
    //    ///     Example : it used to get the <see cref="IMongoClient"/> from <see cref="IMongoClientFactory"/>
    //    /// </summary>
    //    /// <inheritdoc cref="ISupportInitialization.InitializationAsync{TState}(TState?, CancellationToken)" />
    //    protected override ValueTask OnInitializingAsync(string? configurationKey, CancellationToken token)
    //    {
    //        ArgumentNullException.ThrowIfNull(configurationKey);

    //        this._client = this._mongoClientFactory.Create(configurationKey);

    //        if (this._mongoDBOptions == null)
    //            this._mongoDBOptions = this._serviceProvider.GetKeyedService<IOptions<MongoDBOptions>>(configurationKey);

    //        var dbName = this._mongoDBOptions?.Value?.DatabaseName;

    //        if (string.IsNullOrEmpty(dbName))
    //            dbName = nameof(Democrite);

    //        var database = this._client.GetDatabase(dbName);

    //        var collection = BuildCollectionName(this._mongoDBOptions?.Value?.CollectionPrefix, this._collectionName);

    //        var settings = new MongoCollectionSettings();
    //        this._mongoDBOptions?.Value?.CollectionConfigurator?.Invoke(settings);

    //        this._collection = database.GetCollection<TContainer>(collection, settings);

    //        return ValueTask.CompletedTask;
    //    }

    //    #region Tools

    //    /// <summary>
    //    /// Use to reduce the subset look (Discrimination, ...)
    //    /// </summary>
    //    protected abstract FilterDefinition<TContainer>? PreEntityFilter();

    //    /// <summary>
    //    /// Define how to access searched entity contained
    //    /// </summary>
    //    protected abstract AggregateExpressionDefinition<TContainer, TEntity>? HowToGoToStoredEntity();

    //    /// <summary>
    //    /// Gets the aggregate options.
    //    /// </summary>
    //    protected virtual AggregateOptions GetAggregateOptions()
    //    {
    //        return s_defaultAggregateOptions;
    //    }

    //    /// <summary>
    //    /// Prepares the aggregate pipeline to managed change from <see cref="TContainer"/> <see cref="TEntity"/>
    //    /// </summary>
    //    private IAggregateFluent<TEntity> PrepareAggregatePipeline()
    //    {
    //        var pipelineBuilder = CreateContainerSearchPipeline();

    //        IAggregateFluent<TEntity>? entityPipeline = null;

    //        if (s_noContainer)
    //            entityPipeline = (IAggregateFluent<TEntity>)pipelineBuilder;

    //        var newRoot = HowToGoToStoredEntity();
    //        if (newRoot != null)
    //            entityPipeline = pipelineBuilder.ReplaceRoot(newRoot);

    //        if (entityPipeline is null)
    //            throw new InvalidOperationException("New to redirect search to contains type");

    //        return entityPipeline;
    //    }

    //    /// <summary>
    //    /// Creates the container search pipeline.
    //    /// </summary>
    //    private IAggregateFluent<TContainer> CreateContainerSearchPipeline()
    //    {
    //        var pipelineBuilder = this._collection.Aggregate(GetAggregateOptions());

    //        var prefilter = PreEntityFilter();

    //        if (prefilter != null)
    //            pipelineBuilder = pipelineBuilder.Match(prefilter);
    //        return pipelineBuilder;
    //    }

    //    /// <summary>
    //    /// Gets the projection definition.
    //    /// </summary>
    //    protected ProjectionDefinition<TSource, TProjection> GetProjectionDefinition<TSource, TProjection>()
    //    {
    //        var projectTrait = typeof(TProjection);

    //        s_projectionLocker.EnterReadLock();
    //        try
    //        {
    //            if (s_projectDefinitionCache.TryGetValue(projectTrait, out var projectDoc))
    //                return projectDoc;
    //        }
    //        finally
    //        {
    //            s_projectionLocker.ExitReadLock();
    //        }

    //        IEnumerable<ProjectionDefinition<TSource>> projectProp = EnumerableHelper<ProjectionDefinition<TSource>>.ReadOnly;

    //        bool removeUid = false;

    //        if (typeof(TContainer) != typeof(TEntity))
    //        {
    //            var entityIdInterface = projectTrait.GetTypeInfoExtension()
    //                                                .GetAllCompatibleTypes()
    //                                                .FirstOrDefault(i => i.IsInterface &&
    //                                                                     i.IsGenericType &&
    //                                                                     i.GetGenericTypeDefinition() == typeof(IEntityWithId<>));

    //            if (entityIdInterface is not null)
    //            {
    //                removeUid = true;
    //                projectProp = ((ProjectionDefinition<TSource>)s_idProjection).AsEnumerable();
    //            }
    //        }

    //        projectProp = projectProp.Concat(projectTrait.GetRuntimeProperties()
    //                                                     .Where(p => removeUid == false || !string.Equals(p.Name, nameof(IEntityWithId<Guid>.Uid)))
    //                                                     .Select(prop => Builders<TSource>.Projection.Include(new StringFieldDefinition<TSource>(prop.Name))));

    //        var project = Builders<TSource>.Projection.Combine(projectProp);

    //        s_projectionLocker.EnterWriteLock();
    //        try
    //        {
    //            if (s_projectDefinitionCache.TryGetValue(projectTrait, out var projectDoc))
    //                return projectDoc;

    //            s_projectDefinitionCache.Add(projectTrait, project.Render(BsonSerializer.SerializerRegistry.GetSerializer<TSource>(), BsonSerializer.SerializerRegistry));

    //            return project;
    //        }
    //        finally
    //        {
    //            s_projectionLocker.ExitWriteLock();
    //        }
    //    }

    //    /// <summary>
    //    /// Builds the name of the collection.
    //    /// </summary>
    //    protected virtual string BuildCollectionName(string? collectionPrefix, string? collectionName)
    //    {
    //        return collectionPrefix + collectionName;
    //    }

    //    /// <summary>
    //    /// Appends the match from expression filter if not null
    //    /// </summary>
    //    private IAggregateFluent<TFilterEntity> AppendMatchFromExpressionFilter<TFilterEntity>(IAggregateFluent<TFilterEntity> pipeline,
    //                                                                                           [AllowNull] Expression<Func<TFilterEntity, bool>> filterExpression)
    //    {
    //        if (filterExpression is null)
    //            return pipeline;

    //        return pipeline.Match(filterExpression);
    //    }

    //    /// <summary>
    //    /// Gets the mongo collection.
    //    /// </summary>
    //    protected IMongoCollection<TContainer> GetMongoCollection()
    //    {
    //        if (!this.IsInitialized)
    //            throw new InvalidDataException("A mongo repository MUST be first initialized");

    //        return this._collection!;
    //    }

    //    #endregion

    //    #endregion
    //}

    ///// <summary>
    ///// Base repository with read funcions with no container
    ///// </summary>
    //public abstract class MongoReadOnlyBaseRepository<TEntity> : MongoReadOnlyBaseRepository<TEntity, TEntity>
    //{
    //    #region Ctor

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="MongoReadOnlyBaseRepository{TEntity, TEntityId}"/> class.
    //    /// </summary>
    //    protected MongoReadOnlyBaseRepository(IMongoClientFactory mongoClientFactory,
    //                                          IServiceProvider serviceProvider,
    //                                          string? collectionName = null,
    //                                          IOptions<MongoDBOptions>? mongoDBOptions = null)
    //        : base(mongoClientFactory, serviceProvider, collectionName, mongoDBOptions)
    //    {
    //    }

    //    #endregion

    //    #region Methods

    //    /// <inheritdoc/>
    //    protected override AggregateExpressionDefinition<TEntity, TEntity>? HowToGoToStoredEntity()
    //    {
    //        throw new NotSupportedException("No container this method MUST never be called");
    //    }

    //    #endregion
    //}
}
