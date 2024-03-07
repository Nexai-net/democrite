// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Expressions
{
    using Democrite.Framework.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define an initialization using a <see cref="AccessExpressionDefinition"/> and a new Type during convertion expression type
    /// </summary>
    /// <seealso cref="MemberBindingDefinition" />
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    [DebuggerDisplay("Convert: {ToDebugDisplayName()}")]
    public sealed class MemberInputConvertBindingDefinition : MemberBindingDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInputConvertBindingDefinition"/> class.
        /// </summary>
        public MemberInputConvertBindingDefinition(bool isCtorParameter,
                                                   string memberName,
                                                   ConcretType newType,
                                                   AccessExpressionDefinition access)
            : base(isCtorParameter, memberName)
        {
            this.Access = access;
            this.NewType = newType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the member access.
        /// </summary>
        [DataMember]
        public AccessExpressionDefinition Access { get; }

        /// <summary>
        /// Creates new type.
        /// </summary>
        [DataMember]
        public ConcretType NewType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(MemberBindingDefinition other)
        {
            return other is MemberInputConvertBindingDefinition otherChain &&
                   otherChain.Access == this.Access &&
                   (otherChain.NewType?.Equals((AbstractType)this.NewType) ?? this.NewType is null);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Access, this.NewType);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"{base.MemberName} = {this.Access.ToDebugDisplayName()} To {this.NewType}";
        }

        #endregion
    }
}
