// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Elvex.Toolbox.Abstractions.Supports;

    using System;

    /// <summary>
    /// State used by <see cref="SequenceExecutorVGrain"/> to execute the sequence
    /// </summary>
    [Serializable]
    internal sealed class SequenceExecutorState : ISupportDebugDisplayName, IEquatable<SequenceExecutorState>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorState"/> class.
        /// </summary>
        public SequenceExecutorState(Guid sequenceDefinitionId,
                                     Guid flowUid,
                                     DateTime startAt,
                                     Guid? instanceId = null)
            : this(sequenceDefinitionId,
                   flowUid,
                   instanceId ?? Guid.NewGuid(),
                   null,
                   startAt)
        {

        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SequenceExecutorState"/> class from being created.
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        internal SequenceExecutorState(Guid sequenceDefinitionId,
                                       Guid flowUid,
                                       Guid instanceId,
                                       SequenceExecutorExecThreadState? mainThread,
                                       DateTime startAt)
        {
            this.SequenceDefinitionId = sequenceDefinitionId;
            this.InstanceId = instanceId;
            this.MainThread = mainThread;
            this.FlowUid = flowUid;
            this.StartAt = startAt;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the sequence definition identifier.
        /// </summary>
        public Guid SequenceDefinitionId { get; }

        /// <summary>
        /// Gets the flow identifier.
        /// </summary>
        /// <remarks>
        ///     Unique id shared throught all the vgrain to identify the process work
        /// </remarks>
        public Guid FlowUid { get; }

        /// <summary>
        /// Gets the UTC date/time the sequence startAt.
        /// </summary>
        public DateTime StartAt { get; }

        /// <summary>
        /// Gets the current instance identifier.
        /// </summary>
        public Guid InstanceId { get; }

        /// <summary>
        /// Gets the main execution threads.
        /// </summary>
        public SequenceExecutorExecThreadState? MainThread { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes state with input
        /// </summary>
        internal void Initialize<TInput>(IExecutionContext executionContext, SequenceDefinition sequenceDefinition, TInput? input)
        {
            this.MainThread = new SequenceExecutorExecThreadState(executionContext.FlowUID,
                                                                  this.SequenceDefinitionId,
                                                                  executionContext.CurrentExecutionId,
                                                                  executionContext.ParentExecutionId,
                                                                  sequenceDefinition,
                                                                  input);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return string.Format($"[{this.InstanceId}][SequenceDefinitionId({this.SequenceDefinitionId})] - Flow {this.FlowUid} - {this.MainThread?.ToDebugDisplayName()}");
        }

        /// <inheritdoc />
        public bool Equals(SequenceExecutorState? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this)) 
                return true;

            return this.SequenceDefinitionId == other.SequenceDefinitionId &&
                   this.InstanceId == other.InstanceId &&
                   this.FlowUid == other.FlowUid &&
                   (this.MainThread?.Equals(other.MainThread) ?? other.MainThread is null);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is SequenceExecutorState seqState)
                return Equals(seqState);

            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.SequenceDefinitionId,
                                    this.InstanceId,
                                    this.FlowUid,
                                    this.MainThread);
        }

        #endregion
    }
}
