// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions.Models;

    using Elvex.Toolbox.Abstractions.Disposables;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Handler used to manipulate thread information. Use by <see cref="ISequenceExecutorThreadStageHandler"/> to do advanced thread execution
    /// </summary>
    internal interface ISequenceExecutorThreadHandler
    {
        /// <summary>
        /// Gets a value indicating whether if all inner threads job are done.
        /// </summary>
        bool AllInnerThreadsJobDone { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has inner threads.
        /// </summary>
        bool HasInnerThreads { get; }

        /// <summary>
        /// Gets the state of the current thread saved
        /// </summary>
        ISequenceExecutorExecThreadState GetCurrentDoneThreadState();

        /// <summary>
        /// Gets the state of the current thread in processing
        /// </summary>
        ISequenceExecutorExecThreadState GetCurrentInProcessThreadState();

        /// <summary>
        /// Creates an inner <see cref="ISequenceExecutorExecThread"/>.
        /// </summary>
        ISequenceExecutorExecThread CreateInnerThread(SequenceExecutorExecThreadState sequenceExecutorExecThreadState,
                                                      SequenceDefinition innerFlow, 
                                                      IExecutionContext sourceExecutionContext);

        /// <summary>
        /// Registers the post process callback.
        /// </summary>
        void RegisterPostProcess(Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>> foreachSequenceStagePostProcess);

        /// <summary>
        /// Sets the inner threads.
        /// </summary>
        void SetInnerThreads(IReadOnlyCollection<ISequenceExecutorExecThread> innerThreads);

        /// <summary>
        /// Pulls the inner threads.
        /// </summary>
        /// <param name="emptyOnPull">if set to <c>true</c> [empty on pull].</param>
        IReadOnlyCollection<ISequenceExecutorExecThread> PullInnerThreads(bool emptyOnPull);

        /// <summary>
        /// Gets the sequence execution customization.
        /// </summary>
        ExecutionCustomizationDescriptions? GetSequenceExecutionCustomization();
    }
}
