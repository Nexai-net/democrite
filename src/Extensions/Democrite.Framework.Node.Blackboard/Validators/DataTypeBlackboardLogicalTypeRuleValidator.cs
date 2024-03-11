// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Validators
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;
    using Elvex.Toolbox;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Check if the data type is the correct one
    /// </summary>
    /// <seealso cref="BlackboardLogicalTypeRuleBaseValidator{BlackboardTypeCheckLogicalTypeRule}" />
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed class DataTypeBlackboardLogicalTypeRuleValidator : BlackboardLogicalTypeRuleBaseValidator<BlackboardTypeCheckLogicalTypeRule>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeBlackboardLogicalTypeRuleValidator"/> class.
        /// </summary>
        public DataTypeBlackboardLogicalTypeRuleValidator(BlackboardTypeCheckLogicalTypeRule rule)
            : base(rule)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />  
        public override ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> data,
                                                                                   IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata)
        {
            if (NoneType.IsEqualTo<TData>())
                return ValueTask.FromResult<BlackboardProcessingIssue?>(null);

            var type = this.Rule.FilterType.ToType();

            BlackboardProcessingIssue? issue = null;
            if (!typeof(TData).IsAssignableTo(type))
                issue = DataTypeBlackboardProcessingRuleIssue.Create(type, data);

            return ValueTask.FromResult(issue);
        }

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"rule: type Check : {this.Rule.FilterType.DisplayName}";
        }

        #endregion
    }
}
