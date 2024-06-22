// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class SequencePipelineStageFireSignalBuilder<TPreviousMessage> : ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder>, ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>, TPreviousMessage>
    {
        #region Fields

        private readonly SequencePipelineBuilder<TPreviousMessage> _root;
        private readonly AccessExpressionDefinition _signalInfo;
        private readonly Action<IDefinitionMetaDataWithDisplayNameBuilder>? _metaDataBuilder;
        private readonly Guid? _fixUid;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineStageFireSignalBuilder{TPreviousMessage}"/> class.
        /// </summary>
        public SequencePipelineStageFireSignalBuilder(AccessExpressionDefinition signalInfo, SequencePipelineBuilder<TPreviousMessage> root, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder, Guid? fixUid)
        {
            this._signalInfo = signalInfo;
            this._metaDataBuilder = metaDataBuilder;
            this._fixUid = fixUid;
            this._root = root;                
        }

        #endregion

        #region Methods

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>, TPreviousMessage>.Message<TMessage>(System.Linq.Expressions.Expression<Func<TPreviousMessage, TMessage>> messageAccess)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, TMessage?>(this._signalInfo, null, messageAccess, false, this._metaDataBuilder, this._fixUid);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>, TPreviousMessage>.Messages<TMessage, TResult>(System.Linq.Expressions.Expression<Func<TPreviousMessage, TResult>> messageAccess)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, IEnumerable<TMessage>>(this._signalInfo, null, messageAccess, true, this._metaDataBuilder, this._fixUid);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>>.Message<TMessage>(TMessage message)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, TMessage>(this._signalInfo, message, null, true, this._metaDataBuilder, this._fixUid);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>>.Message<TMessage>(IEnumerable<TMessage> messages)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, IEnumerable<TMessage>>(this._signalInfo, messages, null, true, this._metaDataBuilder, this._fixUid);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        ISequencePipelineBuilder ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder>.Message<TMessage>(TMessage message)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, TMessage>(this._signalInfo, message, null, true, this._metaDataBuilder, this._fixUid);
            return this._root.EnqueueStage(stage);
        }

        ISequencePipelineBuilder ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder>.Message<TMessage>(IEnumerable<TMessage> messages)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, IEnumerable<TMessage>>(this._signalInfo, messages, null, true, this._metaDataBuilder, this._fixUid);
            return this._root.EnqueueStage(stage);
        }

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>>.NoMessage()
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, NoneType>(this._signalInfo, null, null, false, this._metaDataBuilder, this._fixUid);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        public ISequencePipelineBuilder NoMessage()
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, NoneType>(this._signalInfo, null, null, false, this._metaDataBuilder, this._fixUid);
            return this._root.EnqueueStage(stage);
        }

        #endregion
    }
}
