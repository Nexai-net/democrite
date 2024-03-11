// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Elvex.Toolbox;

    using System.Linq.Expressions;

    /// <summary>
    /// Pipeline builder
    /// </summary>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    /// <seealso cref="ISequencePipelineBuilder" />
    /// <seealso cref="ISequencePipelineBuilder{TOutput}" />
    internal sealed class SequencePipelineBuilder<TInput> : ISequencePipelineBuilder, ISequencePipelineBuilder<TInput>
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
        public ISequenceBuilder CreateSubSequence(string displayName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(displayName);
            return new SequenceBuilder(this._sequenceBuilder, displayName);
        }

        /// <inheritdoc />
        SequenceDefinition IDefinitionBaseBuilder<SequenceDefinition>.Build()
        {
            return this._sequenceBuilder.Build();
        }
        
        /// <inheritdoc />
        public ISequencePipelineBuilder PushToContext<TInfo>(Expression<Func<TInfo>> data, bool @override)
        {
            var pushToContextStage = new SequencePipelinePushToContextStageBuilder<NoneType>(data, @override, null);
            return EnqueueStage(pushToContextStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineBuilder<TInput>.PushToContext<TInfo>(Expression<Func<TInput, TInfo>> data, bool @override)
        {
            var pushToContextStage = new SequencePipelinePushToContextStageBuilder<TInput>(data, @override, null);
            return EnqueueStage<TInput>(pushToContextStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder ClearContextMetadata<TInfo>()
        {
            throw new NotImplementedException("Yet");
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder ClearAllContextMetadata()
        {
            throw new NotImplementedException("Yet");
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineDataContextlBuilder<ISequencePipelineBuilder<TInput>>.ClearContextMetadata<TInfo>()
        {
            throw new NotImplementedException("Yet");
        }
        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineDataContextlBuilder<ISequencePipelineBuilder<TInput>>.ClearAllContextMetadata()
        {
            throw new NotImplementedException("Yet");
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignal(string signalName)
        {
            var fireStage = FireSignalImpl(signalName);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignal(Guid signalId)
        {
            var fireStage = FireSignalImpl(signalId);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignal(SignalId signalId)
        {
            var fireStage = FireSignalImpl(signalId.Uid);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignal<TMessage>(string signalName, TMessage message)
            where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalName, message);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignal<TMessage>(Guid signalId, TMessage message)
            where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalId, message);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignal<TMessage>(SignalId signalId, TMessage message)
            where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalId.Uid , message);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignals<TMessage>(string signalName, IEnumerable<TMessage> messages) where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalName, messages);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignals<TMessage>(Guid signalName, IEnumerable<TMessage> messages) where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalName, messages);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder FireSignals<TMessage>(SignalId signalName, IEnumerable<TMessage> messages) where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalName.Uid, messages);
            return EnqueueStage(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineBuilder<TInput>.FireSignal<TMessage>(string signalName, Expression<Func<TInput, TMessage>> messageBuilder)
        {
            var fireStage = FireSignalImpl(signalName, messageBuilder);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineBuilder<TInput>.FireSignal<TMessage>(Guid signalId, Expression<Func<TInput, TMessage>> messageBuilder)
        {
            var fireStage = FireSignalImpl(signalId, messageBuilder);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineBuilder<TInput>.FireSignal<TMessage>(SignalId signalId, Expression<Func<TInput, TMessage>> messageBuilder)
        {
            var fireStage = FireSignalImpl(signalId.Uid, messageBuilder);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignal(string signalName)
        {
            var fireStage = FireSignalImpl(signalName);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignal(Guid signalId)
        {
            var fireStage = FireSignalImpl(signalId);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignal(SignalId signalId)
        {
            var fireStage = FireSignalImpl(signalId.Uid);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignal<TMessage>(string signalName, TMessage message)
        {
            var fireStage = FireSignalImpl(signalName, message);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignal<TMessage>(Guid signalId, TMessage message)
        {
            var fireStage = FireSignalImpl(signalId, message);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignal<TMessage>(SignalId signalId, TMessage message)
        {
            var fireStage = FireSignalImpl(signalId.Uid, message);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TInput> FireSignals<TMessage>(string signalName, Expression<Func<TInput, IEnumerable<TMessage>>> messages) where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalName, messages);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TInput> FireSignals<TMessage>(Guid signalId, Expression<Func<TInput, IEnumerable<TMessage>>> messages) where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalId, messages);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TInput> FireSignals<TMessage>(SignalId signalId, Expression<Func<TInput, IEnumerable<TMessage>>> messages) where TMessage : struct
        {
            var fireStage = FireSignalImpl(signalId.Uid, messages);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignals<TMessage>(string signalName, IEnumerable<TMessage> messages)
        {
            var fireStage = FireSignalImpl(signalName, messages);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignals<TMessage>(Guid signalName, IEnumerable<TMessage> messages)
        {
            var fireStage = FireSignalImpl(signalName, messages);
            return EnqueueStage<TInput>(fireStage);
        }

        /// <inheritdoc />
        ISequencePipelineBuilder<TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>>.FireSignals<TMessage>(SignalId signalName, IEnumerable<TMessage> messages)
        {
            var fireStage = FireSignalImpl(signalName.Uid, messages);
            return EnqueueStage<TInput>(fireStage);
        }

        #region Tools

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, NoneTypeStruct?> FireSignalImpl(string signalName)
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, NoneTypeStruct?>(signalName, null, null, null, false, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, NoneTypeStruct?> FireSignalImpl(Guid signalId)
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, NoneTypeStruct?>(null, signalId, null, null, false, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(string signalName, TMessage message)
            where TMessage : struct
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(signalName, null, message, null, false, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>> FireSignalImpl<TMessage>(string signalName, IEnumerable<TMessage> message)
            where TMessage : struct
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>>(signalName, null, message, null, true, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(Guid signalId, TMessage message)
            where TMessage : struct
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(null, signalId, message, null, false, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>> FireSignalImpl<TMessage>(Guid signalId, IEnumerable<TMessage> messages)
            where TMessage : struct
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>>(null, signalId, messages, null, true, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(string signalName, Expression<Func<TInput, TMessage>> messageBuilder)
            where TMessage : struct
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(signalName, null, default, messageBuilder, false, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>> FireSignalImpl<TMessage>(string signalName, Expression<Func<TInput, IEnumerable<TMessage>>> messageBuilder)
            where TMessage : struct
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>>(signalName, null, null, messageBuilder, true, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(Guid signalId, Expression<Func<TInput, TMessage>> messageBuilder)
            where TMessage : struct
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(null, signalId, default, messageBuilder, false, null);
        }

        /// <inheritdoc />
        private SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>> FireSignalImpl<TMessage>(Guid signalId, Expression<Func<TInput, IEnumerable<TMessage>>> messageBuilder)
            where TMessage : struct
        {
            return new SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>>(null, signalId, null, messageBuilder, true, null);
        }

        #endregion

        #endregion
    }
}
