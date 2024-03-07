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
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Implement validation for rule <see cref="BlackboardRegexLogicalTypeRule"/>
    /// </summary>
    /// <seealso cref="BlackboardLogicalTypeRuleBaseValidator{BlackboardMaxRecordLogicalTypeRule}" />
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed class StringRegexBlackboardLogicalTypeRuleValidator : BlackboardLogicalTypeRuleBaseValidator<BlackboardRegexLogicalTypeRule>
    {
        #region Fields

        private readonly Regex _localRegex;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StringRegexBlackboardLogicalTypeRuleValidator"/> class.
        /// </summary>
        public StringRegexBlackboardLogicalTypeRuleValidator(BlackboardRegexLogicalTypeRule rule)
            : base(rule)
        {
            this._localRegex = new Regex(rule.MatchRegex, rule.RegexOptions);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> record,
                                                                                   IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata)
        {
            BlackboardProcessingIssue? issue = null;

            // record.Data is null => Not the job of this validator to validate if null is a correct value
            if (record.Data is string str)
            {
                if (!this._localRegex.IsMatch(str))
                    issue = new StringRegexBlackboardProcessingRuleIssue(this._localRegex.ToString(), record);
            }
            else if (record.Data is not null)
            {
                issue = DataTypeBlackboardProcessingRuleIssue.Create(typeof(string), record);
            }

            return ValueTask.FromResult(issue);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"rule: string regex : {this._localRegex}";
        }

        #endregion
    }
}
