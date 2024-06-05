// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Carry intormation with a specific etag used to synchronize versions
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct EtagContainer<TInfo>(string Etag, TInfo Info);

    /// <summary>
    /// Carry intormation with a specific etag used to synchronize versions
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class EtagContainerCls<TInfo>(string Etag, TInfo Info);
}
