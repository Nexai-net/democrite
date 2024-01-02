// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Implementations.Triggers
{
    using Democrite.Framework.Builders.Triggers;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using System;

    /// <summary>
    /// Build trigger definition
    /// </summary>
    /// <typeparam name="TOutputMesage">The type of the output mesage.</typeparam>
    /// <seealso cref="ITriggerDefinitionBuilder" />
    internal abstract class TriggerDefinitionBaseBuilder : ITriggerDefinitionBuilder, ITriggerDefinitionFinalizeBuilder, ITriggerInputBuilder
    {
        #region Fields

        private readonly HashSet<Guid> _targetSequenceIds;
        private readonly HashSet<SignalId> _targetSignalIds;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ITriggerDefinitionBuilder"/> class.
        /// </summary>
        protected TriggerDefinitionBaseBuilder(TriggerTypeEnum triggerType)
        {
            this._targetSequenceIds = new HashSet<Guid>();
            this._targetSignalIds = new HashSet<SignalId>();
            this.TriggerType = triggerType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the trigger input definition.
        /// </summary>
        protected InputSourceDefinition? TriggerInputDefinition { get; private set; }

        /// <summary>
        /// Gets the type of the trigger.
        /// </summary>
        protected TriggerTypeEnum TriggerType { get; private set; }

        /// <summary>
        /// Gets target sequence ids.
        /// </summary>
        protected IReadOnlyCollection<Guid> TargetSequenceIds
        {
            get { return this._targetSequenceIds; }
        }

        /// <summary>
        /// Gets target signal ids.
        /// </summary>
        protected IReadOnlyCollection<SignalId> TargetSignalIds
        {
            get { return this._targetSignalIds; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ITriggerDefinitionFinalizeBuilder SetInputSource(Func<ITriggerInputBuilder, InputSourceDefinition> inputBuilders)
        {
            ArgumentNullException.ThrowIfNull(nameof(inputBuilders));
            this.TriggerInputDefinition = inputBuilders(this);

            return this;
        }

        /// <inheritdoc />
        public ITriggerStaticCollectionInputBuilder<TTriggerOutput> StaticCollection<TTriggerOutput>(IEnumerable<TTriggerOutput> collection)
        {
            return new TriggerStaticCollectionInputBuilder<TTriggerOutput>(collection);
        }

        /// <inheritdoc />
        public ITriggerDefinitionFinalizeBuilder AddTargetSequence(SequenceDefinition sequenceDefinition)
        {
            ArgumentNullException.ThrowIfNull(sequenceDefinition);

            this._targetSequenceIds.Add(sequenceDefinition.Uid);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public ITriggerDefinitionFinalizeBuilder AddTargetSequence(Guid targetId)
        {
            this._targetSequenceIds.Add(targetId);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public ITriggerDefinitionFinalizeBuilder AddTargetSignal(SignalId signalId)
        {
            this._targetSignalIds.Add(signalId);
            return GetBuilderFinalizer();
        }

        /// <inheritdoc />
        public abstract TriggerDefinition Build();

        /// <summary>
        /// Gets the builder finalizer.
        /// </summary>
        protected virtual ITriggerDefinitionFinalizeBuilder GetBuilderFinalizer()
        {
            if (this is ITriggerDefinitionFinalizeBuilder finalizeBuilder)
                return finalizeBuilder;

            throw new NotImplementedException("Plz overrider GetBuilderFinalizer for " + GetType());
        }

        #endregion
    }
}
