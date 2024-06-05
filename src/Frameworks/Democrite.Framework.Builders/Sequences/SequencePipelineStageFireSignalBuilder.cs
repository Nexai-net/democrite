﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class SequencePipelineStageFireSignalBuilder<TPreviousMessage> : ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder>, ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>, TPreviousMessage>
    {
        #region Fields

        private readonly SequencePipelineBuilder<TPreviousMessage> _root;
        private readonly AccessExpressionDefinition _signalInfo;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineStageFireSignalBuilder{TPreviousMessage}"/> class.
        /// </summary>
        public SequencePipelineStageFireSignalBuilder(AccessExpressionDefinition signalInfo, SequencePipelineBuilder<TPreviousMessage> root)
        {
            this._signalInfo = signalInfo;
            this._root = root;                
        }

        #endregion

        #region Methods

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>, TPreviousMessage>.Message<TMessage>(System.Linq.Expressions.Expression<Func<TPreviousMessage, TMessage>> messageAccess)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, TMessage?>(this._signalInfo, null, messageAccess, false, null);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>, TPreviousMessage>.Messages<TMessage, TResult>(System.Linq.Expressions.Expression<Func<TPreviousMessage, TResult>> messageAccess)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, IEnumerable<TMessage>>(this._signalInfo, null, messageAccess, true, null);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>>.Message<TMessage>(TMessage message)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, TMessage>(this._signalInfo, message, null, true, null);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>>.Message<TMessage>(IEnumerable<TMessage> messages)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, IEnumerable<TMessage>>(this._signalInfo, messages, null, true, null);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        ISequencePipelineBuilder ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder>.Message<TMessage>(TMessage message)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, TMessage>(this._signalInfo, message, null, true, null);
            return this._root.EnqueueStage(stage);
        }

        ISequencePipelineBuilder ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder>.Message<TMessage>(IEnumerable<TMessage> messages)
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, IEnumerable<TMessage>>(this._signalInfo, messages, null, true, null);
            return this._root.EnqueueStage(stage);
        }

        ISequencePipelineBuilder<TPreviousMessage> ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>>.NoMessage()
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, NoneType>(this._signalInfo, null, null, false, null);
            return this._root.EnqueueStage<TPreviousMessage>(stage);
        }

        public ISequencePipelineBuilder NoMessage()
        {
            var stage = new SequencePipelineFireSignalStageBuilder<TPreviousMessage, NoneType>(this._signalInfo, null, null, false, null);
            return this._root.EnqueueStage(stage);
        }

        #endregion
    }
}
