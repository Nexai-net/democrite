// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Validators
{
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;
    using Elvex.Toolbox.Abstractions.Enums;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Default validator provider
    /// </summary>
    /// <seealso cref="IBlackboardDataLogicalTypeRuleValidatorProvider" />
    public class BlackboardDataLogicalTypeRuleValidatorProvider : IBlackboardDataLogicalTypeRuleValidatorProvider
    {
        #region Fields

        private static readonly Type s_numberRangeRuleTraits = typeof(BlackboardNumberRangeLogicalTypeRule<>);
        private static readonly Type s_numberRangeValidatorTraits = typeof(NumberRangeBlackboardLogicalTypeRuleValidator<>);

        #endregion

        #region Methods

        /// <inheritdoc />
        public IBlackboardDataLogicalTypeRuleSolver Create(Regex logicalTypeFilterPattern, IReadOnlyCollection<BlackboardLogicalTypeBaseRule> rules)
        {
            if (rules is null || rules.Count == 0)
                return NullBlackboardDataLogicalTypeRuleSolver.Instance;

            if (rules.Count == 1)
                return Build(logicalTypeFilterPattern, rules.Single());

            return new GroupBlackboardDataLogicalTypeRuleSolver(ValidationModeEnum.All,
                                                                string.Empty,
                                                                Builds(logicalTypeFilterPattern, rules));
        }

        /// <summary>
        /// Builds the specified validator dedicated to rule <paramref name="rule"/>
        /// </summary>
        private IReadOnlyCollection<IBlackboardDataLogicalTypeRuleValidator> Builds(Regex logicalTypeFilterPattern, IEnumerable<BlackboardLogicalTypeBaseRule> rules)
        {
            return rules.GroupBy(kv => kv is BlackboardHierarchyLogicalTypeBaseRule hierarchy ? (hierarchy.CollectionGroup, hierarchy.ValidationMode) : ("", ValidationModeEnum.All))
                        .Select(grp =>
                        {
                            var validators = grp.Select(g => Build(logicalTypeFilterPattern, g)).ToArray();

                            if (validators.Count() == 1)
                                return validators.Single();

                            return new GroupBlackboardDataLogicalTypeRuleSolver(grp.Key.ValidationMode ?? ValidationModeEnum.All,
                                                                                grp.Key.CollectionGroup ?? string.Empty,
                                                                                validators);
                        }).ToArray();
        }

        /// <summary>
        /// Builds the specified validator dedicated to rule <paramref name="rule"/>
        /// </summary>
        private IBlackboardDataLogicalTypeRuleValidator Build(Regex logicalTypeFilterPattern, BlackboardLogicalTypeBaseRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);
            var rootValidator = BuildRootValidator(logicalTypeFilterPattern, rule);

            if (rootValidator is not null && rule is BlackboardHierarchyLogicalTypeBaseRule hierarchy && hierarchy.Children is not null && hierarchy.Children.Any())
                return new ChildrentBlackboardDataLogicalTypeRuleSolver(rootValidator, Builds(logicalTypeFilterPattern, hierarchy.Children));

            if (rootValidator is not null)
                return rootValidator!;

            throw new NotSupportedException("Rule not supported plz contact the framework dev to add this validation or inherite from current class and add yourself the BuildRootValidator");
        }

        /// <summary>
        /// Builds the root validator.
        /// </summary>
        protected virtual IBlackboardDataLogicalTypeRuleValidator? BuildRootValidator(Regex logicalTypeFilterPattern, BlackboardLogicalTypeBaseRule rule)
        {
            if (rule is BlackboardMaxRecordLogicalTypeRule max)
                return new MaxRecordBlackboardLogicalTypeRuleValidator(max, logicalTypeFilterPattern);
            else if (rule is BlackboardTypeCheckLogicalTypeRule typeCheck)
                return new DataTypeBlackboardLogicalTypeRuleValidator(typeCheck);
            else if (rule is BlackboardRegexLogicalTypeRule regexRule)
                return new StringRegexBlackboardLogicalTypeRuleValidator(regexRule);
            else if (rule is BlackboardLogicalTypeUniqueRule uniqueRule)
                return new UniqueBlackboardLogicalTypeRuleValidator(uniqueRule);
            else if (rule.GetType().GetGenericTypeDefinition() == s_numberRangeRuleTraits)
                return (IBlackboardDataLogicalTypeRuleValidator)Activator.CreateInstance(s_numberRangeValidatorTraits.MakeGenericType(rule.GetType().GetGenericArguments()), new object[] { rule })!;

            return null;
        }

        #endregion
    }
}
