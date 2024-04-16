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
    public sealed record class RejectActionBlackboardCommand(BlackboardProcessingIssue SourceIssue) : BlackboardCommand(BlackboardCommandTypeEnum.Reject)
    {
        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static RejectActionBlackboardCommand()
        {
            Default = new RejectActionBlackboardCommand(BlackboardNotSupportedProcessingIssue.Default);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default reject command.
        /// </summary>
        public static RejectActionBlackboardCommand Default { get; }

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
    public sealed record class DeferredResponseBlackboardCommand(Guid QueryUid) : BlackboardCommand(BlackboardCommandTypeEnum.Deferred)
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
    public sealed record class ResponseBlackboardCommand<TResponse>(TResponse Response, Guid QueryUid) : BlackboardCommand(BlackboardCommandTypeEnum.Reponse)
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
    public sealed record class RetryDeferredQueryBlackboardCommand() : BlackboardCommand(BlackboardCommandTypeEnum.RetryDeferred)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - Retry pending query";
        }
    }

    /// <summary>
    /// Command used to request a stae change
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class StateRequestedChangeBlackboardCommand(BlackboardLifeStatusEnum NewState) : BlackboardCommand(BlackboardCommandTypeEnum.StateRequestedChange)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.ActionType}] - State change {this.NewState}";
        }
    }
}
