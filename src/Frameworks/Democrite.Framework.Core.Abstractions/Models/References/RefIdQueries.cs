// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models.References
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base Clase of every Reference Id Query
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class RefIdQuery(RefTypeEnum RefType, string SimpleNameIdentifier, string? NamespaceIdentifier = null)
    {
        /// <summary>
        /// Converts to uri.
        /// </summary>
        public virtual Uri ToUri()
        {
            return RefIdHelper.Generate(this.RefType, this.SimpleNameIdentifier!, this.NamespaceIdentifier);
        }
    }

    /// <summary>
    /// Query dedicated to <see cref="RefTypeEnum.VGrain"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class RefVGrainQuery(string SimpleNameIdentifier, string? NamespaceIdentifier = null) : RefIdQuery(RefTypeEnum.VGrain, SimpleNameIdentifier, NamespaceIdentifier)
    {
        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="RefVGrainQuery"/>.
        /// </summary>
        public static implicit  operator RefVGrainQuery(string SimpleNameIdentifier)
        {
            return new RefVGrainQuery(SimpleNameIdentifier);
        }
    }


    /// <summary>
    /// Query dedicated to <see cref="RefTypeEnum.VGrainImplementation"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class RefVGrainImplQuery(string SimpleNameIdentifier, string? NamespaceIdentifier = null) : RefIdQuery(RefTypeEnum.VGrainImplementation, SimpleNameIdentifier, NamespaceIdentifier)
    {
        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="RefVGrainQuery"/>.
        /// </summary>
        public static implicit operator RefVGrainImplQuery(string SimpleNameIdentifier)
        {
            return new RefVGrainImplQuery(SimpleNameIdentifier);
        }
    }

    /// <summary>
    /// Query dedicated to <see cref="RefTypeEnum.Sequence"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class RefSequenceQuery(string SimpleNameIdentifier, string? NamespaceIdentifier = null) : RefIdQuery(RefTypeEnum.Sequence, SimpleNameIdentifier, NamespaceIdentifier)
    {
        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="RefVGrainQuery"/>.
        /// </summary>
        public static implicit operator RefSequenceQuery(string SimpleNameIdentifier)
        {
            return new RefSequenceQuery(SimpleNameIdentifier);
        }
    }

    /// <summary>
    /// Query dedicated to <see cref="RefTypeEnum.Method"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class RefMethodQuery(string SimpleNameIdentifier) : RefIdQuery(RefTypeEnum.Method, SimpleNameIdentifier, null)
    {
        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="RefVGrainQuery"/>.
        /// </summary>
        public static implicit operator RefMethodQuery(string SimpleNameIdentifier)
        {
            return new RefMethodQuery(SimpleNameIdentifier);
        }
    }
}
