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
    /// Member binding based on constant value
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    [DebuggerDisplay("par: {ToDebugDisplayName()}")]
    public sealed class MemberInputParameterBindingDefinition : MemberBindingDefinition
    {
        #region Ctor

        /// <summary>
        /// Initialize a new instance of the class <see cref="MemberInputConstantBindingDefinition{TValue}"/>
        /// </summary>
        public MemberInputParameterBindingDefinition(bool isCtorParameter,
                                                     string memberName,
                                                     int parameterIndex)
            : base(isCtorParameter, memberName)
        {
            this.ParameterIndex = parameterIndex;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the index of the parameter.
        /// </summary>
        [DataMember]
        public int ParameterIndex { get; }

        #endregion

        #region Methods

        /// <inheritdoc cref="IEquatable{T}.Equals(T?)" />
        protected override bool OnEquals(MemberBindingDefinition other)
        {
            return other is MemberInputParameterBindingDefinition p &&
                   p.ParameterIndex == this.ParameterIndex;
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected override int OnGetHashCode()
        {
            return this.ParameterIndex.GetHashCode();
        }

        /// <summary>
        /// Converts to debugdisplayname.
        /// </summary>
        public override string ToDebugDisplayName()
        {
            return $"{base.MemberName} = Arg{this.ParameterIndex}";
        }

        #endregion
    }
}
