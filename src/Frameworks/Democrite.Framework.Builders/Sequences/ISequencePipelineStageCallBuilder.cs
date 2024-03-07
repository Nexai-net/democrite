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
    /// Get a stage using a method call
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
        ISequencePipelineStageConfiguredCallBuilder<TSequenceVGrain, TCtx> Configure<TCtx>(TCtx context);
    }

    /// <summary>
    /// Get a stage using a method call
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
        /// Add execution context configuration
        /// </summary>
        ISequencePipelineStageConfiguredCallBuilder<TSequenceVGrain, TCtx, TInputMessage> Configure<TCtx>(TCtx context);

        /// <summary>
        /// Add execution context configuration from input data
        /// </summary>
        ISequencePipelineStageConfiguredCallBuilder<TSequenceVGrain, TCtx, TInputMessage> ConfigureFromInput<TCtx>(Expression<Func<TInputMessage, TCtx>> executionConfiguration);

        /// <summary>
        /// Use input as execution context configuration
        /// </summary>
        /// <remarks>
        ///     Use full when configuration must depend of the input 
        /// </remarks>
        ISequencePipelineStageConfiguredCallBuilder<TSequenceVGrain, TInputMessage, TInputMessage> UseInputAsConfiguration();

    }
}
