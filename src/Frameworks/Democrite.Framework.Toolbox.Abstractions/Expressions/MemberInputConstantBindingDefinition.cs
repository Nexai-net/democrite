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
    public abstract class MemberInputConstantBindingDefinition : MemberBindingDefinition
    {
        #region Ctor

        /// <summary>
        /// Initialize a new instance of the class <see cref="MemberInputConstantBindingDefinition{TValue}"/>
        /// </summary>
        public MemberInputConstantBindingDefinition(bool isCtorParameter,
                                                    string memberName)
            : base(isCtorParameter, memberName)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the value 
        /// </summary>
        public abstract object? GetValue();

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    [DebuggerDisplay("cst:  {ToDebugDisplayName()}")]
    public sealed class MemberInputConstantBindingDefinition<TValue> : MemberInputConstantBindingDefinition
    {
        #region Ctor

        /// <summary>
        /// Initialize a new instance of the class <see cref="MemberInputConstantBindingDefinition{TValue}"/>
        /// </summary>
        public MemberInputConstantBindingDefinition(bool isCtorParameter,
                                                    string memberName,
                                                    TValue value) 
            : base(isCtorParameter, memberName)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value in
        /// </summary>
        [DataMember]
        public TValue Value { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override object? GetValue()
        {
            return this.Value;
        }

        /// <inheritdoc />
        protected override bool OnEquals(MemberBindingDefinition other)
        {
            return other is MemberInputConstantBindingDefinition<TValue> otherVal &&
                   EqualityComparer<TValue>.Default.Equals(otherVal.Value, this.Value);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Value?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"{this.MemberName} = {this.Value}";
        }

        #endregion
    }
}
