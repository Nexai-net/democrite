// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Elvex.Toolbox.Abstractions.Enums;
    using Elvex.Toolbox.Abstractions.Supports;

    /// <summary>
    /// Validate a record data
    /// </summary>
    public interface IBlackboardDataLogicalTypeRuleValidator : IBlackboardDataLogicalTypeRuleSolver, ISupportDebugDisplayName
    {
        /// <summary>
        /// Gets the collection group.
        /// </summary>
        string CollectionGroup { get; }

        /// <summary>
        /// Gets the validation mode.
        /// </summary>
        ValidationModeEnum ValidationMode { get; }
    }
}
