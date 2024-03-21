// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models.SignalMessage
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct CollectionChangeSignalMessage<TEntityId>(CollectionChangeTypeEnum ChangeType, IReadOnlyCollection<TEntityId> ImpactedIds);
}
