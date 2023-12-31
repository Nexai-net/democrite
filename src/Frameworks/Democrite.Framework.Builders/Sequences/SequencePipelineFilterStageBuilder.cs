﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Toolbox.Abstractions.Conditions;

    using System;

    /// <summary>
    /// Stage about loop throught <typeparamref name="TInput"/> to apply a filter
    /// </summary>
    /// <seealso cref="ISequencePipelineInternalStage" />
    internal sealed class SequencePipelineFilterStageBuilder<TInput> : SequencePipelineStageBaseBuilder<TInput>, ISequencePipelineStageDefinitionProvider
    {
        #region Fields

        private readonly ConditionExpressionDefinition _condition;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineForeachBuilder"/> class.
        /// </summary>
        public SequencePipelineFilterStageBuilder(ConditionExpressionDefinition condition, Action<ISequencePipelineStageConfigurator<TInput>>? configAction)
            : base(configAction)
        {
            this._condition = condition;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ISequenceStageDefinition ToDefinition()
        {
            var config = BuildConfigDefinition();
            return new SequenceStageFilterDefinition(typeof(TInput), this._condition, config);
        }

        #endregion
    }
}
