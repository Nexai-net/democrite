// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Store simple value
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionValueDefinition : ConditionBaseDefinition
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionValueDefinition"/> class.
        /// </summary>
        public ConditionValueDefinition(Type type, object? value)
        {
            this.Type = type;
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public object? Value { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionValueDefinition x, ConditionValueDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionValueDefinition x, ConditionValueDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            return other is ConditionValueDefinition val &&
                   this.Type.Equals(val.Type) &&
                   object.Equals(this.Value, val.Value);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Type.GetHashCode() ^ (this.Value?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
