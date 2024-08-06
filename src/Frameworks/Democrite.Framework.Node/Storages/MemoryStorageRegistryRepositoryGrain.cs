// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Node.Abstractions.Repositories;

    using Elvex.Toolbox.Abstractions.Models;

    using Microsoft.Extensions.Logging;

    using Orleans;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// System storage repository grain registry
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="IMemoryStorageRepositoryRegistryGrain" />
    [KeepAlive]
    internal sealed class MemoryStorageRegistryRepositoryGrain<TKey> : MemoryStorageRegistryBaseGrain<TKey, byte[], IMemoryStorageRepositoryGrain<TKey>>, IMemoryStorageRepositoryRegistryGrain<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageRegistryBaseGrain"/> class.
        /// </summary>
        public MemoryStorageRegistryRepositoryGrain(IGrainFactory grainFactory,
                                                    ILogger<IMemoryStorageRegistryGrain> logger,
                                                    IDedicatedObjectConverter dedicatedObjectConverter)
            : base(grainFactory, logger, dedicatedObjectConverter)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task<byte[]?> OnRequestStoredDataAsync(TKey key, MemoryStorageInfo info)
        {
            var grain = info.StorageGrain ?? this.RegisterGrainFactory.GetGrain<IMemoryStorageRepositoryGrain<TKey>>(info.Storage);
            var data = (await grain.ReadDataAsync(key))?.ToArray();
            return data;
        }

        /// <inheritdoc />
        protected override ReadOnlyMemory<byte> GetEntityBytes(byte[] dataStored)
        {
            return dataStored;
        }

        #endregion
    }
}
