// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using Elvex.Toolbox.Abstractions.Comparers;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Global registry used for synchronization
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal record class ReferenceTargetRegistry(string Etag, IReadOnlyCollection<ReferenceTarget> References);

    /// <summary>
    /// Define information reference by the <paramref name="RefId"/>
    /// </summary>
    /// <remarks>
    ///     <see cref="ReferenceTarget"/> is not a 'record' because the <see cref="URI"/> Comparaison doesn't by default take care of the fragments.
    ///     Method are encoded on the fragments part
    /// </remarks>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal abstract class ReferenceTarget : IEquatable<ReferenceTarget>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceTarget"/> class.
        /// </summary>
        protected ReferenceTarget(Uri refId, RefTypeEnum refType)
        {
            this.RefId = refId;
            this.RefType = refType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the reference identifier.
        /// </summary>
        [Id(0)]
        [DataMember]
        public Uri RefId { get; }

        /// <summary>
        /// Gets the type of the reference.
        /// </summary>
        [Id(1)]
        [DataMember]
        public RefTypeEnum RefType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(ReferenceTarget? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return other.RefType == this.RefType &&
                   UriComparer.WithFragment.Equals(other.RefId, this.RefId) &&
                   OnEquals(other);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is ReferenceTarget target)
                return Equals(target);
            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.RefId, this.RefType, OnGetHashCode());
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T?)" />
        protected abstract bool OnEquals(ReferenceTarget other);

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract object OnGetHashCode();

        #endregion
    }

    /// <summary>
    /// Define Definition (Sequence, Trigger, Signal, ...) reference by the <paramref name="RefId"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal sealed class ReferenceDefinitionTarget : ReferenceTarget
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDefinitionTarget"/> class.
        /// </summary>
        public ReferenceDefinitionTarget(Uri refId, RefTypeEnum refType, Guid definitionId)
            : base(refId, refType)
        {
            this.DefinitionId = definitionId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the definition identifier.
        /// </summary>
        [Id(0)]
        [DataMember]
        public Guid DefinitionId { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(ReferenceTarget other)
        {
            return other is ReferenceDefinitionTarget def &&
                   def.DefinitionId == this.DefinitionId;
        }

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return this.DefinitionId.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// Define type reference by the <paramref name="RefId"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal class ReferenceTypeTarget : ReferenceTarget
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceTypeTarget"/> class.
        /// </summary>
        public ReferenceTypeTarget(Uri refId, RefTypeEnum refType, AbstractType type)
            : base(refId, refType)
        {
            this.Type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type.
        /// </summary>
        public AbstractType Type { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(ReferenceTarget other)
        {
            return other is ReferenceTypeTarget typed &&
                   typed.Type.IsEqualTo(this.Type!);
        }

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return this.Type.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// Define Method reference by the <paramref name="RefId"/>
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    internal sealed class ReferenceTypeMethodTarget : ReferenceTypeTarget
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceTypeMethodTarget"/> class.
        /// </summary>
        public ReferenceTypeMethodTarget(Uri refId, RefTypeEnum refType, AbstractType type, AbstractMethod method)
            : base(refId, refType, type)
        {
            this.Method = method;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the method.
        /// </summary>
        public AbstractMethod Method { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(ReferenceTarget other)
        {
            return other is ReferenceTypeMethodTarget mth &&
                   base.OnEquals(other) &&
                   mth.Method.Equals(this.Method!);
        }

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return HashCode.Combine(this.Method.GetHashCode(), base.OnGetHashCode());
        }

        #endregion
    }
}