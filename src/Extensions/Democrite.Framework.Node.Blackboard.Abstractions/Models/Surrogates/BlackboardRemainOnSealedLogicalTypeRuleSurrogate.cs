// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    [GenerateSerializer]
    public record struct BlackboardRemainOnSealedLogicalTypeRuleSurrogate(string LogicalTypePattern) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardRemainOnSealedLogicalTypeRuleConverter : IConverter<BlackboardRemainOnSealedLogicalTypeRule, BlackboardRemainOnSealedLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardRemainOnSealedLogicalTypeRule ConvertFromSurrogate(in BlackboardRemainOnSealedLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardRemainOnSealedLogicalTypeRule(surrogate.LogicalTypePattern);
        }

        /// <inheritdoc />
        public BlackboardRemainOnSealedLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardRemainOnSealedLogicalTypeRule value)
        {
            return new BlackboardRemainOnSealedLogicalTypeRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
            };
        }
    }
}
