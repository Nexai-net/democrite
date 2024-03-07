// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;

    using System.Text.RegularExpressions;

    /// <summary>
    /// Provide data validator
    /// </summary>
    public interface IBlackboardDataLogicalTypeRuleValidatorProvider
    {
        /// <summary>
        /// Creates the specified rule whith childrens
        /// </summary>
        IBlackboardDataLogicalTypeRuleSolver Create(Regex logicalTypeFilterPattern, IReadOnlyCollection<BlackboardLogicalTypeBaseRule> rules);
    }
}
