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
    public sealed class BlackboardRemainOnSealedLogicalTypeRule : BlackboardLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardRemainOnSealedLogicalTypeRule"/> class.
        /// </summary>
        public BlackboardRemainOnSealedLogicalTypeRule(string logicalTypePattern) 
            : base(logicalTypePattern)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardRemainOnSealedLogicalTypeRule;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return 42;
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.LogicalTypePattern}] - Remain On Sealed";
        }

        #endregion
    }
}
