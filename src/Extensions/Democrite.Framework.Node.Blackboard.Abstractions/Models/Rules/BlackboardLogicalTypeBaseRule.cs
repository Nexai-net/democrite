// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules
{
    using Elvex.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    [DebuggerDisplay("{ToDebugDisplayName()}")]

    [KnownType(typeof(BlackboardMaxRecordLogicalTypeRule))]
    [KnownType(typeof(BlackboardNumberRangeLogicalTypeRule<>))]
    [KnownType(typeof(BlackboardOrderLogicalTypeRule))]
    [KnownType(typeof(BlackboardRegexLogicalTypeRule))]
    [KnownType(typeof(BlackboardStorageLogicalTypeRule))]
    [KnownType(typeof(BlackboardTypeCheckLogicalTypeRule))]
    public abstract class BlackboardLogicalTypeBaseRule : IEquatable<BlackboardLogicalTypeBaseRule>, ISupportDebugDisplayName
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardLogicalTypeBaseRule"/> class.
        /// </summary>
        protected BlackboardLogicalTypeBaseRule(string logicalTypePattern)
        {
            ArgumentException.ThrowIfNullOrEmpty(logicalTypePattern);

            this.LogicalTypePattern = string.IsNullOrEmpty(logicalTypePattern) ? ".*" : logicalTypePattern;
        }

        #endregion

        #region Properties

        [DataMember]
        public string LogicalTypePattern { get; }

        /// <summary>
        /// Gets the unique identity.
        /// </summary>
        [IgnoreDataMember]
        public virtual string UniqueIdentity
        {
            get { return this.GetType().FullName!.ToString(); }
        }

        #endregion

        #region Methods
        
        /// <inheritdoc />
        public bool Equals(BlackboardLogicalTypeBaseRule? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return string.Equals(this.LogicalTypePattern, other.LogicalTypePattern) &&
                   OnEquals(other);
        }

        /// <inheritdoc cref="object.Equals(object?)" />
        protected abstract bool OnEquals(BlackboardLogicalTypeBaseRule other);

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.LogicalTypePattern,
                                    OnGetHashCode());
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract int OnGetHashCode();

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is BlackboardLogicalTypeBaseRule ruleBase)
                return Equals(ruleBase);
            return false;
        }

        /// <inheritdoc />
        public abstract string ToDebugDisplayName();

        #endregion
    }
}
