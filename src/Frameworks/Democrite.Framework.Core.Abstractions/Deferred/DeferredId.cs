// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Deferred
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Define a id structure used to access deferred reponse
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct DeferredId(Guid Uid, Guid SourceId);
}
