// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Doors
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Identifier of a door
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public readonly record struct DoorId(Guid Uid, string? Name);
}
