// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules
{
    using Elvex.Toolbox.Abstractions.Enums;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public abstract class BlackboardHierarchyLogicalTypeBaseRule : BlackboardLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardHierarchyLogicalTypeBaseRule"/> class.
        /// </summary>
        protected BlackboardHierarchyLogicalTypeBaseRule(string logicalTypePattern,
                                                         IEnumerable<BlackboardLogicalTypeBaseRule> children,
                                                         string? collectionGroup = null,
                                                         ValidationModeEnum? validationMode = null)
            : base(logicalTypePattern)
        {
            ArgumentException.ThrowIfNullOrEmpty(logicalTypePattern);

            this.CollectionGroup = collectionGroup;
            this.ValidationMode = validationMode;
            this.Children = children?.ToArray() ?? EnumerableHelper<BlackboardLogicalTypeBaseRule>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        [DataMember]
        public IReadOnlyCollection<BlackboardLogicalTypeBaseRule> Children { get; }

        /// <summary>
        /// Used to group rules and apply a logic between them
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string? CollectionGroup { get; }

        /// <summary>
        /// Gets the validation mode.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ValidationModeEnum? ValidationMode { get; }

        #endregion

        #region Methods

        /// <inheritdoc cref="object.Equals(object?)" />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardHierarchyLogicalTypeBaseRule hierarchy &&
                   string.Equals(this.CollectionGroup, hierarchy.CollectionGroup) &&
                   this.ValidationMode == hierarchy.ValidationMode &&
                   this.Children.SequenceEqual(hierarchy.Children);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Children.Aggregate(0, (acc, c) => acc ^ c.GetHashCode()),
                                    this.CollectionGroup,
                                    this.ValidationMode);
        }

        #endregion
    }
}
