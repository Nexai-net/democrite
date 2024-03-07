// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Toolbox;

    using System.Linq.Expressions;

    /// <summary>
    /// Builder about <see cref="SequenceStageFireSignalDefinition"/> a stage able to fire a signal without or without message
    /// </summary>
    /// <seealso cref="ISequencePipelineStageDefinitionProvider" />
    internal sealed class SequencePipelineFireSignalStageBuilder<TInput, TMessage> : SequencePipelineStageBaseBuilder<TInput>, ISequencePipelineStageDefinitionProvider
        //where TMessage : struct
    {
        #region Fields

        private readonly LambdaExpression? _fetchMessage;
        private readonly TMessage? _directMessage;
        private readonly string? _signalName;
        private readonly Guid? _signalUid;
        private readonly bool _multi;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineFireSignalStageBuilder"/> class.
        /// </summary>
        public SequencePipelineFireSignalStageBuilder(string? signalName,
                                                      Guid? signalUid,
                                                      TMessage? directMessage,
                                                      LambdaExpression? fetchMessage,
                                                      bool multi,
                                                      Action<ISequencePipelineStageConfigurator<TInput>>? configAction)
            : base(configAction)
        {
            this._signalName = signalName;
            this._signalUid = signalUid;
            this._multi = multi;
            this._directMessage = directMessage;
            this._fetchMessage = fetchMessage;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceStageBaseDefinition ToDefinition()
        {
            var messageAccess = this._fetchMessage?.CreateAccess() ?? this._directMessage?.CreateAccess();

            return new SequenceStageFireSignalDefinition(NoneType.IsEqualTo<TInput>() ? null : typeof(TInput).GetAbstractType(),
                                                         this._signalName,
                                                         this._signalUid,
                                                         this._multi,
                                                         messageAccess);
        }

        #endregion
    }
}
