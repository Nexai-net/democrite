// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base class of every controller orders
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class BlackboardCommand(BlackboardCommandTypeEnum ActionType) : ISupportDebugDisplayName
    {
        /// <inheritdoc />
        public abstract string ToDebugDisplayName();
    }

    /// <summary>
    /// Command used to reject any source action
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class RejectActionBlackboardCommand(BlackboardProcessingIssue SourceIssue) : BlackboardCommand(BlackboardCommandTypeEnum.Reject)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - Reject an source action that cause the issue {this.SourceIssue.ToDebugDisplayName()}";
        }
    }
}
