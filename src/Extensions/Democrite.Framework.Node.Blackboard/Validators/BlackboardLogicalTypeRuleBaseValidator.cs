// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Validators
{
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;
    using Elvex.Toolbox.Abstractions.Enums;

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class for all the <see cref="IBlackboardDataLogicalTypeRuleValidator"/>
    /// </summary>
    /// <seealso cref="IBlackboardDataLogicalTypeRuleValidator" />
    public abstract class BlackboardLogicalTypeRuleBaseValidator<TRule> : IBlackboardDataLogicalTypeRuleValidator
        where TRule : BlackboardLogicalTypeBaseRule
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardLogicalTypeRuleBaseValidator"/> class.
        /// </summary>
        protected BlackboardLogicalTypeRuleBaseValidator(TRule rule)
        {
            this.CollectionGroup = (rule as BlackboardHierarchyLogicalTypeBaseRule)?.CollectionGroup ?? string.Empty;
            this.ValidationMode = (rule as BlackboardHierarchyLogicalTypeBaseRule)?.ValidationMode ?? ValidationModeEnum.All;

            this.Rule = rule;
        }

        #endregion Property

        /// <inheritdoc />
        public string CollectionGroup { get; }

        /// <inheritdoc />
        public ValidationModeEnum ValidationMode { get; }

        /// <summary>
        /// Gets the rule.
        /// </summary>
        public TRule Rule { get; }

        #region Methods

        /// <inheritdoc />
        public abstract ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> data, IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata);

        /// <inheritdoc />
        public abstract string ToDebugDisplayName();

        #endregion
    }
}
