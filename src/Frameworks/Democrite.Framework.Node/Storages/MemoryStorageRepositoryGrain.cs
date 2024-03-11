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

        private readonly Dictionary<TKey, byte[]> _store;
        private readonly IGrainFactory _grainFactory;

        private IMemoryStorageRepositoryRegistryGrain<TKey>? _registry;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageRepositoryGrain{TKey}"/> class.
        /// </summary>
        public MemoryStorageRepositoryGrain(ILogger<IMemoryStorageRepositoryGrain<TKey>> logger,
                                            IGrainFactory grainFactory)
        {
            this._store = new Dictionary<TKey, byte[]>();
            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> DeleteDataAsync(string stateName, TKey key)
        {
            var delete = this._store.Remove(key);
            await this._registry!.ReportActionAsync(StoreActionEnum.Clear, stateName, key!, null, null, null, this.GetGrainId());
            return delete;
        }

        /// <inheritdoc />
        public Task<ReadOnlyMemory<byte>?> ReadDataAsync(TKey key)
        {
            if (this._store.TryGetValue(key, out var value))
            {
                //await this._registry!.ReportActionAsync(StoreActionEnum.Read, stateName, key!, null, null, null, this.GetGrainId());
                return Task.FromResult((ReadOnlyMemory<byte>?)value);
            }
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
            if (!insertIfNew && !this._store.ContainsKey(key))
                return false;

            this._store[key] = data.ToArray();
            await this._registry!.ReportActionAsync(StoreActionEnum.Write, stateName, key, entityType, parentTypes, null, this.GetGrainId());
            return true;
        }

        /// <inheritdoc />
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            this.GetPrimaryKeyLong(out var storageName);
            this._registry = this._grainFactory.GetGrain<IMemoryStorageRepositoryRegistryGrain<TKey>>(storageName);
            return base.OnActivateAsync(cancellationToken);
        }

        #endregion
    }
}
