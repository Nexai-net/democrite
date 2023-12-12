// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Conditions
{
    using Democrite.Framework.Toolbox.Abstractions.Enums;

    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define a group condition link by <see cref="LogicEnum"/>
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionGroupDefinition : ConditionBaseDefinition
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    {
        #region Fields

        public const string TypeDiscriminator = "grp";

        #endregion

        #region Ctor        

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionGroupDefinition"/> class.
        /// </summary>
        public ConditionGroupDefinition(LogicEnum logic, IEnumerable<ConditionBaseDefinition> conditions)
        {
            this.Logic = logic;
            this.Conditions = conditions?.ToArray() ?? Array.Empty<ConditionBaseDefinition>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logic.
        /// </summary>
        public LogicEnum Logic { get; }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public IReadOnlyCollection<ConditionBaseDefinition> Conditions { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionGroupDefinition x, ConditionGroupDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionGroupDefinition x, ConditionGroupDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            return other is ConditionGroupDefinition group &&
                   this.Logic == group.Logic &&
                   this.Conditions.OrderBy(a => (a?.GetHashCode() ?? 0)).SequenceEqual(group.Conditions.OrderBy(a => (a?.GetHashCode() ?? 0)));
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Logic.GetHashCode() ^
                   (this.Conditions.OrderBy(a => (a?.GetHashCode() ?? 0)).Aggregate(0, (acc, a) => acc + (a?.GetHashCode() ?? 0)));
        }

        #endregion
    }
}
