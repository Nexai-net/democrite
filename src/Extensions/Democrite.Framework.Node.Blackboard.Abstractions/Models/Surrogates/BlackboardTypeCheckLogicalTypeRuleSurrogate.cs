// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    [GenerateSerializer]
    public record struct BlackboardTypeCheckLogicalTypeRuleSurrogate(string LogicalTypePattern,
                                                                     IConcretTypeSurrogate FilterType,
                                                                     IReadOnlyCollection<IBlackboardLogicalTypeBaseRuleSurrogate> Children) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardTypeCheckLogicalTypeRuleConverter : IConverter<BlackboardTypeCheckLogicalTypeRule, BlackboardTypeCheckLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardTypeCheckLogicalTypeRule ConvertFromSurrogate(in BlackboardTypeCheckLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardTypeCheckLogicalTypeRule(surrogate.LogicalTypePattern,
                                                          ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.FilterType),
                                                          BlackboardLogicalTypeBaseRuleSurrogateConverter.ConvertFromSurrogate(surrogate.Children));
        }

        /// <inheritdoc />
        public BlackboardTypeCheckLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardTypeCheckLogicalTypeRule value)
        {
            return new BlackboardTypeCheckLogicalTypeRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                FilterType = ConcretBaseTypeConverter.ConvertToSurrogate(value.FilterType),
                Children = BlackboardLogicalTypeBaseRuleSurrogateConverter.ConvertToSurrogate(value.Children).ToReadOnly()
            };
        }
    }
}
