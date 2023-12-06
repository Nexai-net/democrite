// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    /// <summary>
    /// Define the instance is the one expression parameter
    /// </summary>
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionExpressionParameterDefinition : ConditionBaseDefinition
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    {
        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            return true;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return 1;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionExpressionParameterDefinition x, ConditionExpressionParameterDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionExpressionParameterDefinition x, ConditionExpressionParameterDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        #endregion
    }
}
