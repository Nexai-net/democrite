// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Enums;
    using Elvex.Toolbox.Models;

    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Persistant grain used to monitor activity in memory storage for a dedicated configuration
    /// </summary>
    /// <seealso cref="IGrainWithStringKey" />
    internal interface IMemoryStorageRegistryGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Gets all store data that match requested type <paramref name="entityAbstract"/>
        /// </summary>
        /// <remarks>
        /// Attention: the registry grain could be deployed on any silo that doesn't have specifically the store type declared
        /// that why the value store and transmit are only in serialize binary format.
        /// </remarks>
        Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreDataAsync([AllowNull] string? stateName, [NotNull] AbstractType entityAbstract, GrainCancellationToken token);
    }

    /// <summary>
    /// Persistant grain used to monitor activity in memory storage for a dedicated configuration
    /// </summary>
    /// <seealso cref="IGrainWithStringKey" />
    internal interface IMemoryStorageRegistryGrain<TKey> : IMemoryStorageRegistryGrain
        where TKey : notnull, IEquatable<TKey>
    {
        /// <summary>
        /// Gets all store data that match requested type <paramref name="entityAbstract"/>
        /// </summary>
        /// <remarks>
        /// Attention: the registry grain could be deployed on any silo that doesn't have specifically the store type declared
        /// that why the value store and transmit are only in serialize binary format.
        /// </remarks>
        Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreByKeysDataAsync([AllowNull] string? stateName, IReadOnlyCollection<TKey> fullkeys, GrainCancellationToken token);

        /// <summary>
        /// Reports the action occured on a specific storage
        /// </summary>
        Task ReportActionAsync(StoreActionEnum storeAction, string stateName, TKey fullkey, AbstractType? type, IReadOnlyCollection<AbstractType>? parentTypes, GrainId? source, GrainId? storageGrain);
    }
}
