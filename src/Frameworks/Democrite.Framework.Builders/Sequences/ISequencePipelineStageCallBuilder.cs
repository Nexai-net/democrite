// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions;

    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Build a stage using a method call
    /// </summary>
    public interface ISequencePipelineStageCallBuilder<TSequenceVGrain> : ISequencePipelineStageBaseBuilder
        where TSequenceVGrain : IVGrain
    {
        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TSequenceVGrain> Call<TOutputMessage>(Expression<Func<TSequenceVGrain, IExecutionContext, Task<TOutputMessage>>> expr);

        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        ISequencePipelineStageFinalizerBuilder<TSequenceVGrain> Call(Expression<Func<TSequenceVGrain, IExecutionContext, Task>> expr);

        /// <summary>
        /// Add execution context
        /// </summary>
        ISequencePipelineStageContextedCallBuilder<TSequenceVGrain, TCtx> Context<TCtx>(TCtx context);
    }

    /// <summary>
    /// Build a stage using a method call
    /// </summary>
    public interface ISequencePipelineStageCallBuilder<TInputMessage, TSequenceVGrain> : ISequencePipelineStageBaseBuilder
        where TSequenceVGrain : IVGrain
    {
        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TSequenceVGrain> Call<TOutputMessage>(Expression<Func<TSequenceVGrain, TInputMessage, IExecutionContext, Task<TOutputMessage>>> expr);

        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        ISequencePipelineStageFinalizerBuilder<TSequenceVGrain> Call(Expression<Func<TSequenceVGrain, TInputMessage, IExecutionContext, Task>> expr);

        /// <summary>
        /// Add execution context
        /// </summary>
        ISequencePipelineStageContextedCallBuilder<TSequenceVGrain, TCtx, TInputMessage> Configure<TCtx>(TCtx context);

    }
}
