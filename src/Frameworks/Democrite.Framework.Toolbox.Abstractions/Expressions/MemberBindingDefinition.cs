// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Expressions
{
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a way to bind a member
    /// </summary>
    /// <seealso cref="IEquatable{MemberBindingDefinition}" />
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]

    [KnownType(typeof(MemberInputConstantBindingDefinition<>))]
    [KnownType(typeof(MemberInputParameterBindingDefinition))]
    [KnownType(typeof(MemberInputCallChainBindingDefinition))]
    [KnownType(typeof(MemberInputNestedInitBindingDefinition))]
    [KnownType(typeof(MemberInputAccessBindingDefinition))]
    
    public abstract class MemberBindingDefinition : IEquatable<MemberBindingDefinition>, ISupportDebugDisplayName
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberBindingDefinition"/> class.
        /// </summary>
        protected MemberBindingDefinition(bool isCtorParameter, string memberName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(memberName);

            this.IsCtorParameter = isCtorParameter;
            this.MemberName = memberName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is ctor parameter.
        /// </summary>
        [DataMember]
        public bool IsCtorParameter { get; }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        [DataMember]
        public string MemberName { get; }

        #endregion

        #region Methods        

        /// <inheritdoc />
        public bool Equals(MemberBindingDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return string.Equals(this.MemberName, other.MemberName) &&
                   this.IsCtorParameter == other.IsCtorParameter &&
                   OnEquals(other);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is MemberBindingDefinition member)
                return Equals(member);
            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.IsCtorParameter,
                                    this.MemberName,
                                    OnGetHashCode());
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        protected abstract int OnGetHashCode();

        /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
        protected abstract bool OnEquals(MemberBindingDefinition other);

        /// <inheritdoc />
        public abstract string ToDebugDisplayName();

        #endregion
    }
}
