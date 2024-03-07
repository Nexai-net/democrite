// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;

    /// <summary>
    /// Validate a record data
    /// </summary>
    public interface IBlackboardDataLogicalTypeRuleSolver
    {
        /// <summary>
        /// Validates <paramref name="data"/> in the context of the blacboard with all the rule setups.
        /// </summary>
        /// <returns>
        ///     Issue found, stop at 1st the founded; I no issue return null;
        /// </returns>
        ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> data,
                                                                   IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata);
    }
}
