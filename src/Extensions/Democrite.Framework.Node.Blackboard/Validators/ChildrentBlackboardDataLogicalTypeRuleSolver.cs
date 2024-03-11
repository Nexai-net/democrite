// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Validators
{
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Elvex.Toolbox.Abstractions.Enums;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Calidation rule managing root with child inherachy
    /// </summary>
    /// <seealso cref="IBlackboardDataLogicalTypeRuleSolver" />
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed class ChildrentBlackboardDataLogicalTypeRuleSolver : IBlackboardDataLogicalTypeRuleValidator
    {
        #region Fields

        private readonly IReadOnlyCollection<IBlackboardDataLogicalTypeRuleValidator> _childrenValidators;
        private readonly IBlackboardDataLogicalTypeRuleValidator _rootValidators;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBlackboardDataLogicalTypeRuleSolver"/> class.
        /// </summary>
        public ChildrentBlackboardDataLogicalTypeRuleSolver(IBlackboardDataLogicalTypeRuleValidator root, IReadOnlyCollection<IBlackboardDataLogicalTypeRuleValidator> validators)
        {
            this._rootValidators = root;
            this._childrenValidators = validators;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public string CollectionGroup
        {
            get { return string.Empty; }
        }

        /// <inheritdoc />
        public ValidationModeEnum ValidationMode
        {
            get { return ValidationModeEnum.None; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> data,
                                                                                IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata)
        {
            var rootIssue = await this._rootValidators.ValidateAsync(data, recordMetadata);

            if (rootIssue is not null)
                return rootIssue;

            foreach (var child in this._childrenValidators)
            {
                var childIssue = await child.ValidateAsync(data, recordMetadata);
                if (childIssue is not null)
                    return childIssue;
            }

            return null;
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"Rule Root '{this._rootValidators.ToDebugDisplayName()}' With children : {this._childrenValidators.Count}";
        }

        #endregion
    }
}
