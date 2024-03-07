// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using Democrite.Framework.Toolbox.Abstractions.Enums;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a group math mathOperator link by <see cref="MathOperatorEnum"/>
    /// </summary>
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines mathOperator == or mathOperator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines mathOperator == or mathOperator != but does not override Object.GetHashCode()
    public sealed class ConditionMathOperationDefinition : ConditionBaseDefinition
#pragma warning restore CS0660 // Type defines mathOperator == or mathOperator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines mathOperator == or mathOperator != but does not override Object.GetHashCode()
    {
        #region Fields

        public const string TypeDiscriminator = "mathOp";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionMathOperationDefinition"/> class.
        /// </summary>
        public ConditionMathOperationDefinition(ConditionBaseDefinition? left,
                                                MathOperatorEnum mathOperator,
                                                ConditionBaseDefinition right)
        {
            this.Left = left;
            this.MathOperator = mathOperator;
            this.Right = right;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the left.
        /// </summary>
        [DataMember]
        public ConditionBaseDefinition? Left { get; }

        /// <summary>
        /// Gets the operand.
        /// </summary>
        [DataMember]
        public MathOperatorEnum MathOperator { get; }

        /// <summary>
        /// Gets the right.
        /// </summary>
        [DataMember]
        public ConditionBaseDefinition Right { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the mathOperator !=.
        /// </summary>
        public static bool operator !=(ConditionMathOperationDefinition x, ConditionMathOperationDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the mathOperator ==.
        /// </summary>
        public static bool operator ==(ConditionMathOperationDefinition x, ConditionMathOperationDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            var op = other as ConditionMathOperationDefinition;

            return op is not null &&
                   (this.Left?.Equals(op.Left) ?? op.Left is null) &&
                   this.MathOperator == op.MathOperator &&
                   this.Right.Equals(op.Right);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Left,
                                    this.MathOperator,
                                    this.Right);
        }

        #endregion
    }
}
