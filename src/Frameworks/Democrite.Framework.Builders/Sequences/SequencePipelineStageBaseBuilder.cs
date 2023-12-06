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
    /// <typeparam name="TInput">The type of the input.</typeparam>
    public abstract class SequencePipelineStageBaseBuilder<TInput>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineStageBaseBuilder{TInput}"/> class.
        /// </summary>
        public SequencePipelineStageBaseBuilder(Action<ISequencePipelineStageConfigurator<TInput>>? configAction)
        {
            this.ConfigAction = configAction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the configuration action.
        /// </summary>
        protected Action<ISequencePipelineStageConfigurator<TInput>>? ConfigAction { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Configurations the option.
        /// </summary>
        protected virtual SequenceOptionStageDefinition? BuildConfigDefinition()
        {
            if (this.ConfigAction != null)
            {
                var builder = new SequencePipelineConfiguratorStageBuilder<TInput>();
                this.ConfigAction?.Invoke(builder);
                return builder.ToDefinition();
            }
            return null;
        }

        #endregion
    }
}
