// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;

    using Elvex.Toolbox.Abstractions.Expressions;
    using Elvex.Toolbox.Abstractions.Models;

    using System;

    /// <summary>
    /// Stage about loop throught <typeparamref name="TInput"/> to apply a processing sequence on each item
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <seealso cref="ISequencePipelineStageDefinitionProvider" />
    internal sealed class SequencePipelineForeachStageBuilder<TInput> : SequencePipelineStageBaseBuilder, ISequencePipelineStageDefinitionProvider
    {
        #region Fields

        private readonly AccessExpressionDefinition? _memberAccess;
        private readonly ISequenceBuilder _sequenceBuilder;
        private readonly AbstractMethod? _setMethod;
        private readonly bool _relayInput;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineForeachStageBuilder"/> class.
        /// </summary>
        public SequencePipelineForeachStageBuilder(ISequenceBuilder sequenceBuilder,
                                                   AccessExpressionDefinition? memberAccess = null,
                                                   AbstractMethod? setMethod = null,
                                                   bool relayInput = false,
                                                   Action<ISequencePipelineStageConfigurator>? configAction = null)
            : base(configAction)
        {
            this._sequenceBuilder = sequenceBuilder;
            this._memberAccess = memberAccess;
            this._relayInput = relayInput;
            this._setMethod = setMethod;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceStageBaseDefinition ToDefinition()
        {
            var innerDefinition = this._sequenceBuilder.Build();
            var cfg = BuildConfigDefinition();

            var input = typeof(TInput).GetAbstractType();

            return new SequenceStageForeachDefinition(input,
                                                      innerDefinition,
                                                      output: this._relayInput ? input : innerDefinition.Output,
                                                      innerDefinition.Output,
                                                      memberAccess: this._memberAccess,
                                                      setMethod: this._setMethod,
                                                      options: cfg);
        }

        #endregion
    }
}
