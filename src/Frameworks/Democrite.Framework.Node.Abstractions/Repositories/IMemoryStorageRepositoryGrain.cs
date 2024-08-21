// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Repositories
{
    using Elvex.Toolbox.Models;

    using Orleans.Concurrency;

    /// <summary>
    /// In memory storage grain used to serialize in binary data in the cluster memory
    /// Grain Id must be a compound (index+StorageName). the age name will be used to contact the correct registry
    /// </summary>
    public interface IMemoryStorageRepositoryGrain<TKey> : IGrainWithIntegerCompoundKey
        where TKey : notnull, IEquatable<TKey>
    {
        /// <summary>
        /// Writes the data.
        /// </summary>
        Task<bool> WriteDatAsync(TKey key,
                                 object? entityKey,
                                 bool insertIfNew,
                                 string stateName,
                                 ReadOnlyMemory<byte> data,
                                 AbstractType entityType,
                                 IReadOnlyCollection<AbstractType> parentTypes);

        /// <summary>
        /// Read data bytes
        /// </summary>
        [ReadOnly]
        Task<ReadOnlyMemory<byte>?> ReadDataAsync(TKey key);

        /// <summary>
        /// Deletes the data
        /// </summary>
        Task<bool> DeleteDataAsync(string stateName, TKey key, object? entityKey);
    }
}
