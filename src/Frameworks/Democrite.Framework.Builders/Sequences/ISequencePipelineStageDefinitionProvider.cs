// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Get pipeline stage
    /// </summary>
    public interface ISequencePipelineStageDefinitionProvider
    {
        /// <summary>
        /// Converts to definition.
        /// </summary>
        SequenceStageDefinition ToDefinition();
    }
}
