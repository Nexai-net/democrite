// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.DataModel
{
    using Orleans;

    using System.ComponentModel;

    /// <summary>
    /// Data container with level of thrust
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class ThrustContainer<TData>(TData Data, double TrustLevel);
}