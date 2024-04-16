// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class of custom metadata attached to a <see cref="DataRecordContainer"/>
    /// </summary>
    /// <remarks>
    ///     Used to filter with more informations directly on meta data instead of data itself
    /// </remarks>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class RecordMetadata();

    /// <summary>
    /// Base class of custom metadata attached to a <see cref="DataRecordContainer"/> with expose scoring system
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class ScoredRecordMetadata(double? Score) : RecordMetadata();
}
