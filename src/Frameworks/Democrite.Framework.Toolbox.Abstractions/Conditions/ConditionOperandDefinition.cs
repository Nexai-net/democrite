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
    /// Define a group condition link by <see cref="OperandEnum"/>
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionOperandDefinition : ConditionBaseDefinition
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    {
        #region Fields

        public const string TypeDiscriminator = "op";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionGroupDefinition"/> class.
        /// </summary>
        public ConditionOperandDefinition(ConditionBaseDefinition? left,
                                          OperandEnum operand,
                                          ConditionBaseDefinition right)
        {
            this.Left = left;
            this.Operand = operand;
            this.Right = right;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the left.
        /// </summary>
        public ConditionBaseDefinition? Left { get; }

        /// <summary>
        /// Gets the operand.
        /// </summary>
        public OperandEnum Operand { get; }

        /// <summary>
        /// Gets the right.
        /// </summary>
        public ConditionBaseDefinition Right { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionOperandDefinition x, ConditionOperandDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionOperandDefinition x, ConditionOperandDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            var op = other as ConditionOperandDefinition;

            return op is not null &&
                   (this.Left?.Equals(op.Left) ?? op.Left is null) &&
                   this.Operand == op.Operand &&
                   this.Right.Equals(op.Right);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return (this.Left?.GetHashCode() ?? 0) ^
                   this.Operand.GetHashCode() ^
                   this.Right.GetHashCode();
        }

        #endregion
    }
}
