// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;

    /// <summary>
    /// base class to every stage builder
    /// </summary>
    public abstract class SequencePipelineStageBaseBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineStageBaseBuilder{TInput}"/> class.
        /// </summary>
        public SequencePipelineStageBaseBuilder(Action<ISequencePipelineStageConfigurator>? configAction)
        {
            this.ConfigAction = configAction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the configuration action.
        /// </summary>
        protected Action<ISequencePipelineStageConfigurator>? ConfigAction { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Configurations the option.
        /// </summary>
        protected virtual SequenceOptionStageDefinition? BuildConfigDefinition()
        {
            if (this.ConfigAction != null)
            {
                var builder = new SequencePipelineConfiguratorStageBuilder();
                this.ConfigAction?.Invoke(builder);
                return builder.ToDefinition();
            }
            return null;
        }

        #endregion
    }
}
