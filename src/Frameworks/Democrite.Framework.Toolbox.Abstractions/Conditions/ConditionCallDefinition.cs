// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionCallDefinition : ConditionBaseDefinition
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionCallDefinition"/> class.
        /// </summary>
        public ConditionCallDefinition(ConditionBaseDefinition? instance,
                                       string methodName,
                                       IEnumerable<ConditionBaseDefinition>? arguments)
        {
            this.Instance = instance;
            this.MethodName = methodName;
            this.Arguments = arguments?.ToArray() ?? Array.Empty<ConditionBaseDefinition>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance to call the method; could be null on static method
        /// </summary>
        public ConditionBaseDefinition? Instance { get; }

        /// <summary>
        /// Gets the name of the method called
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public IReadOnlyCollection<ConditionBaseDefinition> Arguments { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionCallDefinition x, ConditionCallDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionCallDefinition x, ConditionCallDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            var call = other as ConditionCallDefinition;

            return call is not null &&
                   (this.Instance?.Equals(call.Instance) ?? call.Instance is null) &&
                   this.MethodName == call.MethodName &&
                   this.Arguments.OrderBy(a => (a?.GetHashCode() ?? 0)).SequenceEqual(call.Arguments.OrderBy(a => (a?.GetHashCode() ?? 0)));
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return (this.Instance?.GetHashCode() ?? 0) ^
                   (this.MethodName?.GetHashCode() ?? 0) ^
                   (this.Arguments.OrderBy(a => (a?.GetHashCode() ?? 0)).Aggregate(0, (acc, a) => acc + (a?.GetHashCode() ?? 0)));
        }

        #endregion
    }
}
