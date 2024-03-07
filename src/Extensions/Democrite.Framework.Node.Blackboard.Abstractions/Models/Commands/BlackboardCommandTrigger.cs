// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base class of any trigger action
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class BlackboardCommandTrigger(BlackboardCommandTriggerActionTypeEnum TriggerActionType) : BlackboardCommand(BlackboardCommandTypeEnum.Trigger);

    /// <summary>
    /// Sequence trigger command
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardCommandTriggerSequence<TInput>(Guid SequenceId, TInput? Input = default) : BlackboardCommandTrigger(BlackboardCommandTriggerActionTypeEnum.Sequence)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] [{this.TriggerActionType}] - Trigger Sequence ({this.SequenceId}) - {typeof(TInput)}";
        }
    }
}
