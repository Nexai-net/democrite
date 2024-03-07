// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;
    using System.Text.RegularExpressions;

    [GenerateSerializer]
    public record struct BlackboardRegexLogicalTypeRuleSurrogate(string LogicalTypePattern,
                                                                 string MatchRegex,
                                                                 RegexOptions RegexOptions,
                                                                 IReadOnlyCollection<IBlackboardLogicalTypeBaseRuleSurrogate> Children) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardRegexLogicalTypeRuleConverter : IConverter<BlackboardRegexLogicalTypeRule, BlackboardRegexLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardRegexLogicalTypeRule ConvertFromSurrogate(in BlackboardRegexLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardRegexLogicalTypeRule(surrogate.LogicalTypePattern, surrogate.MatchRegex, surrogate.RegexOptions);
        }

        /// <inheritdoc />
        public BlackboardRegexLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardRegexLogicalTypeRule value)
        {
            return new BlackboardRegexLogicalTypeRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                MatchRegex = value.MatchRegex,
                RegexOptions = value.RegexOptions,
            };
        }
    }
}
