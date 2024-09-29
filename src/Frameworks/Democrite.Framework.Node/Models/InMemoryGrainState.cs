// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using Orleans;

    /// <summary>
    /// Proxy Grain state used by the inmemory system
    /// </summary>
    [Serializable]
    [GenerateSerializer]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class InMemoryGrainState<T> : IGrainState<T>, IEntityWithId<string>
    {
        [Id(0)]
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public T State { get; set; }
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

        [Id(1)]
        public string? ETag { get; set; }

        [Id(2)]
        public bool RecordExists { get; set; }

        [Id(3)]
        public string Uid { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
