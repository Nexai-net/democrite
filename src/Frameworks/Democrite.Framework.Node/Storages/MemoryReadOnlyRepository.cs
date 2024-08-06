// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;

    using Newtonsoft.Json;

    using Orleans.Concurrency;
    using Orleans.Storage;

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    //    /// <summary>
    //    /// Repository used to navigate through cluster grain state stored in memory
    //    /// </summary>
    //    /// <remarks>
    //    ///     Memory storage on the cluster is efficient to access using _stateName and StorageName keys otherwise we have to get all the information
    //    ///     to filtered it is not efficient to use with knowledge of performance issue.
    //    /// </remarks>
    //    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    //    /// <seealso cref="IStorageReadRepository{TEntity}" />
    //    [DebuggerDisplay("Memory Repository {s_entityTraits} : {_configurationName}")]
    //    internal class MemoryReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
    //    {
    //        #region Fields

    //        private static readonly AbstractType s_entityAbstractTraits = typeof(TEntity).GetAbstractType();
    //        private static readonly Type s_entityTraits = typeof(TEntity);
    //        private static readonly Func<TEntity, bool> s_allTrue;

    //        #endregion

    //        #region Ctor

    //        /// <summary>
    //        /// Initializes the <see cref="MemoryReadOnlyGrainStateRepository{TEntity}"/> class.
    //        /// </summary>
    //        static MemoryReadOnlyRepository()
    //        {
    //            Expression<Func<TEntity, bool>>? filter = _ => true;
    //            s_allTrue = filter.Compile();
    //        }

    //        /// <summary>
    //        /// Initializes a new instance of the <see cref="MemoryReadOnlyRepository{TEntity}"/> class.
    //        /// </summary>
    //        public MemoryReadOnlyRepository(string stateName,
    //                                        string storageName,
    //                                        IGrainFactory grainFactory,
    //                                        IDemocriteSerializer democriteSerializer)
    //        {
    //            this.StorageName = storageName;
    //            this.DemocriteSerializer = democriteSerializer;

    //            this.StateName = stateName;
    //            this.RegistryGrain = grainFactory.GetGrain<IMemoryStorageRegistryGrainMaster>(storageName);
    //        }

    //        #endregion

    //        #region Properties

    //        /// <summary>
    //        /// Gets the name of the storage configuration.
    //        /// </summary>
    //        public string StorageName { get; }

    //        /// <summary>
    //        /// Gets the name of the state.
    //        /// </summary>
    //        public string StateName { get; }

    //        /// <summary>
    //        /// Gets the registry grain.
    //        /// </summary>
    //        public IMemoryStorageRegistryGrainMaster RegistryGrain { get; }

    //        /// <summary>
    //        /// Gets the grain storage serializer.
    //        /// </summary>
    //        public IDemocriteSerializer DemocriteSerializer { get; }

    //        #endregion

    //        #region Methods

    //        /// <inheritdoc />
    //        [ReadOnly]
    //        public async ValueTask<IReadOnlyCollection<TEntity>> GetValuesAsync(Expression<Func<TEntity, bool>>? filter, CancellationToken token)
    //        {
    //            var entities = await GetAllImplAsync(filter, token);

    //            return entities?.ToArray() ?? EnumerableHelper<TEntity>.ReadOnlyArray;
    //        }

    //        /// <inheritdoc />
    //        [ReadOnly]
    //        public async ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>(Expression<Func<TEntity, bool>>? filterExpression, CancellationToken token)
    //        {
    //            var results = await GetValuesAsync(filterExpression, token);

    //            return results?.Select(r => Map<TProjection, TEntity>(r))
    //                           .Where(r => r is not null)
    //                           .Select(r => r!)
    //                           .ToArray() ?? EnumerableHelper<TProjection>.ReadOnlyArray;
    //        }

    //        /// <inheritdoc />
    //        [ReadOnly]
    //        public async ValueTask<TEntity?> GetFirstValueAsync(Expression<Func<TEntity, bool>>? filter, CancellationToken token)
    //        {
    //            var entities = await GetAllImplAsync(filter, token);

    //            return entities.FirstOrDefault();
    //        }

    //        /// <inheritdoc />
    //        [ReadOnly]
    //        public async ValueTask<TProjection?> GetFirstValueAsync<TProjection>(Expression<Func<TEntity, bool>>? filterExpression, CancellationToken token)
    //        {
    //            var result = await GetFirstValueAsync(filterExpression, token);
    //            return Map<TProjection, TEntity>(result);
    //        }

    //        #region Tools

    //        /// <summary>
    //        /// Download all compatible values and return a enumeration that could be filtered
    //        /// </summary>
    //        protected async Task<IEnumerable<TEntity>> GetAllImplAsync(Expression<Func<TEntity, bool>>? filterExpression, CancellationToken token)
    //        {
    //            using (var cancelSource = new GrainCancellationTokenSource())
    //            {
    //                token.Register(() => cancelSource.Cancel().ConfigureAwait(false));

    //                var filter = filterExpression?.Compile() ?? s_allTrue;

    //                var entitiesByBytes = await this.RegistryGrain.GetAllStoreDataAsync(this.StateName, s_entityAbstractTraits, cancelSource.Token);

    //                return entitiesByBytes?.Select(e => this.DemocriteSerializer.Deserialize<TEntity>(e))
    //                                       .Where(filter) ?? EnumerableHelper<TEntity>.ReadOnly;
    //            }
    //        }

    //        /// <summary>
    //        /// Download all compatible values and return a enumeration that could be filtered
    //        /// </summary>
    //        protected Task<IEnumerable<TEntity>?> GetAllImplByKeysAsync<TKey>(CancellationToken token, params TKey[] keys)
    //            where TKey : notnull, IEquatable<TKey>
    //        {
    //            return GetAllImplByKeysAsync<TKey>(keys, token);
    //        }

    //        /// <summary>
    //        /// Download all compatible values and return a enumeration that could be filtered
    //        /// </summary>
    //        protected async Task<IEnumerable<TEntity>?> GetAllImplByKeysAsync<TKey>(IReadOnlyCollection<TKey> keys, CancellationToken token)
    //            where TKey : notnull, IEquatable<TKey>
    //        {
    //            using (var cancelSource = new GrainCancellationTokenSource())
    //            {
    //                token.Register(() => cancelSource.Cancel().ConfigureAwait(false));

    //                // TODO : Add state filter
    //                var entitiesByBytes = await this.RegistryGrain.GetAllStoreByKeysDataAsync<TKey>(this.StateName, keys, cancelSource.Token);

    //                return entitiesByBytes?.Select(e => this.DemocriteSerializer.Deserialize<TEntity>(e)) ?? EnumerableHelper<TEntity>.ReadOnly;
    //            }
    //        }

    //        /// <summary>
    //        /// Maps <typeparamref name="TSource"/> to <typeparamref name="TProjection"/>
    //        /// </summary>
    //        private TProjection? Map<TProjection, TSource>(in TSource? source)
    //{
    //    if (EqualityComparer<TSource>.Default.Equals(source, default))
    //        return default;

    //    // OPTI : Usual non opti projection mapping passing by a json convertion -> Change to auto mapper techno more performant

    //    var objJson = Newtonsoft.Json.JsonConvert.SerializeObject(source);
    //    return JsonConvert.DeserializeObject<TProjection>(objJson) ?? throw new InvalidOperationException("Map source (" + typeof(TSource) + ") to projection (" + typeof(TProjection) + ") failed");
    //}

    //        #endregion

    //        #endregion
    //    }

    //    /// <inheritdoc />
    //    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    //    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    //    /// <seealso cref="IReadOnlyRepository{TEntity}" />
    //    [DebuggerDisplay("Memory Repository {s_entityTraits}[{s_entityIdTraits}] : {_configurationName}")]
    //internal class MemoryReadOnlyRepository<TEntity, TEntityId> : MemoryReadOnlyRepository<TEntity>, IReadOnlyRepository<TEntity, TEntityId>
    //        where TEntity : IEntityWithId<TEntityId>
    //        where TEntityId : IEquatable<TEntityId>
    //{
    //    #region Fields

    //    private static readonly Type s_entityIdTraits = typeof(TEntityId);

    //    private readonly IDedicatedObjectConverter _dedicatedObjectConverter;

    //    #endregion

    //    #region Ctor

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="MemoryReadOnlyGrainStateRepository{TEntity, TEntityId}"/> class.
    //    /// </summary>
    //    public MemoryReadOnlyRepository(string stateName,
    //                                    string storageName,
    //                                    IGrainFactory grainFactory,
    //                                    IDemocriteSerializer democriteSerializer,
    //                                    IDedicatedObjectConverter dedicatedObjectConverter)
    //        : base(stateName, storageName, grainFactory, democriteSerializer)
    //    {
    //        this._dedicatedObjectConverter = dedicatedObjectConverter;
    //    }

    //    #endregion

    //    #region Methods

    //    /// <inheritdoc />
    //    public virtual async ValueTask<IReadOnlyCollection<TEntity>> GetByIdsValueAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
    //    {
    //        var entities = await GetAllImplByKeysAsync<TEntityId>(entityIds, token);
    //        return entities?.Where(e => entityIds.Contains(e.Uid)).ToReadOnly() ?? EnumerableHelper<TEntity>.ReadOnly;
    //    }

    //    /// <inheritdoc />
    //    public virtual async ValueTask<IReadOnlyCollection<TProjection>> GetByIdsValueAsync<TProjection>([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
    //    {
    //        var entities = await GetAllImplByKeysAsync<TEntityId>(entityIds, token);
    //        var results = entities?.Where(e => entityIds.Contains(e.Uid)).ToReadOnly() ?? EnumerableHelper<TEntity>.ReadOnly;

    //        return results.Select(r =>
    //        {
    //            if (r is TProjection projection)
    //                return projection;

    //            if (this._dedicatedObjectConverter.TryConvert(r, typeof(TProjection), out var result))
    //                return (TProjection)result!;

    //            throw new InvalidDataException("Could not project " + r + " to " + typeof(TProjection));
    //        }).ToArray();
    //    }

    //    /// <inheritdoc />
    //    public virtual async ValueTask<TEntity?> GetByIdValueAsync([NotNull] TEntityId entityId, CancellationToken token)
    //    {
    //        var entities = await GetAllImplByKeysAsync<TEntityId>(token, entityId);

    //        if (entities is null)
    //            return default;

    //        return entities.FirstOrDefault(e => EqualityComparer<TEntityId>.Default.Equals(entityId, e.Uid)) ?? default;
    //    }

    //    #endregion
    //}
}