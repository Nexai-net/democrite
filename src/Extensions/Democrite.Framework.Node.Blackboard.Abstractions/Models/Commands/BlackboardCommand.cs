// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;

    using Elvex.Toolbox.Abstractions.Supports;

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
    public sealed record class BlackboardCommandRejectAction(BlackboardProcessingIssue SourceIssue) : BlackboardCommand(BlackboardCommandTypeEnum.Reject)
    {
        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static BlackboardCommandRejectAction()
        {
            Default = new BlackboardCommandRejectAction(BlackboardNotSupportedProcessingIssue.Default);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default reject command.
        /// </summary>
        public static BlackboardCommandRejectAction Default { get; }

        #endregion

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - Reject an source action that cause the issue {this.SourceIssue.ToDebugDisplayName()}";
        }
    }

    /// <summary>
    /// Command used to deferred response any source action
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardCommandDeferredResponse(Guid QueryUid) : BlackboardCommand(BlackboardCommandTypeEnum.Deferred)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - Response will be send later";
        }
    }

    /// <summary>
    /// Command used to directly provide a response 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardCommandResponse<TResponse>(TResponse Response, Guid QueryUid) : BlackboardCommand(BlackboardCommandTypeEnum.Response)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - Direct Response";
        }
    }

    /// <summary>
    /// Command used to retry deferred query
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardCommandRetryDeferredQuery() : BlackboardCommand(BlackboardCommandTypeEnum.RetryDeferred)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - Retry pending query";
        }
    }

    /// <summary>
    /// Command used to change the blackboard life status
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandLifeStatusChange(BlackboardLifeStatusEnum NewStatus, BlackboardLifeStatusEnum OldStatus) : BlackboardCommand(BlackboardCommandTypeEnum.LifeStatusChange)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - Life Status change {this.NewStatus}";
        }
    }

    /// <summary>
    /// Command used to change the blackboard life status
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardCommandLifeInitializeChange(BlackboardLifeStatusEnum OldStatus, IReadOnlyCollection<DataRecordContainer>? InitData = null) : BlackboardCommandLifeStatusChange(BlackboardLifeStatusEnum.Running, OldStatus)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - Life Init Status change {this.NewStatus}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardCommandSignalSubscription(Guid SignalUid, string DisplayName, bool IsDoor, BlackboardCommandSignalTypeEnum Type) : BlackboardCommand(BlackboardCommandTypeEnum.Signal)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[Type:{this.Type}] - {this.SignalUid} ({this.DisplayName})";
        }
    }
}
