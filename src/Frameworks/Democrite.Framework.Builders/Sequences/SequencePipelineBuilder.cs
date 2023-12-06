// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Toolbox;

    /// <summary>
    /// Pipeline builder
    /// </summary>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    /// <seealso cref="ISequencePipelineBuilder" />
    /// <seealso cref="ISequencePipelineBuilder{TOutput}" />
    internal sealed class SequencePipelineBuilder<TInput> : ISequencePipelineBuilder, ISequencePipelineBuilder<TInput>//, ISequencePipelineInternalBuilder
    {
        #region Fields

        private readonly SequenceBuilder _sequenceBuilder;

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineBuilder{TOutput}"/> class.
        /// </summary>
        internal SequencePipelineBuilder(SequenceBuilder sequenceBuilder)
        {
            ArgumentNullException.ThrowIfNull(sequenceBuilder);

            this._sequenceBuilder = sequenceBuilder;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ISequencePipelineBuilder EnqueueStage(ISequencePipelineStageDefinitionProvider stage)
        {
            this._sequenceBuilder.EnqueueStage(stage);
            return new SequencePipelineBuilder<NoneType>(this._sequenceBuilder);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TOutput> EnqueueStage<TOutput>(ISequencePipelineStageDefinitionProvider stage)
        {
            this._sequenceBuilder.EnqueueStage(stage);
            return new SequencePipelineBuilder<TOutput>(this._sequenceBuilder);
        }

        /// <inheritdoc />
        ISequencePipelineStageCallBuilder<TVGrain> ISequencePipelineBuilder.Use<TVGrain>(Action<ISequencePipelineStageConfigurator>? cfg)
        {
            var stageBuilder = new SequencePipelineVGrainStageCallBuilder<TVGrain, NoneType, NoneType>(this,
                                                                                                     cfg == null
                                                                                                          ? null
                                                                                                          : (i) => cfg.Invoke((ISequencePipelineStageConfigurator)(object)i));
            return stageBuilder;
        }

        /// <inheritdoc />
        ISequencePipelineStageCallBuilder<TInput, TVGrain> ISequencePipelineBuilder<TInput>.Use<TVGrain>(Action<ISequencePipelineStageConfigurator<TInput>>? cfg)
        {
            var stageBuilder = new SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, NoneType>(this, cfg);
            return stageBuilder;
        }

        /// <inheritdoc />
        ISequencePipelineStageCallBuilder<TInput, TVGrain> ISequencePipelineBuilder<TInput>.Convert<TVGrain>(Action<ISequencePipelineStageConfigurator<TInput>>? cfg)
        {
            var stageBuilder = new SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, NoneType>(this, cfg);
            return stageBuilder;
        }

        /// <inheritdoc/>
        public ISequenceBuilder CreateSubSequence(string? displayName)
        {
            return new SequenceBuilder(this._sequenceBuilder, displayName);
        }

        /// <inheritdoc />
        SequenceDefinition IDefinitionBaseBuilder<SequenceDefinition>.Build()
        {
            return this._sequenceBuilder.Build();
        }

        #endregion
    }
}
