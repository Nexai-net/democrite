// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Elvex.Toolbox;

    using System;

    /// <summary>
    /// Entry point to build <see cref="SequenceDefinition"/>
    /// </summary>
    internal sealed class SequenceBuilder : ISequenceBuilder
    {
        #region Fields  

        private ISequencePipelineBuilder? _root;

        private readonly Queue<ISequencePipelineStageDefinitionProvider> _stages;

        private readonly SequenceOptionDefinition _options;
        private readonly SequenceBuilder? _parent;

        private readonly string? _displayName;
        private readonly string _simpleNameIdentifier;
        private readonly Guid _uid;
        private readonly DefinitionMetaData? _definitionMetaData;

        #endregion

        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="SequenceBuilder"/> class from being created.
        /// </summary>
        /// <param name="options">Global sequence option</param>
        /// <param name="displayName"></param>
        /// <param name="triggerDefinition">All information about the worflow trigger; if null the sequence will need manual trigger</param>
        internal SequenceBuilder(Guid uid,
                                 string simpleNameIdentifier,
                                 string displayName,
                                 SequenceOptionDefinition? options,
                                 DefinitionMetaData? definitionMetaData)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(displayName);

            this._options = options ?? SequenceOptionDefinition.Default;
            this._displayName = displayName;
            this._simpleNameIdentifier = simpleNameIdentifier;
            this._uid = uid;
            this._definitionMetaData = definitionMetaData;

            this._stages = new Queue<ISequencePipelineStageDefinitionProvider>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceBuilder"/> class link to a parent one
        /// </summary>
        internal SequenceBuilder(SequenceBuilder parent, string displayName)
            : this(Guid.NewGuid(), parent._simpleNameIdentifier, displayName, parent._options, parent._definitionMetaData)
        {
            this._parent = parent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the parent builder.
        /// </summary>
        public ISequenceBuilder? Parent
        {
            get { return this._parent; }
        }

        #endregion

        #region Methods

        #region Trigger

        /// <summary>
        /// Define the sequence will only be triggered manually
        /// </summary>
        public ISequencePipelineBuilder NoInput()
        {
            return (SequencePipelineBuilder<NoneType>)RequiredInput<NoneType>();
        }

        /// <summary>
        /// Define the sequence will only be triggered manually
        /// </summary>
        public ISequencePipelineBuilder<TInput> RequiredInput<TInput>()
        {
            var root = new SequencePipelineBuilder<TInput>(this);
            this._root = root;
            return root;
        }

        #endregion

        #region Tools

        /// <summary>
        /// Enqueue a new stage
        /// </summary>
        internal void EnqueueStage(ISequencePipelineStageDefinitionProvider stage)
        {
            this._stages.Enqueue(stage);
        }

        /// <inheritdoc cref="ISequencePipelineBaseBuilder.Build()"/>
        public SequenceDefinition Build()
        {
            return new SequenceDefinition(this._uid,
                                          RefIdHelper.Generate(RefTypeEnum.Sequence, this._simpleNameIdentifier, this._definitionMetaData?.NamespaceIdentifier), 
                                          this._displayName,
                                          this._options,
                                          this._stages.Select(s => s.ToDefinition()).ToArray(),
                                          this._definitionMetaData);
        }

        #endregion

        #endregion
    }
}
