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
    /// Get contexted call
    /// </summary>
    /// <typeparam name="TSequenceVGrain">The type of the sequence virtual grain.</typeparam>
    /// <typeparam name="TContextInfo">The type of the context information.</typeparam>
    public interface ISequencePipelineStageConfiguredCallBuilder<TSequenceVGrain, TContextInfo>
        where TSequenceVGrain : IVGrain
    {
        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TSequenceVGrain> Call<TOutputMessage>(Expression<Func<TSequenceVGrain, IExecutionContext<TContextInfo>, Task<TOutputMessage>>> expr);

        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        ISequencePipelineStageFinalizerBuilder<TSequenceVGrain> Call(Expression<Func<TSequenceVGrain, IExecutionContext<TContextInfo>, Task>> expr);
    }

    /// <summary>
    /// Get contexted call
    /// </summary>
    /// <typeparam name="TSequenceVGrain">The type of the sequence virtual grain.</typeparam>
    /// <typeparam name="TContextInfo">The type of the context information.</typeparam>
    /// <typeparam name="TInputMessage">TInput type message.</typeparam>
    public interface ISequencePipelineStageConfiguredCallBuilder<TSequenceVGrain, TContextInfo, TInputMessage>
        where TSequenceVGrain : IVGrain
    {
        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TSequenceVGrain> Call<TOutputMessage>(Expression<Func<TSequenceVGrain, TInputMessage, IExecutionContext<TContextInfo>, Task<TOutputMessage>>> expr);

        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        ISequencePipelineStageFinalizerBuilder<TSequenceVGrain> Call(Expression<Func<TSequenceVGrain, TInputMessage, IExecutionContext<TContextInfo>, Task>> expr);
    }
}
