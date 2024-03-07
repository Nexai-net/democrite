// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Expressions
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define an initialization using a input call chain
    /// </summary>
    /// <seealso cref="MemberBindingDefinition" />
    [DebuggerDisplay("Input: {ToDebugDisplayName()}")]
    public sealed class MemberInputCallChainBindingDefinition : MemberBindingDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInputCallChainBindingDefinition"/> class.
        /// </summary>
        public MemberInputCallChainBindingDefinition(bool isCtorParameter,
                                                     string memberName,
                                                     string callchain,
                                                     int parameterIndex) 
            : base(isCtorParameter, memberName)
        {
            this.CallChain = callchain; 
            this.ParameterIndex = parameterIndex;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the call chain.
        /// </summary>
        [DataMember]
        public string CallChain { get; }

        /// <summary>
        /// Gets the index of the argument.
        /// </summary>
        [DataMember] 
        public int ParameterIndex { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(MemberBindingDefinition other)
        {
            return other is MemberInputCallChainBindingDefinition otherChain &&
                   string.Equals(otherChain.CallChain, this.CallChain) &&
                   otherChain.ParameterIndex == this.ParameterIndex;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.CallChain, this.ParameterIndex);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"{base.MemberName} = (Arg{this.ParameterIndex}) {this.CallChain}";
        }

        #endregion
    }
}
