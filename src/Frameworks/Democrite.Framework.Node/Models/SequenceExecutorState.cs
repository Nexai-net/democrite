// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using System;

    /// <summary>
    /// State used by <see cref="SequenceExecutorVGrain"/> to execute the sequence
    /// </summary>
    [Serializable]
    [GenerateSerializer]
    public sealed class SequenceExecutorState : Freezable, ISupportDebugDisplayName
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorState"/> class.
        /// </summary>
        /// <remarks>
        ///     Default CTOR that must be only used by the storage default loader
        /// </remarks>
        public SequenceExecutorState()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorState"/> class.
        /// </summary>
        public SequenceExecutorState(Guid sequenceDefinitionId, Guid flowUid, Guid? instanceId = null)
            : this(sequenceDefinitionId, flowUid, instanceId ?? Guid.NewGuid(), null)
        {

        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SequenceExecutorState"/> class from being created.
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        private SequenceExecutorState(Guid sequenceDefinitionId,
                                      Guid flowUid,
                                      Guid instanceId,
                                      SequenceExecutorExecThreadState? mainThread)
        {
            this.SequenceDefinitionId = sequenceDefinitionId;
            this.InstanceId = instanceId;
            this.MainThread = mainThread;
            this.FlowUid = flowUid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the sequence definition identifier.
        /// </summary>
        [Id(0)]
        public Guid SequenceDefinitionId { get; }

        /// <summary>
        /// Gets the flow identifier.
        /// </summary>
        /// <remarks>
        ///     Unique id shared throught all the vgrain to identify the process work
        /// </remarks>
        [Id(1)]
        public Guid FlowUid { get; }

        /// <summary>
        /// Gets the current instance identifier.
        /// </summary>
        [Id(2)]
        public Guid InstanceId { get; }

        /// <summary>
        /// Gets the main execution threads.
        /// </summary>
        [Id(3)]
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

        #endregion
    }
}
