// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Create a sequence pipeline steps builder
    /// </summary>
    public interface ISequencePipelineBaseBuilder : IDefinitionBaseBuilder<SequenceDefinition>
    {
        /// <summary>
        /// Generic method to add a stage into the current pipeline.
        /// </summary>
        /// <returns>
        ///     Return next stage builder
        /// </returns>
        ISequencePipelineBuilder EnqueueStage(ISequencePipelineStageDefinitionProvider stage);

        /// <summary>
        /// Generic method to add a stage into the current pipeline.
        /// </summary>
        /// <returns>
        ///     Return next stage builder with input <typeparamref name="TOutput"/>
        /// </returns>
        ISequencePipelineBuilder<TOutput> EnqueueStage<TOutput>(ISequencePipelineStageDefinitionProvider stage);

        /// <summary>
        /// Create a sub <see cref="SequenceBuilder"/>
        /// </summary>
        /// <remarks>
        ///     used to link all sub sequence definition to an origine one
        /// </remarks>
        ISequenceBuilder CreateSubSequence(string? displayName);
    }

    /// <summary>
    /// Create a sequence pipeline steps builder
    /// </summary>
    public interface ISequencePipelineBuilder : ISequencePipelineBaseBuilder
    {
        /// <summary>
        /// Add a new <see cref="IVGrain"/> in the pipeline to transform action based on input to produce normally and output
        /// </summary>
        ISequencePipelineStageCallBuilder<TVGrain> Use<TVGrain>(Action<ISequencePipelineStageConfigurator>? cfg = null) where TVGrain : IVGrain;
    }

    /// <summary>
    /// Pipeline builder with a <typeparamref name="TPreviousMessage"/> message
    /// </summary>
    /// <typeparam name="TPreviousMessage">The type of the previous message.</typeparam>
    public interface ISequencePipelineBuilder<TPreviousMessage> : ISequencePipelineBaseBuilder
    {
        /// <summary>
        /// Add a new <see cref="IVGrain"/> in the pipeline to transform action based on input to produce normally and output
        /// </summary>
        ISequencePipelineStageCallBuilder<TPreviousMessage, TVGrain> Use<TVGrain>(Action<ISequencePipelineStageConfigurator<TPreviousMessage>>? cfg = null)
            where TVGrain : IVGrain;

        /// <summary>
        /// Add a new converter <see cref="ITransformerConvertVGrain{TInput}"/> in the pipeline to transform input into output
        /// </summary>
        ISequencePipelineStageCallBuilder<TPreviousMessage, TConverterVGrain> Convert<TConverterVGrain>(Action<ISequencePipelineStageConfigurator<TPreviousMessage>>? cfg = null)
            where TConverterVGrain : IVGrain;

    }
}
