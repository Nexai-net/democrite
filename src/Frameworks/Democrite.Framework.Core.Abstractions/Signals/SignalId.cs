// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Data structure used to carry the unique information about a signal
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public readonly record struct SignalId(Guid Uid, string? Name);
}
