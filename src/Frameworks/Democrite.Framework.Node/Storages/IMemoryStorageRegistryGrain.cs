// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox.Models;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Persistant grain used to monitor activity in memory storage for a dedicated configuration
    /// </summary>
    /// <seealso cref="IGrainWithStringKey" />
    internal interface IMemoryStorageRegistryGrain : IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// Gets all store data that match requested type <paramref name="entityAbstract"/>
        /// </summary>
        /// <remarks>
        /// Attention: the registry grain could be deployed on any silo that doesn't have specifically the store type declared
        /// that why the value store and transmit are only in serialize binary format.
        /// </remarks>
        [ReadOnly]
        Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreDataAsync([NotNull] AbstractType entityAbstract, GrainCancellationToken token);

        /// <summary>
        /// Gets all store data that match requested type <paramref name="entityAbstract"/>
        /// </summary>
        /// <remarks>
        /// Attention: the registry grain could be deployed on any silo that doesn't have specifically the store type declared
        /// that why the value store and transmit are only in serialize binary format.
        /// </remarks>
        [ReadOnly]
        Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreByKeysDataAsync<TKey>(IReadOnlyCollection<TKey> fullkeys, GrainCancellationToken token)
                where TKey : notnull, IEquatable<TKey>;

        /// <summary>
        /// Reports the action occured on a specific storage
        /// </summary>
        [OneWay]
        Task ReportActionAsync<TKey>(StoreActionEnum storeAction, TKey fullkey, object? entityKey, AbstractType? type, IReadOnlyCollection<AbstractType>? parentTypes, GrainId? source, GrainId? storageGrain)
            where TKey : notnull, IEquatable<TKey>;
    }

    /// <summary>
    /// GRAIN STATE
    /// 
    /// Persistant grain used to monitor activity in memory storage for a dedicated configuration
    /// </summary>
    /// <seealso cref="IGrainWithStringKey" />
    internal interface IMemoryStorageStateRegistryGrain<TKey> : IMemoryStorageRegistryGrain//<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
    }

    /// <summary>
    /// REPOSITORY
    /// 
    /// Persistant grain used to monitor activity in memory storage for a dedicated configuration
    /// </summary>
    /// <seealso cref="IGrainWithStringKey" />
    internal interface IMemoryStorageRepositoryRegistryGrain<TKey> : IMemoryStorageRegistryGrain//<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
    }

    /// <summary>
    /// Cluster Singleton registry by <typeparamref name="TKey"/> (StorageConfiguration) value used to allow multiple <see cref="IMemoryStorageRegistryGrain{TKey}"/>.
    /// This distribute the Write and Read process for local registry and allow a global SLOW inspection
    /// </summary>
    internal interface IMemoryStorageRegistryGrainMaster : IGrainWithStringKey
    {
        /// <summary>
        /// Registers the registry type <see cref="IMemoryStorageRegistryGrain{TKey}"/>
        /// </summary>
        /// <remarks>
        ///     Method called by the dedicated registry at first activation
        /// </remarks>
        [OneWay]
        Task RegisterRegistryAsync(DedicatedGrainId<IMemoryStorageRegistryGrain> dedicatedGrainId, string stateName);

        /// <summary>
        /// Gets all store data that match requested type <paramref name="entityAbstract"/>
        /// </summary>
        /// <remarks>
        /// Attention: the registry grain could be deployed on any silo that doesn't have specifically the store type declared
        /// that why the value store and transmit are only in serialize binary format.
        /// </remarks>
        [ReadOnly]
        Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreDataAsync(string stateName, [NotNull] AbstractType entityAbstract, GrainCancellationToken token); // , ConditionExpressionDefinition? filter

        /// <summary>
        /// Gets all store data that match requested type <paramref name="entityAbstract"/>
        /// </summary>
        /// <remarks>
        /// Attention: the registry grain could be deployed on any silo that doesn't have specifically the store type declared
        /// that why the value store and transmit are only in serialize binary format.
        /// </remarks>
        [ReadOnly]
        Task<IReadOnlyCollection<ReadOnlyMemory<byte>>> GetAllStoreByKeysDataAsync<TKey>(string stateName, IReadOnlyCollection<TKey> fullkeys, GrainCancellationToken token)
                where TKey : notnull, IEquatable<TKey>;
    }
}
