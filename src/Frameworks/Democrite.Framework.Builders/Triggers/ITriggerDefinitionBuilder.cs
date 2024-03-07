// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;

    /// <summary>
    /// Common part of trigger definition
    /// </summary>
    public interface ITriggerDefinitionBuilder<TDefWithExtention>
        where TDefWithExtention : ITriggerDefinitionBuilder<TDefWithExtention>
    {
        /*
         * Sequences
         */

        /// <summary>
        /// Adds the targe id. Sample:  Sequence.Uid
        /// </summary>
        TDefWithExtention AddTargetSequence(Guid targetSequenceId, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null);

        /// <summary>
        /// Adds the targe id. Sample:  Sequence.Uid
        /// </summary>
        TDefWithExtention AddTargetSequences(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params Guid[] targetSequenceId);

        /// <summary>
        /// Adds the targe id. Sample:  Sequence.Uid
        /// </summary>
        TDefWithExtention AddTargetSequences(params Guid[] targetSequenceId);

        /// <summary>
        /// Adds the targe id. Sample:  Sequence.Uid
        /// </summary>
        TDefWithExtention AddTargetSequence(SequenceDefinition sequenceDefinition, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null);

        /// <summary>
        /// Adds the targe id. Sample:  Sequence.Uid
        /// </summary>
        TDefWithExtention AddTargetSequences(params SequenceDefinition[] sequenceDefinition);

        /// <summary>
        /// Adds the targe id. Sample:  Sequence.Uid
        /// </summary>
        TDefWithExtention AddTargetSequences(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params SequenceDefinition[] sequenceDefinition);

        /*
         * Signals
         */

        /// <summary>
        /// Adds a signal as target
        /// </summary>
        TDefWithExtention AddTargetSignal(SignalId signalId, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null);

        /// <summary>
        /// Adds a signal as target
        /// </summary>
        TDefWithExtention AddTargetSignals(params SignalId[] signalId);

        /// <summary>
        /// Adds a signal as target
        /// </summary>
        TDefWithExtention AddTargetSignals(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params SignalId[] signalId);

        /// <summary>
        /// Adds a signal as target
        /// </summary>
        TDefWithExtention AddTargetSignal(Guid signalId, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null);

        /// <summary>
        /// Adds a signal as target
        /// </summary>
        TDefWithExtention AddTargetSignals(params Guid[] signalId);

        /// <summary>
        /// Adds a signal as target
        /// </summary>
        TDefWithExtention AddTargetSignals(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params Guid[] signalId);

        /*
         * Stream
         */

        /// <summary>
        /// Adds the targe stream <see cref="StreamQueueDefinition.Uid"/>
        /// </summary>
        TDefWithExtention AddTargetStream(Guid targetStreamId, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null);

        /// <summary>
        /// Adds the targe stream <see cref="StreamQueueDefinition.Uid"/>
        /// </summary>
        TDefWithExtention AddTargetStreams(params Guid[] targetStreamId);

        /// <summary>
        /// Adds the targe stream <see cref="StreamQueueDefinition.Uid"/>
        /// </summary>
        TDefWithExtention AddTargetStreams(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params Guid[] targetStreamId);

        /// <summary>
        /// Adds the targe stream <see cref="StreamQueueDefinition.Uid"/>
        /// </summary>
        TDefWithExtention AddTargetStream(StreamQueueDefinition streamDefinition, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null);

        /// <summary>
        /// Adds the targe stream <see cref="StreamQueueDefinition.Uid"/>
        /// </summary>
        TDefWithExtention AddTargetStreams(params StreamQueueDefinition[] streamDefinition);

        /// <summary>
        /// Adds the targe stream <see cref="StreamQueueDefinition.Uid"/>
        /// </summary>
        TDefWithExtention AddTargetStreams(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params StreamQueueDefinition[] streamDefinition);
    }
}
