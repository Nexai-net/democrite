// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Implementations.Triggers
{
    using Democrite.Framework.Builders.Triggers;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using System;
    using System.Linq;

    /// <summary>
    /// Get trigger definition
    /// </summary>
    /// <typeparam name="TOutputMesage">The type of the output mesage.</typeparam>
    /// <seealso cref="ITriggerDefinitionBuilder" />
    internal abstract class TriggerDefinitionBaseBuilder<TDefWithExtention> : ITriggerDefinitionBuilder<TDefWithExtention>
        where TDefWithExtention : ITriggerDefinitionBuilder<TDefWithExtention>
    {
        #region Fields

        private readonly HashSet<TriggerTargetDefinition> _targets;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ITriggerDefinitionBuilder"/> class.
        /// </summary>
        protected TriggerDefinitionBaseBuilder(TriggerTypeEnum triggerType, string displayName, Guid? fixUid = null)
        {

            this._targets = new HashSet<TriggerTargetDefinition>();
            this.TriggerType = triggerType;

            this.Uid = fixUid ?? Guid.NewGuid();

            ArgumentNullException.ThrowIfNullOrEmpty(displayName);
            this.DisplayName = displayName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the trigger.
        /// </summary>
        protected TriggerTypeEnum TriggerType { get; private set; }

        /// <summary>
        /// Gets the uid.
        /// </summary>
        protected Guid Uid { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        protected string DisplayName { get; }

        /// <summary>
        /// Gets targets infomation
        /// </summary>
        protected IReadOnlyCollection<TriggerTargetDefinition> Targets
        {
            get { return this._targets; }
        }

        /// <summary>
        /// Gets the definition meta data.
        /// </summary>
        public DefinitionMetaData? DefinitionMetaData { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TDefWithExtention MetaData(Action<IDefinitionMetaDataBuilder> action)
        {
            if (action is not null)
            {
                var inst = new DefinitionMetaDataBuilder();
                action?.Invoke(inst);

                this.DefinitionMetaData = inst.Build(out _);
            }

            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSequence(Guid targetSequenceId, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null)
        {
            return AddTarget(targetSequenceId, TargetTypeEnum.Sequence, dedicatedOutputBuilders);
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSequences(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params Guid[] targetSequenceId)
        {
            foreach (var targetId in targetSequenceId)
                AddTargetSequence(targetId, dedicatedOutputBuilders);
            return GetBuilderFinalizer();
        }
       
        /// <inheritdoc />
        public TDefWithExtention AddTargetSequences(params Guid[] targetSequenceId)
        {
            foreach (var targetId in targetSequenceId)
                AddTargetSequence(targetId, (Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>?)null);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSequence(SequenceDefinition sequenceDefinition, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null)
        {
            AddTargetSequence(sequenceDefinition.Uid, dedicatedOutputBuilders);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSequences(params SequenceDefinition[] sequenceDefinition)
        {
            foreach (var seq in sequenceDefinition)
                AddTargetSequence(seq.Uid, (Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>?)null);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSequences(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params SequenceDefinition[] sequenceDefinition)
        {
            foreach (var seq in sequenceDefinition)
                AddTargetSequence(seq.Uid, dedicatedOutputBuilders);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSignal(Guid signalId, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null)
        {
            return AddTarget(signalId, TargetTypeEnum.Signal, dedicatedOutputBuilders);
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSignal(SignalId signalId, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null)
        {
            return AddTargetSignal(signalId.Uid, dedicatedOutputBuilders);
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSignals(params SignalId[] signalIds)
        {
            foreach (var signalId in signalIds)
                AddTargetSignal(signalId.Uid, null);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSignals(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params SignalId[] signalIds)
        {
            foreach (var signalId in signalIds)
                AddTargetSignal(signalId.Uid, dedicatedOutputBuilders);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSignals(params Guid[] signalIds)
        {
            foreach (var signalId in signalIds)
                AddTargetSignal(signalId, null);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetSignals(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params Guid[] signalIds)
        {
            foreach (var signalId in signalIds)
                AddTargetSignal(signalId, dedicatedOutputBuilders);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetStream(Guid targetStreamId, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null)
        {
            return AddTarget(targetStreamId, TargetTypeEnum.Stream, dedicatedOutputBuilders);
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetStream(StreamQueueDefinition streamDefinition, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null)
        {
            return AddTargetStream(streamDefinition.Uid, dedicatedOutputBuilders);
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetStreams(params Guid[] targetStreamIds)
        {
            foreach (var targetStreamId in targetStreamIds)
                AddTargetStream(targetStreamId, null);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetStreams(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params Guid[] targetStreamIds)
        {
            foreach (var targetStreamId in targetStreamIds)
                AddTargetStream(targetStreamId, dedicatedOutputBuilders);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetStreams(params StreamQueueDefinition[] streamDefinitions)
        {
            foreach (var streamDefinition in streamDefinitions)
                AddTargetStream(streamDefinition.Uid, null);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public TDefWithExtention AddTargetStreams(Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>> dedicatedOutputBuilders, params StreamQueueDefinition[] streamDefinitions)
        {
            foreach (var streamDefinition in streamDefinitions)
                AddTargetStream(streamDefinition.Uid, dedicatedOutputBuilders);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public abstract TriggerDefinition Build();

        #region Tools

        /// <summary>
        /// Adds the target.
        /// </summary>
        private TDefWithExtention AddTarget(Guid uid, TargetTypeEnum type, Func<ITriggerOutputBuilder, IDefinitionBaseBuilder<DataSourceDefinition>>? dedicatedOutputBuilders = null)
        {
            this._targets.Add(new TriggerTargetDefinition(uid, type, dedicatedOutputBuilders?.Invoke(new TriggerOutputBuilder())?.Build(), null));
            return GetBuilderFinalizer();
        }

        /// <summary>
        /// Gets the builder finalizer.
        /// </summary>
        protected virtual TDefWithExtention GetBuilderFinalizer()
        {
            if (this is TDefWithExtention finalizeBuilder)
                return finalizeBuilder;

            throw new NotImplementedException("Plz overrider GetBuilderFinalizer for " + GetType());
        }

        #endregion

        #endregion
    }
}
