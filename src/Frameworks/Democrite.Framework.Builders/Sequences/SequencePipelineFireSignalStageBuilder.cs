// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;

    using System.Linq.Expressions;

    /// <summary>
    /// Builder about <see cref="SequenceStageFireSignalDefinition"/> a stage able to fire a signal without or without message
    /// </summary>
    /// <seealso cref="ISequencePipelineStageDefinitionProvider" />
    internal sealed class SequencePipelineFireSignalStageBuilder<TInput, TMessage> : SequencePipelineStageBaseBuilder, ISequencePipelineStageDefinitionProvider
    {
        #region Fields

        private readonly AccessExpressionDefinition _signalInfo;
        private readonly LambdaExpression? _fetchMessage;
        private readonly TMessage? _directMessage;
        private readonly bool _multi;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineFireSignalStageBuilder"/> class.
        /// </summary>
        public SequencePipelineFireSignalStageBuilder(AccessExpressionDefinition signalInfo,
                                                      TMessage? directMessage,
                                                      LambdaExpression? fetchMessage,
                                                      bool multi, 
                                                      Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction, 
                                                      Guid? fixUid)
            : base(metaDataBuilderAction, fixUid)
        {
            this._signalInfo = signalInfo;
            this._multi = multi;
            this._directMessage = directMessage;
            this._fetchMessage = fetchMessage;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceStageDefinition ToDefinition()
        {
            var messageAccess = this._fetchMessage?.CreateAccess() ?? this._directMessage?.CreateAccess();

            var metaData = base.BuildDefinitionMetaData(out var displayName);

            return new SequenceStageFireSignalDefinition(this.FixUid,
                                                         displayName ?? "Fire Signal",
                                                         NoneType.IsEqualTo<TInput>() ? null : typeof(TInput).GetAbstractType(),
                                                         this._signalInfo,
                                                         this._multi,
                                                         messageAccess,
                                                         metaData);
        }

        #endregion
    }
}
