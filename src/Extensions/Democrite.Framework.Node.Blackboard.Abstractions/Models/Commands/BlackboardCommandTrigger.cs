// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.Models;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base class of any trigger action
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract record class BlackboardCommandTrigger(BlackboardCommandTriggerActionTypeEnum TriggerActionType) : BlackboardCommand(BlackboardCommandTypeEnum.Trigger)
    {
        /// <inheritdoc />
        public sealed override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] [{this.TriggerActionType}] - {OnDebugDisplayName()}";
        }

        /// <inheritdoc cref="ISupportDebugDisplayName.ToDebugDisplayName" />
        protected abstract object OnDebugDisplayName();
    }

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
        protected override string OnDebugDisplayName()
        {
            return $"Trigger Sequence ({this.SequenceId}) - {typeof(TInput)}";
        }
    }

    /// <summary>
    /// Signal trigger command
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandTriggerSignal(SignalId SignalId) : BlackboardCommandTrigger(BlackboardCommandTriggerActionTypeEnum.Signal)
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardCommandTriggerSignal"/> class.
        /// </summary>
        protected BlackboardCommandTriggerSignal(SignalId SignalId, bool carryData)
            : this(SignalId)
        {
            this.CarryData = carryData;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        public bool CarryData { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return $"Trigger Signal {this.SignalId.Name} ({this.SignalId.Uid})";
        }

        #endregion
    }

    /// <summary>
    /// Signal trigger command
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardCommandTriggerSignal<TData>(SignalId SignalId, in TData Data) : BlackboardCommandTriggerSignal(SignalId, true)
        where TData : struct
    {
        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return $"Trigger Signal {this.SignalId.Name} ({this.SignalId.Uid}) with Data {typeof(TData)} = {this.Data}";
        }
    }
}
