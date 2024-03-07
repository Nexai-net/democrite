﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Storage;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// System storage grain state <see cref="IGrainState{T}"/> registry
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="IMemoryStorageRegistryGrain" />
    [KeepAlive]
    internal sealed class MemoryStorageRegistryStateGrain : MemoryStorageRegistryBaseGrain<string, IGrainState<ReadOnlyMemory<byte>>>, IMemoryStorageStateRegistryGrain<string>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStorageRegistryGrain"/> class.
        /// </summary>
        public MemoryStorageRegistryStateGrain(IGrainFactory grainFactory,
                                               ILogger<IMemoryStorageRegistryGrain<string>> logger)
            : base(grainFactory, logger)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Task<IGrainState<ReadOnlyMemory<byte>>?> OnRequestStoredDataAsync(string key, MemoryStorageInfo info)
        {
            var storageGrain = this.RegisterGrainFactory.GetGrain<IMemoryStorageGrain>(info.Storage);
            return storageGrain.ReadStateAsync<ReadOnlyMemory<byte>>(key);
        }

        /// <inheritdoc />
        protected override ReadOnlyMemory<byte> GetEntityBytes(IGrainState<ReadOnlyMemory<byte>> dataStored)
        {
            return dataStored.State;
        }

        #endregion

    }
}
