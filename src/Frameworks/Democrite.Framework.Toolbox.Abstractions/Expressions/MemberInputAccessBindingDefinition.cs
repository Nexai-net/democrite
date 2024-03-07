// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Expressions
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define an initialization using a <see cref="AccessExpressionDefinition"/>
    /// </summary>
    /// <seealso cref="MemberBindingDefinition" />
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    [DebuggerDisplay("Access: {ToDebugDisplayName()}")]
    public sealed class MemberInputAccessBindingDefinition : MemberBindingDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInputAccessBindingDefinition"/> class.
        /// </summary>
        public MemberInputAccessBindingDefinition(bool isCtorParameter,
                                                  string memberName,
                                                  AccessExpressionDefinition access) 
            : base(isCtorParameter, memberName)
        {
            this.Access = access;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the member access.
        /// </summary>
        [DataMember]
        public AccessExpressionDefinition Access { get; } 

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(MemberBindingDefinition other)
        {
            return other is MemberInputAccessBindingDefinition otherChain &&
                   otherChain.Access == this.Access;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Access);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"{base.MemberName} = {this.Access.ToDebugDisplayName()}";
        }

        #endregion
    }
}
