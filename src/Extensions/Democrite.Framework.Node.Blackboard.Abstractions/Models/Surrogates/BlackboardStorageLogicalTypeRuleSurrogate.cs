// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    [GenerateSerializer]
    public record struct BlackboardStorageLogicalTypeRuleSurrogate(string LogicalTypePattern, BlackboardStorageDefinition Storage) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardStorageLogicalTypeRuleConverter : IConverter<BlackboardStorageLogicalTypeRule, BlackboardStorageLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardStorageLogicalTypeRule ConvertFromSurrogate(in BlackboardStorageLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardStorageLogicalTypeRule(surrogate.LogicalTypePattern, surrogate.Storage);
        }

        /// <inheritdoc />
        public BlackboardStorageLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardStorageLogicalTypeRule value)
        {
            return new BlackboardStorageLogicalTypeRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                Storage = value.Storage
            };
        }
    }
}
