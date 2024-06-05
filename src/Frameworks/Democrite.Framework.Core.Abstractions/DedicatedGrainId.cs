// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Elvex.Toolbox.Models;

    using Orleans.Runtime;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct DedicatedGrainId<TType>(GrainId Target, bool IsGrainService, ConcretType GrainInterface);
}
