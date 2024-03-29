﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// 
    /// </summary>
    internal interface IInternalSequencePipelineVGrainStageBaseBuilder
    {
        /// <summary>
        /// Gets the sequence pipeline builder.
        /// </summary>
        ISequencePipelineBaseBuilder SequencePipelineBuilder { get; }

        /// <summary>
        /// Prevents the output.
        /// </summary>
        void PreventOutput();

        /// <summary>
        /// Converts to definition.
        /// </summary>
        SequenceStageBaseDefinition? ToDefinition();

        /// <summary>
        /// Configurations the option.
        /// </summary>
        SequenceOptionStageDefinition? ConfigOption();
    }
}
