// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Validators
{
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;

    using Elvex.Toolbox.Abstractions.Enums;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Group validation rule and solve the validation usign <see cref="ValidationModeEnum"/>
    /// </summary>
    /// <seealso cref="IBlackboardDataLogicalTypeRuleSolver" />
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed class GroupBlackboardDataLogicalTypeRuleSolver : IBlackboardDataLogicalTypeRuleValidator
    {
        #region Fields

        private readonly IReadOnlyCollection<IBlackboardDataLogicalTypeRuleValidator> _validators;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBlackboardDataLogicalTypeRuleSolver"/> class.
        /// </summary>
        public GroupBlackboardDataLogicalTypeRuleSolver(ValidationModeEnum mode,
                                                        string collectionGroup,
                                                        IReadOnlyCollection<IBlackboardDataLogicalTypeRuleValidator> validators)
        {
            this.ValidationMode = mode;
            this.CollectionGroup = collectionGroup;
            this._validators = validators;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public string CollectionGroup { get; }

        /// <inheritdoc />
        public ValidationModeEnum ValidationMode { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"rule: Group {this.CollectionGroup} (Mode:{this.ValidationMode}) nb: {this._validators.Count}";
        }

        /// <inheritdoc />
        public async ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> data, IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata)
        {
            List<BlackboardProcessingIssue>? issues = null;

            foreach (var rule in this._validators)
            {
                var ruleIssue = await rule.ValidateAsync(data, recordMetadata);

                if (this.ValidationMode == ValidationModeEnum.AtLeastOne)
                {
                    if (ruleIssue is null)
                    {
                        return null;
                    }
                    else
                    {
                        issues ??= new List<BlackboardProcessingIssue>();
                        issues.Add(ruleIssue);
                    }
                }

                if (this.ValidationMode == ValidationModeEnum.All && ruleIssue is not null)
                    return ruleIssue;

                if (this.ValidationMode == ValidationModeEnum.NoOne && ruleIssue is null)
                    return new BlackboardProcessingGenericRuleIssues($"No rule mus be valid, Rule valid {rule.ToDebugDisplayName()}");
            }

            if (issues is not null && issues.Count > 0)
                return new BlackboardAggregateProcessingIssue(issues);

            return null;
        }

        #endregion
    }
}
