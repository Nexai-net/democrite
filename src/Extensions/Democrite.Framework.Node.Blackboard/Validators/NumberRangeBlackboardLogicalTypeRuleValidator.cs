// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Validators
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using System.Threading.Tasks;

    /// <summary>
    /// Implement validation for rule <see cref="BlackboardNumberRangeLogicalTypeRule{TNumber}"/>
    /// </summary>
    /// <seealso cref="BlackboardLogicalTypeRuleBaseValidator{BlackboardMaxRecordLogicalTypeRule}" />
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed class NumberRangeBlackboardLogicalTypeRuleValidator<TNumber> : BlackboardLogicalTypeRuleBaseValidator<BlackboardNumberRangeLogicalTypeRule<TNumber>>
         where TNumber : struct, INumber<TNumber>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxRecordBlackboardLogicalTypeRuleValidator{BlackboardMaxRecordLogicalTypeRule}"/> class.
        /// </summary>
        public NumberRangeBlackboardLogicalTypeRuleValidator(BlackboardNumberRangeLogicalTypeRule<TNumber> rule)
            : base(rule)
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> record,
                                                                                   IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata)
        {
            BlackboardProcessingIssue? issue = null;

            if (record.Data is TNumber number)
            {
                if (this.Rule.MinValue is not null && number < this.Rule.MinValue.Value)
                    issue = NumberRangeBlackboardProcessingRuleIssue.Create(this.Rule.MinValue, this.Rule.MaxValue, record);
                else if (this.Rule.MaxValue is not null && number >= this.Rule.MaxValue.Value)
                    issue = NumberRangeBlackboardProcessingRuleIssue.Create(this.Rule.MinValue, this.Rule.MaxValue, record);
            }
            else if (record.Data is not null)
            {
                issue = DataTypeBlackboardProcessingRuleIssue.Create(typeof(TNumber), record);
            }

            return ValueTask.FromResult(issue);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"rule: number range : [Min: {this.Rule.MinValue} -> Max: {this.Rule.MaxValue} [";
        }

        #endregion
    }
}
