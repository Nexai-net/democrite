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
    internal class InMemoryGrainState<T> : IGrainState<T>, IEntityWithId<string>
    {
        [Id(0)]
        public T State { get; set; }

        [Id(1)]
        public string ETag { get; set; }

        [Id(2)]
        public bool RecordExists { get; set; }

        [Id(3)] 
        public string Uid { get; set; }
    }
}
