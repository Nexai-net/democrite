// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Sequence.Stages
{
    public interface IFlowHostStageDefinition
    {
        /// <summary>
        /// Gets the inner flow
        /// </summary>
        SequenceDefinition InnerFlow { get; }
    }
}
