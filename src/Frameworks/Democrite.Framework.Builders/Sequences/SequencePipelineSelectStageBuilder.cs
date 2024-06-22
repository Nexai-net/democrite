// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;

    using System;

    /// <summary>
    /// Allow selection, input transformation, injection, during a sequences
    /// </summary>
    internal sealed class SequencePipelineSelectStageBuilder<TInput, TSelect> : SequencePipelineStageBaseBuilder, ISequencePipelineStageDefinitionProvider
    {
        #region Fields
        
        private readonly AccessExpressionDefinition _expressionDefinition;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineSelectStageBuilder{TInput, TSelect}"/> class.
        /// </summary>
        public SequencePipelineSelectStageBuilder(AccessExpressionDefinition expressionDefinition,
                                                  Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction,
                                                  Guid? fixUid)
            : base(metaDataBuilderAction, fixUid)
        {
            this._expressionDefinition = expressionDefinition;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceStageDefinition ToDefinition()
        {
            var metaData = BuildDefinitionMetaData(out var displayName);
            return new SequenceStageSelectDefinition(this.FixUid,
                                                     displayName ?? "Select",
                                                     NoneType.IsEqualTo<TInput>() ? null : typeof(TInput).GetAbstractType(),
                                                     typeof(TSelect).GetAbstractType(),
                                                     this._expressionDefinition,
                                                     metaData);
        }

        #endregion
    }
}
