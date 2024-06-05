// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Node.Abstractions.Repositories;

    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Memory storage for any type of data
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Orleans.Grain" />
    /// <seealso cref="IMemoryStorageRepositoryGrain{TKey}" />
    internal sealed class MemoryStorageRepositoryGrain<TKey> : Grain, IMemoryStorageRepositoryGrain<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        #region Fields

        private readonly Dictionary<string, IMemoryStorageRepositoryRegistryGrain<TKey>> _registries;
        
        private readonly Dictionary<TKey, byte[]> _store;
        private readonly IGrainFactory _grainFactory;
        private long _primarykey;
        private string _storageName;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageRepositoryGrain{TKey}"/> class.
        /// </summary>
        public MemoryStorageRepositoryGrain(ILogger<IMemoryStorageRepositoryGrain<TKey>> logger,
                                            IGrainFactory grainFactory)
        {
            // storage name will be set during the grain activation
            // It will not be null during grain usage
            this._storageName = null!;

            this._registries = new Dictionary<string, IMemoryStorageRepositoryRegistryGrain<TKey>>();
            this._store = new Dictionary<TKey, byte[]>();
            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> DeleteDataAsync(string stateName, TKey key)
        {
            var delete = this._store.Remove(key);

            var registry = GetRegistry(stateName);
            await registry!.ReportActionAsync(StoreActionEnum.Clear, key!, null, null, null, this.GetGrainId());
            return delete;
        }

        /// <inheritdoc />
        public Task<ReadOnlyMemory<byte>?> ReadDataAsync(TKey key)
        {
            if (this._store.TryGetValue(key, out var value))
                return Task.FromResult((ReadOnlyMemory<byte>?)value);

            return Task.FromResult((ReadOnlyMemory<byte>?)null);
        }

        /// <inheritdoc />
        public async Task<bool> WriteDatAsync(TKey key,
                                              bool insertIfNew,
                                              string stateName,
                                              ReadOnlyMemory<byte> data,
                                              AbstractType entityType,
                                              IReadOnlyCollection<AbstractType> parentTypes)
        {
            var containKey = this._store.ContainsKey(key);
            if (!insertIfNew && !containKey)
                return false;

            this._store[key] = data.ToArray();

            if (containKey == false)
            {
                var registry = GetRegistry(stateName);
                await registry!.ReportActionAsync(StoreActionEnum.Write, key, entityType, parentTypes, null, this.GetGrainId());
            }

            return true;
        }

        /// <inheritdoc />
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            this._primarykey = this.GetPrimaryKeyLong(out var storageName);
            this._storageName = storageName;

            return base.OnActivateAsync(cancellationToken);
        }

        #region Tools

        /// <summary>
        /// Get dedicated registry
        /// </summary>
        private IMemoryStorageRepositoryRegistryGrain<TKey> GetRegistry(string stateName)
        {
            if (this._registries.TryGetValue(stateName, out var cachedRegistry))
                return cachedRegistry;

            // Registry are distribute through memoryIndex % 10 + stateName ### storageName
            // This will reduce the botle neck of all the memory grain reporting to same registry grain

            var registry = this._grainFactory.GetGrain<IMemoryStorageRepositoryRegistryGrain<TKey>>(this._primarykey / 10, MemoryStorageRegistryHelper.ComputeRegistryExtName(stateName, this._storageName));
            this._registries.Add(stateName, registry);
            return registry;
        }

        #endregion

        #endregion
    }
}
