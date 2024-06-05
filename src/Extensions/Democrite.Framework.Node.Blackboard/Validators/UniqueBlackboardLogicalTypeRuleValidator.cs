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
    using System.Threading.Tasks;

    /// <summary>
    /// Implement validation for rule <see cref="BlackboardLogicalTypeUniqueRule"/>
    /// </summary>
    /// <seealso cref="BlackboardLogicalTypeRuleBaseValidator{BlackboardMaxRecordLogicalTypeRule}" />
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed class UniqueBlackboardLogicalTypeRuleValidator : BlackboardLogicalTypeRuleBaseValidator<BlackboardLogicalTypeUniqueRule>
    {
        #region Fields
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueBlackboardLogicalTypeRuleValidator"/> class.
        /// </summary>
        public UniqueBlackboardLogicalTypeRuleValidator(BlackboardLogicalTypeUniqueRule rule)
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

            var logicalExisting = recordMetadata.FirstOrDefault(r => string.Equals(record.LogicalType, r.Value.LogicalType));

            if (logicalExisting.Key != default && logicalExisting.Key != record.Uid)
            {
                if (this.Rule.AllowReplacement)
                {
                    issue = new MaxRecordBlackboardProcessingRuleIssue(1,
                                                                       logicalExisting.AsEnumerable().Select(kv => kv.Value).ToArray(),
                                                                       record,
                                                                       BlackboardProcessingResolutionLimitTypeEnum.KeepNewest,
                                                                       BlackboardProcessingResolutionRemoveTypeEnum.Remove);
                }
                else
                {
                    issue = new UniqueBlackboardProcessingRuleIssue(record);
                }
            }

            return ValueTask.FromResult(issue);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"rule: unique";
        }

        #endregion
    }
}
