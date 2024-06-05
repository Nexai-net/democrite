// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class BlackboardLogicalTypeUniqueRule : BlackboardLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardLogicalTypeUniqueRule"/> class.
        /// </summary>
        public BlackboardLogicalTypeUniqueRule(string logicalTypePattern, bool allowReplacement)
            : base(logicalTypePattern)
        {
            this.AllowReplacement = allowReplacement;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether [allow replacement].
        /// </summary>
        public bool AllowReplacement { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardLogicalTypeUniqueRule otherUnique &&
                   otherUnique.AllowReplacement == this.AllowReplacement;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.AllowReplacement.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.LogicalTypePattern}] - Unique";
        }

        #endregion
    }
}
