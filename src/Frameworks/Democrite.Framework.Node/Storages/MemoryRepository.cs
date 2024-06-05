// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Node.Abstractions.Repositories;
    using Democrite.Framework.Node.Models;

    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;
    using Microsoft.Extensions.Options;
    using Orleans.Configuration;

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    /// <seealso cref="IReadOnlyRepository{TEntity}" />
    [DebuggerDisplay("Memory (AllowWrite) Repository {s_entityTraits}[{s_entityIdTraits}] : {_storageName}")]
    internal class MemoryRepository<TEntity, TEntityId> : MemoryReadOnlyRepository<TEntity, TEntityId>, IReadOnlyRepository<TEntity, TEntityId>, IRepository<TEntity, TEntityId>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : notnull, IEquatable<TEntityId>

    {
        #region Fields

        private static readonly AbstractType s_entityType;

        private readonly IMemoryStorageRepositoryGrain<TEntityId>[] _dataBalancer;
        private readonly IOptionsMonitor<MemoryGrainStorageOptions> _grainStorageOptions;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        static MemoryRepository()
        {
            s_entityType = typeof(TEntity).GetAbstractType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        public MemoryRepository(string stateName,
                                string storageName,
                                IGrainFactory grainFactory,
                                IOptionsMonitor<MemoryGrainStorageOptions> grainStorageOptions,
                                IDemocriteSerializer democriteSerializer,
                                IDedicatedObjectConverter dedicatedObjectConverter)
            : base(stateName, storageName, grainFactory, democriteSerializer, dedicatedObjectConverter)
        {
            this._grainStorageOptions = grainStorageOptions;
            this._grainFactory = grainFactory;
            this._dataBalancer = new IMemoryStorageRepositoryGrain<TEntityId>[this._grainStorageOptions.CurrentValue?.NumStorageGrains ?? 10];
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> PushDataRecordAsync(TEntity entity, bool insertIfNew, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var storageShelveGrain = GetTargetStorage(entity.Uid);

            var bytes = this.DemocriteSerializer.SerializeToBinary(entity);

            return await storageShelveGrain.WriteDatAsync(entity.Uid,
                                                          insertIfNew,
                                                          this.StateName,
                                                          bytes,
                                                          entity.GetType().GetAbstractType(),
                                                          entity.GetType().GetAllCompatibleAbstractTypes());
        }

        /// <inheritdoc />
        public Task<bool> DeleteRecordAsync(TEntityId uid, CancellationToken cancellationToken)
        {
            return DeleteRecordsAsync(cancellationToken, uid.AsEnumerable());
        }

        /// <inheritdoc />
        public async Task<bool> DeleteRecordsAsync(CancellationToken cancellationToken, IEnumerable<TEntityId> ids)
        {
            var removed = false;
            foreach (var id in ids)
            {
                var storageShelveGrain = GetTargetStorage(id);
                removed |= await storageShelveGrain.DeleteDataAsync(this.StateName, id);

            }

            return removed;
        }

        #region Tools

        /// <summary>
        /// Gets the target storage based on <typeparamref name="TEntityId"/> value and balancer size
        /// </summary>
        private IMemoryStorageRepositoryGrain<TEntityId> GetTargetStorage(TEntityId entityUid)
        {
            var containerIndex = Math.Abs(entityUid.GetHashCode() % this._dataBalancer.Length);
            var storageShelveGrain = this._dataBalancer[containerIndex];

            if (storageShelveGrain is null)
            {
                storageShelveGrain = this._grainFactory.GetGrain<IMemoryStorageRepositoryGrain<TEntityId>>(containerIndex, this.StorageName);
                this._dataBalancer[containerIndex] = storageShelveGrain;
            }

            return storageShelveGrain;
        }

        #endregion

        #endregion
    }
}