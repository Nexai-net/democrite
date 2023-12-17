// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions
{
    using Democrite.Framework.Node.Abstractions.Models;

    using System;
    using System.Collections.Generic;
    
    internal interface ISequenceExecutorExecThreadState
    {
        /// <summary>
        /// Gets the flow identifier.
        /// </summary>
        Guid FlowUid { get; }

        /// <summary>
        /// Gets the definition id of the flow to execute
        /// </summary>
        Guid FlowDefinitionId { get; }

        /// <summary>
        /// Gets the current stage execute identifier.
        /// </summary>
        Guid CurrentStageExecId { get; }

        /// <summary>
        /// Gets the parent stage execute identifier.
        /// </summary>
        Guid? ParentStageExecId { get; }

        /// <summary>
        /// Gets cursor about the current stage id execution.
        /// </summary>
        Guid? Cursor { get; }

        /// <summary>
        /// Gets the input.
        /// </summary>
        object? ThreadInput { get; }

        /// <summary>
        /// Gets the last stage output. Null is thread doesn't have been started; Otherwise <see cref="NoneType.Instance"/>
        /// </summary>
        object? Output { get; }

        /// <summary>
        /// Gets a value indicating whether job is done.
        /// </summary>
        bool JobDone { get; }

        /// <summary>
        /// Gets the inner threads.
        /// </summary>
        IReadOnlyCollection<SequenceExecutorExecThreadState> InnerThreads { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        string? ErrorMessage { get; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SequenceExecutorExecThreadState"/> is started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if started; otherwise, <c>false</c>.
        /// </value>
        bool Started { get; }

        /// <summary>
        /// Gets a value indicating whether this thread is running
        /// </summary>
        bool Running { get; }
    }
}
