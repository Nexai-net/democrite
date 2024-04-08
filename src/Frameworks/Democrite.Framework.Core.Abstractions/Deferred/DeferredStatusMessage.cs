// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Deferred
{
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct DeferredStatusMessage(DeferredId DeferredId, DeferredStatusEnum Status, DateTime UTCLastUpdateStatus);
}
