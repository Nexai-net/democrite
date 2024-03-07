// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using System.Numerics;

    [GenerateSerializer]
    public record struct BlackboardNumberRangeLogicalTypeRuleSurrogate<TNumber>(string LogicalTypePattern,
                                                                                TNumber? MinValue,
                                                                                TNumber? MaxValue,
                                                                                IReadOnlyCollection<IBlackboardLogicalTypeBaseRuleSurrogate> Children) :  IBlackboardLogicalTypeBaseRuleSurrogate 
        where TNumber : struct, INumber<TNumber>;

    [RegisterConverter]
    public sealed class BlackboardNumberRangeLogicalTypeRuleConverter<TNumber> : IConverter<BlackboardNumberRangeLogicalTypeRule<TNumber>, BlackboardNumberRangeLogicalTypeRuleSurrogate<TNumber>>
                 where TNumber : struct, INumber<TNumber>
    {
        /// <inheritdoc />
        public BlackboardNumberRangeLogicalTypeRule<TNumber> ConvertFromSurrogate(in BlackboardNumberRangeLogicalTypeRuleSurrogate<TNumber> surrogate)
        {
            return new BlackboardNumberRangeLogicalTypeRule<TNumber>(surrogate.LogicalTypePattern, surrogate.MinValue, surrogate.MaxValue);
        }

        /// <inheritdoc />
        public BlackboardNumberRangeLogicalTypeRuleSurrogate<TNumber> ConvertToSurrogate(in BlackboardNumberRangeLogicalTypeRule<TNumber> value)
        {
            return new BlackboardNumberRangeLogicalTypeRuleSurrogate<TNumber>()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                MinValue = value.MinValue,
                MaxValue = value.MaxValue,
            };
        }
    }
}
