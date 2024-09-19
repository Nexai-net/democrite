// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Node.Abstractions.Repositories;

    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    using Orleans.Configuration;

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    /// <seealso cref="IReadOnlyRepository{TEntity}" />
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    internal class MemoryRepository<TEntity, TEntityId> : IReadOnlyRepository<TEntity, TEntityId>, IRepository<TEntity, TEntityId>, ISupportDebugDisplayName
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : notnull, IEquatable<TEntityId>

    {
        #region Fields

        private static readonly Func<TEntity, bool> s_allTrue;
        private static readonly AbstractType s_entityTypeId;
        private static readonly AbstractType s_entityType;

        private readonly IOptionsMonitor<MemoryGrainStorageOptions> _grainStorageOptions;
        private readonly IMemoryStorageRepositoryGrain<TEntityId>[] _dataBalancer;
        private readonly IDedicatedObjectConverter _dedicatedObjectConverter;
        private readonly ILogger<IRepository<TEntity, TEntityId>> _logger;
        private readonly IMemoryStorageRegistryGrainMaster _registry;
        private readonly IDemocriteSerializer _democriteSerializer;
        private readonly IGrainFactory _grainFactory;

        private readonly string _configurationName;
        private readonly string _storageName;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        static MemoryRepository()
        {
            s_entityType = typeof(TEntity).GetAbstractType();
            s_entityTypeId = typeof(TEntityId).GetAbstractType();

            Expression<Func<TEntity, bool>>? filter = _ => true;
            s_allTrue = filter.Compile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        public MemoryRepository(string storageName,
                                string configurationName,
                                IGrainFactory grainFactory,
                                IOptionsMonitor<MemoryGrainStorageOptions> grainStorageOptions,
                                IDemocriteSerializer democriteSerializer,
                                IDedicatedObjectConverter dedicatedObjectConverter,
                                ILogger<IRepository<TEntity, TEntityId>> logger,
                                bool readOnly)
        {
            this._dedicatedObjectConverter = dedicatedObjectConverter;
            this._democriteSerializer = democriteSerializer;

            this._grainStorageOptions = grainStorageOptions;
            this._grainFactory = grainFactory;

            this._logger = logger ?? NullLogger<IRepository<TEntity, TEntityId>>.Instance;
            this.IsReadOnly = readOnly;

            this._storageName = storageName;
            this._configurationName = configurationName;

            this._dataBalancer = new IMemoryStorageRepositoryGrain<TEntityId>[this._grainStorageOptions.CurrentValue?.NumStorageGrains ?? 10];
            this._registry = grainFactory.GetGrain<IMemoryStorageRegistryGrainMaster>(configurationName);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsReadOnly { get; }

        /// <inheritdoc />
        public bool SupportExpressionFilter
        {
            get { return true; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<TEntity?> GetFirstValueAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            var entities = await GetAllImplAsync(filterExpression, token, 1);
            return entities.FirstOrDefault();
        }

        /// <inheritdoc />
        public async ValueTask<TProjection?> GetFirstValueAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            var entities = await GetAllImplAsync(filterExpression, token, 1);
            return entities.Select(e => Map<TProjection, TEntity>(e)!).FirstOrDefault();
        }

        public async ValueTask<TEntity?> GetValueByIdAsync([NotNull] TEntityId entityId, CancellationToken token)
        {
            return (await GetValueByIdAsync(entityId.AsEnumerable().ToArray(), token)).FirstOrDefault();
        }

        /// <inheritdoc />
        public async ValueTask<TProjection?> GetValueByIdAsync<TProjection>([NotNull] TEntityId entityId, CancellationToken token)
        {
            return (await GetValueByIdAsync<TProjection>(entityId.AsEnumerable().ToArray(), token)).FirstOrDefault();
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TProjection>> GetValueByIdAsync<TProjection>([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            var entities = await GetAllImplByKeysAsync(entityIds, token);
            return entities.Select(e => Map<TProjection, TEntity>(e)!).ToArray();
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TEntity>> GetValueByIdAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            var entities = await GetAllImplByKeysAsync(entityIds, token);
            return entities.ToArray();
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            var entities = await GetAllImplAsync(filterExpression, token);
            return entities.Select(e => Map<TProjection, TEntity>(e)!).ToArray();
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TEntity>> GetValuesAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            var entities = await GetAllImplAsync(filterExpression, token);
            return entities.ToArray();
        }

        /// <inheritdoc />
        public async Task<bool> PushDataRecordAsync(TEntity entity, bool insertIfNew, CancellationToken token)
        {
            CheckReadOnly();

            ArgumentNullException.ThrowIfNull(entity);

            var storageShelveGrain = GetTargetStorage(entity.Uid);

            var bytes = this._democriteSerializer.SerializeToBinary(entity);

            return await storageShelveGrain.WriteDatAsync(entity.Uid,
                                                          entity.Uid,
                                                          insertIfNew,
                                                          this._storageName,
                                                          bytes,
                                                          entity.GetType().GetAbstractType(),
                                                          entity.GetType().GetAllCompatibleAbstractTypes());
        }

        /// <inheritdoc />
        public async Task<int> PushDataRecordAsync(IReadOnlyCollection<TEntity> entities, bool insertIfNew, CancellationToken token)
        {
            CheckReadOnly();

            int pushCounter = 0;
            foreach (var entity in entities)
            {
                try
                {
                    var pushed = await PushDataRecordAsync(entity, insertIfNew, token);

                    if (pushed)
                        pushCounter++;
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Error, "Memory Push data record failed insertIfNew : {insertIfNew} : {entity} => {exception}", entity, ex);
                }
            }

            return pushCounter;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteRecordAsync(TEntityId uid, CancellationToken cancellationToken)
        {
            CheckReadOnly();

            var storageShelveGrain = GetTargetStorage(uid);
            return await storageShelveGrain.DeleteDataAsync(this._storageName, uid, null);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteRecordAsync(IReadOnlyCollection<TEntityId> entityIds, CancellationToken cancellationToken)
        {
            CheckReadOnly();

            bool deletedAll = true;
            foreach (var uid in entityIds)
            {
                try
                {
                    deletedAll &= await DeleteRecordAsync(uid, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Error, "Memory Delete data failed  {entityId} => {exception}", uid, ex);
                }
            }

            return deletedAll;
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"Memory Repository <{s_entityType}, {s_entityTypeId}> : Configuration : {this._configurationName} => {this._storageName}";
        }

        #region Tools

        /// <summary>
        /// Download all compatible values and return a enumeration that could be filtered
        /// </summary>
        protected async Task<IEnumerable<TEntity>> GetAllImplAsync(Expression<Func<TEntity, bool>>? filterExpression, CancellationToken token, int? limit = null)
        {
            using (var cancelSource = new GrainCancellationTokenSource())
            {
                limit ??= int.MaxValue;

                token.Register(() => cancelSource.Cancel().ConfigureAwait(false));

                var filter = filterExpression?.Compile() ?? s_allTrue;

                var entitiesByBytes = await this._registry.GetAllStoreDataAsync(this._storageName, s_entityType, cancelSource.Token);

                return entitiesByBytes?.Select(e => this._democriteSerializer.Deserialize<TEntity>(e))
                                       .Where(filter)
                                       .Take(limit.Value) ?? EnumerableHelper<TEntity>.ReadOnly;
            }
        }

        /// <summary>
        /// Download all compatible values and return a enumeration that could be filtered
        /// </summary>
        protected Task<IEnumerable<TEntity>> GetAllImplByKeysAsync<TKey>(CancellationToken token, params TKey[] keys)
            where TKey : notnull, IEquatable<TKey>
        {
            return GetAllImplByKeysAsync<TKey>(keys, token);
        }

        /// <summary>
        /// Download all compatible values and return a enumeration that could be filtered
        /// </summary>
        protected async Task<IEnumerable<TEntity>> GetAllImplByKeysAsync<TKey>(IReadOnlyCollection<TKey> keys, CancellationToken token)
            where TKey : notnull, IEquatable<TKey>
        {
            using (var cancelSource = new GrainCancellationTokenSource())
            {
                token.Register(() => cancelSource.Cancel().ConfigureAwait(false));

                // TODO : Add state filter
                var entitiesByBytes = await this._registry.GetAllStoreByKeysDataAsync<TKey>(this._storageName, keys, cancelSource.Token);

                return entitiesByBytes?.Select(e => this._democriteSerializer.Deserialize<TEntity>(e)) ?? EnumerableHelper<TEntity>.ReadOnly;
            }
        }

        /// <summary>
        /// Maps <typeparamref name="TSource"/> to <typeparamref name="TProjection"/>
        /// </summary>
        private TProjection? Map<TProjection, TSource>(in TSource? source)
        {
            if (EqualityComparer<TSource>.Default.Equals(source, default))
                return default;

            if (source is TProjection projection)
                return projection;

            if (source is ISupportConvert<TProjection> convertProj)
                return convertProj.Convert();

            if (source is ISupportConvert convert && convert.TryConvert<TProjection>(out var manualConvert))
                return manualConvert;

            // OPTI : Usual non opti projection mapping passing by a json convertion -> Change to auto mapper techno more performant

            var objJson = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            return OnMap<TProjection, TSource>(source, objJson);
        }

        /// <summary>
        /// Called when [map].
        /// </summary>
        protected virtual TProjection OnMap<TProjection, TSource>(in TSource? source, string objJson)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<TProjection>(objJson) ?? throw new InvalidOperationException("Map source (" + typeof(TSource) + ") to projection (" + typeof(TProjection) + ") failed");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Checks the read only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckReadOnly([CallerMemberName] string? callerName = null)
        {
            if (this.IsReadOnly)
                throw new InvalidOperationException("Repository in readonly access, {0} is not allowed".WithArguments(callerName));
        }

        /// <summary>
        /// Gets the target storage based on <typeparamref name="TEntityId"/> value and balancer size
        /// </summary>
        private IMemoryStorageRepositoryGrain<TEntityId> GetTargetStorage(TEntityId entityUid)
        {
            var containerIndex = Math.Abs(entityUid.GetHashCode() % this._dataBalancer.Length);
            var storageShelveGrain = this._dataBalancer[containerIndex];

            if (storageShelveGrain is null)
            {
                storageShelveGrain = this._grainFactory.GetGrain<IMemoryStorageRepositoryGrain<TEntityId>>(containerIndex, this._configurationName);
                this._dataBalancer[containerIndex] = storageShelveGrain;
            }

            return storageShelveGrain;
        }

        #endregion

        #endregion
    }
}