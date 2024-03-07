// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules
{
    using System;
    using System.ComponentModel;
    using System.Numerics;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class BlackboardNumberRangeLogicalTypeRule<TNumber> : BlackboardLogicalTypeBaseRule
         where TNumber : struct, INumber<TNumber>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardNumberRangeLogicalTypeRule{TNumber}"/> class.
        /// </summary>
        public BlackboardNumberRangeLogicalTypeRule(string logicalTypePattern,
                                                    TNumber? minValue,
                                                    TNumber? maxValue) 
            : base(logicalTypePattern)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

        #endregion

        #region Properites

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        [DataMember]
        public TNumber? MinValue { get; }

        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        [DataMember]
        public TNumber? MaxValue { get; }

        #endregion

        #region Methods

        /// <inheritdoc cref="object.Equals(object?)" />
        protected override bool OnEquals(BlackboardLogicalTypeBaseRule other)
        {
            return other is BlackboardNumberRangeLogicalTypeRule<TNumber> range &&
                   range.MinValue == this.MinValue &&
                   range.MaxValue == this.MaxValue;
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.MinValue, this.MaxValue);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.LogicalTypePattern}] - Number Range - [ {this.MinValue} -> {this.MaxValue} [";
        }

        #endregion
    }
}
