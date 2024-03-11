// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules
{
    using Elvex.Toolbox.Abstractions.Enums;
    using Elvex.Toolbox.Models;

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class BlackboardTypeCheckLogicalTypeRule : BlackboardHierarchyLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTypeCheckLogicalTypeRule"/> class.
        /// </summary>
        public BlackboardTypeCheckLogicalTypeRule(string logicalTypePattern,
                                                  ConcretBaseType filterType,
                                                  IEnumerable<BlackboardLogicalTypeBaseRule> children)
            : base(logicalTypePattern, children, "TypeValidator", ValidationModeEnum.AtLeastOne)
        {
            this.FilterType = filterType;
        }

        #endregion

        #region Properties

        [DataMember]
        public ConcretBaseType FilterType { get; }

        /// <inheritdoc />
        [IgnoreDataMember]
        public override string UniqueIdentity
        {
            get { return base.UniqueIdentity + this.FilterType.AssemblyQualifiedName; }
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="object.Equals(object?)" />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardTypeCheckLogicalTypeRule typeCheck &&
                   EqualityComparer<ConcretBaseType>.Default.Equals(typeCheck.FilterType, this.FilterType);
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.FilterType, base.OnGetHashCode());
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.LogicalTypePattern}] - Filter - {this.FilterType.DisplayName}";
        }

        #endregion
    }
}
