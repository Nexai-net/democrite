// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Expressions;

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
        ISequencePipelineStageCallBuilder<TInput, TVGrain> ISequencePipelineBuilder<TInput>.Use<TVGrain>(Action<ISequencePipelineStageConfigurator>? cfg)
        {
            var stageBuilder = new SequencePipelineVGrainStageCallBuilder<TVGrain, TInput, NoneType>(this, cfg);
            return stageBuilder;
        }

        /// <inheritdoc />
        ISequencePipelineStageCallBuilder<TInput, TVGrain> ISequencePipelineBuilder<TInput>.Convert<TVGrain>(Action<ISequencePipelineStageConfigurator>? cfg)
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
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder> ISequencePipelineSignalBuilder<ISequencePipelineBuilder>.FireSignal(string signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder> ISequencePipelineSignalBuilder<ISequencePipelineBuilder>.FireSignal(Guid signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder> ISequencePipelineSignalBuilder<ISequencePipelineBuilder>.FireSignal(SignalId signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TInput>, TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>, TInput>.FireSignal(string signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TInput>, TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>, TInput>.FireSignal(Guid signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TInput>, TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>, TInput>.FireSignal(SignalId signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TInput>, TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>, TInput>.FireSignal(Expression<Func<TInput, string>> signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TInput>, TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>, TInput>.FireSignal(Expression<Func<TInput, Guid>> signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        ISequencePipelineStageFireSignalBuilder<ISequencePipelineBuilder<TInput>, TInput> ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TInput>, TInput>.FireSignal(Expression<Func<TInput, SignalId>> signalName)
        {
            var builder = new SequencePipelineStageFireSignalBuilder<TInput>(signalName.CreateAccess(), this);
            return builder;
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TSelected> Select<TSelected>(Expression<Func<TSelected>> select, Action<ISequencePipelineStageConfigurator>? cfg = null)
        {
            return SelectImpl<NoneType, TSelected>(select.CreateAccess(), cfg);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TSelected> Select<TSelected>(TSelected item, Action<ISequencePipelineStageConfigurator>? cfg = null)
        {
            return SelectImpl<NoneType, TSelected>(item.CreateAccess(), cfg);
        }

        /// <inheritdoc />
        public ISequencePipelineBuilder<TSelected> Select<TSelected>(Expression<Func<TInput, TSelected>> select, Action<ISequencePipelineStageConfigurator>? cfg = null)
        {
            return SelectImpl<TInput, TSelected>(select.CreateAccess(), cfg);
        }

        #region Tools

        /// <inheritdoc />
        private ISequencePipelineBuilder<TSelected> SelectImpl<TForceInput, TSelected>(AccessExpressionDefinition access, Action<ISequencePipelineStageConfigurator>? cfg)
        {
            var selectStage = new SequencePipelineSelectStageBuilder<TForceInput, TSelected>(access, cfg);
            return EnqueueStage<TSelected>(selectStage);
        }

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, NoneTypeStruct?> FireSignalImpl(string signalName)
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, NoneTypeStruct?>(signalName, null, null, null, null, false, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, NoneTypeStruct?> FireSignalImpl(Guid signalId)
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, NoneTypeStruct?>(null, signalId, null, null, null, false, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(string signalName, TMessage message)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(signalName, null, null, message, null, false, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>> FireSignalImpl<TMessage>(string signalName, IEnumerable<TMessage> message)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>>(signalName, null, null, message, null, true, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(Guid signalId, TMessage message)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(null, signalId, null, message, null, false, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>> FireSignalImpl<TMessage>(Guid signalId, IEnumerable<TMessage> messages)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>>(null, signalId, null, messages, null, true, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(LambdaExpression signalAccess, Expression<Func<TInput, TMessage>> messageBuilder)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(null, null, signalAccess, default, messageBuilder, false, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(string signalName, Expression<Func<TInput, TMessage>> messageBuilder)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(signalName, null, null, default, messageBuilder, false, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>> FireSignalImpl<TMessage>(string signalName, Expression<Func<TInput, IEnumerable<TMessage>>> messageBuilder)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>>(signalName, null, null, null, messageBuilder, true, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, TMessage> FireSignalImpl<TMessage>(Guid signalId, Expression<Func<TInput, TMessage>> messageBuilder)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, TMessage>(null, signalId, null, default, messageBuilder, false, null);
        //}

        ///// <inheritdoc />
        //private SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>> FireSignalImpl<TMessage>(Guid signalId, Expression<Func<TInput, IEnumerable<TMessage>>> messageBuilder)
        //    where TMessage : struct
        //{
        //    return new SequencePipelineFireSignalStageBuilder<TInput, IEnumerable<TMessage>>(null, signalId, null, null, messageBuilder, true, null);
        //}

        /// <inheritdoc />
        public ISequencePipelineNestedSequenceCallBuilder CallSequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null, Action<ISequencePipelineStageConfigurator>? cfg = null)
        {
            return new SequencePipelineNestedSequenceCallBuilder<NoneType>(this, sequenceId, cfgBuilder, cfg);
        }

        /// <inheritdoc />
        ISequencePipelineNestedSequenceCallBuilder<TInput> ISequencePipelineBuilder<TInput>.CallSequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder, Action<ISequencePipelineStageConfigurator>? cfg)
        {
            return new SequencePipelineNestedSequenceCallBuilder<TInput>(this, sequenceId, cfgBuilder, cfg);
        }

        #endregion

        #endregion
    }
}
