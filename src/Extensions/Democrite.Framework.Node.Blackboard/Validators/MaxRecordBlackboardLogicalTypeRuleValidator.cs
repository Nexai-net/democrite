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
    /// Implement validation for rule <see cref="BlackboardMaxRecordLogicalTypeRule"/>
    /// </summary>
    /// <seealso cref="BlackboardLogicalTypeRuleBaseValidator{BlackboardMaxRecordLogicalTypeRule}" />
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed class MaxRecordBlackboardLogicalTypeRuleValidator : BlackboardLogicalTypeRuleBaseValidator<BlackboardMaxRecordLogicalTypeRule>
    {
        #region Fields

        private readonly Regex _logicalTypeFilterPattern;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxRecordBlackboardLogicalTypeRuleValidator{BlackboardMaxRecordLogicalTypeRule}"/> class.
        /// </summary>
        public MaxRecordBlackboardLogicalTypeRuleValidator(BlackboardMaxRecordLogicalTypeRule rule, Regex logicalTypeFilterPattern)
            : base(rule)
        {
            this._logicalTypeFilterPattern = logicalTypeFilterPattern;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> data, IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata)
        {
            BlackboardProcessingIssue? result = null;
            if (!recordMetadata.ContainsKey(data.Uid) && this.Rule.MaxRecord > 0)
            {
                var conflictRecords = new List<BlackboardRecordMetadata>(this.Rule.MaxRecord); 

                foreach (var record in recordMetadata)
                {
                    if (record.Value.Status != RecordStatusEnum.Decommissioned && this._logicalTypeFilterPattern.IsMatch(record.Value.LogicalType))
                    {
                        conflictRecords.Add(record.Value);

                        if (conflictRecords.Count >= this.Rule.MaxRecord)
                        {
                            result = new MaxRecordBlackboardProcessingRuleIssue((ushort)this.Rule.MaxRecord,
                                                                                conflictRecords,
                                                                                data,
                                                                                this.Rule.PreferenceResolution,
                                                                                this.Rule.RemoveResolution);
                            break;
                        }
                    }
                }
            }

            return ValueTask.FromResult(result);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"rule: MaxRecord < {this.Rule.MaxRecord}" + (this.Rule.IncludeDecommissioned ? " Include Decommissioned" : string.Empty);
        }

        #endregion
    }
}
