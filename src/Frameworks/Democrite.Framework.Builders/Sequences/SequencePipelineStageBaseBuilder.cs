// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions;

    using System;

    /// <summary>
    /// base class to every stage builder
    /// </summary>
    public abstract class SequencePipelineStageBaseBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineStageBaseBuilder"/> class.
        /// </summary>
        public SequencePipelineStageBaseBuilder(Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction, Guid? fixUid)
        {
            this.FixUid = fixUid ?? Guid.NewGuid();
            this.MetaDataBuilderAction = metaDataBuilderAction;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Guid FixUid { get; }

        /// <summary>
        /// Gets the configuration action.
        /// </summary>
        protected Action<IDefinitionMetaDataWithDisplayNameBuilder>? MetaDataBuilderAction { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Build stage definition metadata
        /// </summary>
        protected virtual DefinitionMetaData? BuildDefinitionMetaData(out string? displayName)
        {
            if (this.MetaDataBuilderAction != null)
            {
                var builder = new DefinitionMetaDataBuilder();
                this.MetaDataBuilderAction?.Invoke(builder);
                return builder.Build(out displayName, out _);
            }
            displayName = null; 
            return null;
        }

        #endregion
    }
}
