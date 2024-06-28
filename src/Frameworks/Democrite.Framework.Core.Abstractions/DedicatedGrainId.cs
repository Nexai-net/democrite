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

    public interface IDedicatedGrainId
    {
        /// <summary>
        /// Sets the target.
        /// </summary>
        GrainId Target { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is grain service.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is grain service; otherwise, <c>false</c>.
        /// </value>
        bool IsGrainService { get; }

        /// <summary>
        /// Gets the grain interface.
        /// </summary>
        /// <value>
        /// The grain interface.
        /// </value>
        ConcretType GrainInterface { get; }
    }

    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct DedicatedGrainId<TType>(GrainId Target, bool IsGrainService, ConcretType GrainInterface) : IDedicatedGrainId;
}
