// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    /// <summary>
    /// Persistant grain used to monitor activity in memory storage for a dedicated configuration
    /// </summary>
    /// <seealso cref="IGrainWithStringKey" />
    internal interface IMemoryStorageRepositoryRegistryGrain<TKey> : IMemoryStorageRegistryGrain<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
    }
}
