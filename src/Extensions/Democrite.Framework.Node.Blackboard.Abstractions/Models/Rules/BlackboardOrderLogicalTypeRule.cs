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
    public sealed class BlackboardOrderLogicalTypeRule : BlackboardLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardOrderLogicalTypeRule"/> class.
        /// </summary>
        public BlackboardOrderLogicalTypeRule(string logicalTypePattern, short order) 
            : base(logicalTypePattern)
        {
            this.Order = order;
        }

        #endregion

        #region Properties

        [DataMember]
        public short Order { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardOrderLogicalTypeRule orderRule &&
                   this.Order == orderRule.Order;
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return this.Order.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.LogicalTypePattern}] - Order - {this.Order}";
        }

        #endregion
    }
}
