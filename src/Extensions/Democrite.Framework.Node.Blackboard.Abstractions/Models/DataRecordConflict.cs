// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Elvex.Toolbox.Abstractions.Supports;

    using System.Collections.Generic;

    /// <summary>
    /// Data conflict information
    /// </summary>
    [GenerateSerializer]
    public record class DataRecordConflict(IReadOnlyCollection<DataRecordContainer> ConflictItems, DataRecordConflictReasonEnum Reason, string? Details) : ISupportDebugDisplayName
    {
        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"[{this.Reason}] ({this.ConflictItems.Count}) {this.Details}";
        }
    }
}
