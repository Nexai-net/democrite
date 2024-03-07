// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    [GenerateSerializer]
    public record struct BlackboardMaxRecordLogicalTypeRuleSurrogate(string LogicalTypePattern,
                                                                     bool IncludeDecommissioned,
                                                                     short MaxRecord,
                                                                     BlackboardProcessingResolutionLimitTypeEnum? PreferenceResolution,
                                                                     BlackboardProcessingResolutionRemoveTypeEnum? RemoveResolution) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardMaxRecordLogicalTypeRuleConverter : IConverter<BlackboardMaxRecordLogicalTypeRule, BlackboardMaxRecordLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardMaxRecordLogicalTypeRule ConvertFromSurrogate(in BlackboardMaxRecordLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardMaxRecordLogicalTypeRule(surrogate.LogicalTypePattern,
                                                          surrogate.IncludeDecommissioned,
                                                          surrogate.MaxRecord,
                                                          surrogate.PreferenceResolution,
                                                          surrogate.RemoveResolution);
        }

        /// <inheritdoc />
        public BlackboardMaxRecordLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardMaxRecordLogicalTypeRule value)
        {
            return new BlackboardMaxRecordLogicalTypeRuleSurrogate()
            {
                IncludeDecommissioned = value.IncludeDecommissioned,
                LogicalTypePattern = value.LogicalTypePattern,
                MaxRecord = value.MaxRecord,
                PreferenceResolution = value.PreferenceResolution,
                RemoveResolution = value.RemoveResolution
            };
        }
    }
}
