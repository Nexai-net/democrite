// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions;

    using Elvex.Toolbox.Models;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    ///  one Memory registry master by StorageConfig used to redirect any query to dedicated registry indexed by state name
    /// </summary>
    [KeepAlive]
    internal sealed class MemoryStorageRegistryGrainMaster : Grain, IMemoryStorageRegistryGrainMaster
    {
        #region Fields

        private readonly Dictionary<string, Dictionary<GrainId, IMemoryStorageRegistryGrain>> _memoryStorageRegistry;
        private readonly IGrainOrleanFactory _grainOrleanFactory;
        private readonly ReaderWriterLockSlim _locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageRegistryGrainMaster{TKey}"/> class.
        /// </summary>
        public MemoryStorageRegistryGrainMaster(IGrainOrleanFactory grainOrleanFactory)
        {
            this._grainOrleanFactory = grainOrleanFactory;
            this._locker = new ReaderWriterLockSlim();
            this._memoryStorageRegistry = new Dictionary<string, Dictionary<GrainId, IMemoryStorageRegistryGrain>>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        [ReadOnly]
        public Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreByKeysDataAsync<TKey>(string stateName, IReadOnlyCollection<TKey> fullkeys, GrainCancellationToken token)
            where TKey : notnull, IEquatable<TKey>
        {
            return GetAllStoreImplAsync(stateName, token, registry => registry.GetAllStoreByKeysDataAsync(fullkeys, token));
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreDataAsync(string stateName, [NotNull] AbstractType entityAbstract, GrainCancellationToken token)
        {
            return GetAllStoreImplAsync(stateName, token, registry => registry.GetAllStoreDataAsync(entityAbstract, token));
        }

        /// <inheritdoc />
        public Task RegisterRegistryAsync(DedicatedGrainId<IMemoryStorageRegistryGrain> dedicatedGrainId, string stateName)
        {
            this._locker.EnterWriteLock();
            try
            {
                var grainId = dedicatedGrainId.Target;

                Dictionary<GrainId, IMemoryStorageRegistryGrain>? registriesByStateName;

                if (!this._memoryStorageRegistry.TryGetValue(stateName!, out registriesByStateName))
                {
                    registriesByStateName = new Dictionary<GrainId, IMemoryStorageRegistryGrain>();
                    this._memoryStorageRegistry.Add(stateName!, registriesByStateName);
                }

                registriesByStateName[grainId] = this._grainOrleanFactory.GetGrain(grainId).AsReference<IMemoryStorageRegistryGrain>();
            }
            finally
            {
                this._locker.ExitWriteLock();
            }

            return Task.CompletedTask;
        }

        #region Tools

        /// <summary>
        /// Gets all store through all the dedicated registries
        /// </summary>
        private async Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreImplAsync(string stateName, GrainCancellationToken token, Func<IMemoryStorageRegistryGrain, Task<IReadOnlyCollection<ReadOnlyMemory<byte>>>> query)
        {
            Task<IReadOnlyCollection<ReadOnlyMemory<byte>>>[] tasks;
            this._locker.EnterReadLock();
            try
            {
                if (this._memoryStorageRegistry.TryGetValue(stateName, out var registries))
                {
                    tasks = registries.Values
                                      .Select(registry => query(registry))
                                      .ToArray();
                }
                else
                {
                    return EnumerableHelper<ReadOnlyMemory<byte>>.ReadOnly;
                }
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            await tasks.SafeWhenAllAsync(token.CancellationToken);
            return tasks.Where(t => t.IsCompletedSuccessfully && t.Result is not null)
                        .SelectMany(t => t.Result)
                        .Distinct()
                        .ToArray();
        }

        #endregion

        #endregion
    }
}
