﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System.Linq.Expressions;
    using System.Runtime;

    /// <summary>
    /// Get a sequence pipeline steps builder
    /// </summary>
    public interface ISequencePipelineBaseBuilder : IDefinitionBaseBuilder<SequenceDefinition>
    {
        /// <summary>
        /// Generic method to add a stage into the current pipeline.
        /// </summary>
        /// <returns>
        ///     Return next stage builder
        /// </returns>
        ISequencePipelineBuilder EnqueueStage(ISequencePipelineStageDefinitionProvider stage);

        /// <summary>
        /// Generic method to add a stage into the current pipeline.
        /// </summary>
        /// <returns>
        ///     Return next stage builder with input <typeparamref name="TOutput"/>
        /// </returns>
        ISequencePipelineBuilder<TOutput> EnqueueStage<TOutput>(ISequencePipelineStageDefinitionProvider stage);

        /// <summary>
        /// Get a sub <see cref="SequenceBuilder"/>
        /// </summary>
        /// <remarks>
        ///     used to link all sub sequence definition to an origine one
        /// </remarks>
        ISequenceBuilder CreateSubSequence(string displayName);
    }

    /// <summary>
    /// Builder dedicated to signal setup in the sequence
    /// </summary>
    public interface ISequencePipelineSignalBuilder<TBuilderOutput>
    {
        /// <summary>
        /// Fire a designated signal
        /// </summary>
        TBuilderOutput FireSignal(string signalName);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        TBuilderOutput FireSignal(Guid signalName);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        TBuilderOutput FireSignal(SignalId signalName);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        TBuilderOutput FireSignal<TMessage>(string signalName, TMessage message)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal one by message
        /// </summary>
        TBuilderOutput FireSignals<TMessage>(string signalName, IEnumerable<TMessage> messages)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        TBuilderOutput FireSignal<TMessage>(Guid signalName, TMessage message)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal one by message
        /// </summary>
        TBuilderOutput FireSignals<TMessage>(Guid signalName, IEnumerable<TMessage> messages)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        TBuilderOutput FireSignal<TMessage>(SignalId signalName, TMessage message)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal one by message
        /// </summary>
        TBuilderOutput FireSignals<TMessage>(SignalId signalName, IEnumerable<TMessage> messages)
            where TMessage : struct;

    }

    /// <summary>
    /// Builder dedicated to push to data context setup in the sequence
    /// </summary>
    public interface ISequencePipelineDataContextlBuilder<TBuilderOutput>
    {
        /// <summary>
        /// Add a new stage that inject in content to <see cref="IExecutionContext"/>
        /// </summary>
        ISequencePipelineBuilder PushToContext<TInfo>(Expression<Func<TInfo>> data, bool @override = true);

        /// <summary>
        /// Add a new stage that clear context content to <see cref="IExecutionContext"/>
        /// </summary>
        TBuilderOutput ClearContextMetadata<TInfo>();

        /// <summary>
        /// Add a new stage that clear context content to <see cref="IExecutionContext"/>
        /// </summary>
        TBuilderOutput ClearAllContextMetadata();
    }

    /// <summary>
    /// Get a sequence pipeline steps builder
    /// </summary>
    public interface ISequencePipelineBuilder : ISequencePipelineBaseBuilder,
                                                ISequencePipelineSignalBuilder<ISequencePipelineBuilder>,
                                                ISequencePipelineDataContextlBuilder<ISequencePipelineBuilder>
    {
        /// <summary>
        /// Add a new <see cref="IVGrain"/> in the pipeline to transform action based on input to produce normally and output
        /// </summary>
        ISequencePipelineStageCallBuilder<TVGrain> Use<TVGrain>(Action<ISequencePipelineStageConfigurator>? cfg = null) where TVGrain : IVGrain;

        /// <summary>
        /// Selects the specified data as input
        /// </summary>
        ISequencePipelineBuilder<TSelected> Select<TSelected>(Expression<Func<TSelected>> select, Action<ISequencePipelineStageConfigurator>? cfg = null);

        /// <summary>
        /// Selects the specified data as input
        /// </summary>
        ISequencePipelineBuilder<TSelected> Select<TSelected>(TSelected select, Action<ISequencePipelineStageConfigurator>? cfg = null);

        /// <summary>
        /// Add a new stage used to call an existing sequence
        /// </summary>
        ISequencePipelineNestedSequenceCallBuilder CallSequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null, Action<ISequencePipelineStageConfigurator>? cfg = null);
    }

    /// <summary>
    /// Pipeline builder with a <typeparamref name="TPreviousMessage"/> message
    /// </summary>
    /// <typeparam name="TPreviousMessage">The type of the previous message.</typeparam>
    public interface ISequencePipelineBuilder<TPreviousMessage> : ISequencePipelineBaseBuilder,
                                                                  ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>>,
                                                                  ISequencePipelineDataContextlBuilder<ISequencePipelineBuilder<TPreviousMessage>>

    {
        /// <summary>
        /// Selects the specified data as input
        /// </summary>
        ISequencePipelineBuilder<TSelected> Select<TSelected>(Expression<Func<TPreviousMessage, TSelected>> select, Action<ISequencePipelineStageConfigurator>? cfg = null);

        /// <summary>
        /// Add a new <see cref="IVGrain"/> in the pipeline to transform action based on input to produce normally and output
        /// </summary>
        ISequencePipelineStageCallBuilder<TPreviousMessage, TVGrain> Use<TVGrain>(Action<ISequencePipelineStageConfigurator>? cfg = null)
            where TVGrain : IVGrain;

        /// <summary>
        /// Add a new stage that inject in content to <see cref="IExecutionContext"/>
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> PushToContext<TInfo>(Expression<Func<TPreviousMessage, TInfo>> data, bool @override = true);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> FireSignal<TMessage>(string signalName, Expression<Func<TPreviousMessage, TMessage>> message)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal one by message
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> FireSignals<TMessage>(string signalName, Expression<Func<TPreviousMessage, IEnumerable<TMessage>>> messages)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> FireSignal<TMessage>(Guid signalName, Expression<Func<TPreviousMessage, TMessage>> message)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal one by message
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> FireSignals<TMessage>(Guid signalId, Expression<Func<TPreviousMessage, IEnumerable<TMessage>>> messages)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> FireSignal<TMessage>(SignalId signalName, Expression<Func<TPreviousMessage, TMessage>> message)
            where TMessage : struct;

        /// <summary>
        /// Fire a designated signal one by message
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> FireSignals<TMessage>(SignalId signalId, Expression<Func<TPreviousMessage, IEnumerable<TMessage>>> messages)
            where TMessage : struct;

        /// <summary>
        /// Add a new converter <see cref="ITransformerConvertVGrain{TInput}"/> in the pipeline to transform input into output
        /// </summary>
        ISequencePipelineStageCallBuilder<TPreviousMessage, TConverterVGrain> Convert<TConverterVGrain>(Action<ISequencePipelineStageConfigurator>? cfg = null)
            where TConverterVGrain : IVGrain;

        /// <summary>
        /// Add a new stage used to call an existing sequence
        /// </summary>
        ISequencePipelineNestedSequenceCallBuilder<TPreviousMessage> CallSequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null, Action<ISequencePipelineStageConfigurator>? cfg = null);

    }
}
