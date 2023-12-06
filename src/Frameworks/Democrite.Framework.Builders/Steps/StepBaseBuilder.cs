// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Steps
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Base class of <see cref="ISequencePipelineInternalStageStep"/>
    /// </summary>
    /// <seealso cref="ISequencePipelineInternalStageStep" />
    public abstract class StepBaseBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CallStepBuilder"/> class.
        /// </summary>
        protected StepBaseBuilder(Type? input, Type? output)
        {
            this.Input = input;
            this.Output = output;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the input.
        /// </summary>
        protected Type? Input { get; }

        /// <summary>
        /// Gets the output.
        /// </summary>
        protected Type? Output { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public abstract ISequenceStageDefinition ToDefinition<TContext>(SequenceOptionStageDefinition? option, bool preventReturn, TContext? contextInfo = default);

        #endregion
    }
}
