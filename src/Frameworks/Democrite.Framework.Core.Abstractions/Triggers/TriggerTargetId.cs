// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Define a trigger target
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct TriggerTargetId(Guid Uid, string? Name, TargetTypeEnum Type);
}
