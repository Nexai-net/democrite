// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;

    using System;

    /// <summary>
    /// Stage about loop throught <typeparamref name="TInput"/> to apply a processing sequence on each item
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <seealso cref="ISequencePipelineStageDefinitionProvider" />
    internal sealed class SequencePipelineForeachStageBuilder<TInput> : SequencePipelineStageBaseBuilder<TInput>, ISequencePipelineStageDefinitionProvider
    {
        #region Fields

        private readonly ISequenceBuilder _sequenceBuilder;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineForeachStageBuilder"/> class.
        /// </summary>
        public SequencePipelineForeachStageBuilder(ISequenceBuilder sequenceBuilder, Action<ISequencePipelineStageConfigurator<TInput>>? configAction)
            : base(configAction)
        {
            this._sequenceBuilder = sequenceBuilder;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceStageBaseDefinition ToDefinition()
        {
            var innerDefinition = this._sequenceBuilder.Build();
            var cfg = BuildConfigDefinition();

            return new SequenceStageForeachDefinition(typeof(TInput).GetAbstractType(),
                                                      innerDefinition,
                                                      innerDefinition.Output,
                                                      cfg);
        }

        #endregion
    }
}
