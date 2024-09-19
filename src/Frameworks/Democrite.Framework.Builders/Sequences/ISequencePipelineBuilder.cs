// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System.Linq.Expressions;

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
        /// Generic method to add a stage into the current pipeline.
        /// </summary>
        /// <returns>
        ///     Return next stage builder with input <typeparamref name="outputType"/>
        /// </returns>
        ISequencePipelineBuilder EnqueueStage(ISequencePipelineStageDefinitionProvider stage, Type? outputType);

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
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput> FireSignal(string signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput> FireSignal(Guid signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput> FireSignal(SignalId signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);
    }

    /// <summary>
    /// Builder dedicated to signal setup in the sequence
    /// </summary>
    public interface ISequencePipelineSignalBuilder<TBuilderOutput, TPreviousMessage>
    {
        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput, TPreviousMessage> FireSignal(string signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput, TPreviousMessage> FireSignal(Guid signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput, TPreviousMessage> FireSignal(SignalId signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput, TPreviousMessage> FireSignal(Expression<Func<TPreviousMessage, string>> signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput, TPreviousMessage> FireSignal(Expression<Func<TPreviousMessage, Guid>> signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Fire a designated signal
        /// </summary>
        ISequencePipelineStageFireSignalBuilder<TBuilderOutput, TPreviousMessage> FireSignal(Expression<Func<TPreviousMessage, SignalId>> signalName, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);
    }

    /// <summary>
    /// Builder dedicated to push to data context setup in the sequence
    /// </summary>
    public interface ISequencePipelineDataContextlBuilder<TBuilderOutput>
    {
        /// <summary>
        /// Add a new stage that inject in content to <see cref="IExecutionContext"/>
        /// </summary>
        ISequencePipelineBuilder PushToContext<TInfo>(Expression<Func<TInfo>> data, bool @override = true, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Add a new stage that clear context content to <see cref="IExecutionContext"/>
        /// </summary>
        TBuilderOutput ClearContextMetadata<TInfo>(Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Add a new stage that clear context content to <see cref="IExecutionContext"/>
        /// </summary>
        TBuilderOutput ClearAllContextMetadata(Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);
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
        ISequencePipelineStageCallBuilder<TVGrain> Use<TVGrain>(Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null) where TVGrain : IVGrain;

        /// <summary>
        /// Selects the specified data as input
        /// </summary>
        ISequencePipelineBuilder<TSelected> Select<TSelected>(Expression<Func<TSelected>> select, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Selects the specified data as input
        /// </summary>
        ISequencePipelineBuilder<TSelected> Select<TSelected>(TSelected select, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Add a new stage used to call an existing sequence
        /// </summary>
        ISequencePipelineNestedSequenceCallBuilder CallSequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);
    }

    /// <summary>
    /// Pipeline builder with a <typeparamref name="TPreviousMessage"/> message
    /// </summary>
    /// <typeparam name="TPreviousMessage">The type of the previous message.</typeparam>
    public interface ISequencePipelineBuilder<TPreviousMessage> : ISequencePipelineBaseBuilder,
                                                                  ISequencePipelineDataContextlBuilder<ISequencePipelineBuilder<TPreviousMessage>>,
                                                                  ISequencePipelineSignalBuilder<ISequencePipelineBuilder<TPreviousMessage>, TPreviousMessage>
    {
        /// <summary>
        /// Selects the specified data as input
        /// </summary>
        ISequencePipelineBuilder<TSelected> Select<TSelected>(Expression<Func<TPreviousMessage, TSelected>> select, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Add a new <see cref="IVGrain"/> in the pipeline to transform action based on input to produce normally and output
        /// </summary>
        ISequencePipelineStageCallBuilder<TPreviousMessage, TVGrain> Use<TVGrain>(Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null)
            where TVGrain : IVGrain;

        /// <summary>
        /// Add a new stage that inject in content to <see cref="IExecutionContext"/>
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> PushToContext<TInfo>(Expression<Func<TPreviousMessage, TInfo>> data, bool @override = true, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

        /// <summary>
        /// Add a new converter <see cref="ITransformerConvertVGrain{TInput}"/> in the pipeline to transform input into output
        /// </summary>
        ISequencePipelineStageCallBuilder<TPreviousMessage, TConverterVGrain> Convert<TConverterVGrain>(Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null)
            where TConverterVGrain : IVGrain;

        /// <summary>
        /// Add a new stage used to call an existing sequence
        /// </summary>
        ISequencePipelineNestedSequenceCallBuilder<TPreviousMessage> CallSequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null, Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilder = null, Guid? fixUid = null);

    }
}
