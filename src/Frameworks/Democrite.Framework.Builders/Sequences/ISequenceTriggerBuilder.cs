// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    /// <summary>
    /// Used to build a trigger
    /// </summary>
    public interface ISequenceTriggerBuilder
    {
        /// <summary>
        /// Define the sequence need no input.
        /// </summary>
        public ISequencePipelineBuilder NoInput();

        /// <summary>
        /// Define the sequence required input <typeparamref name="TInput"/>
        /// </summary>
        public ISequencePipelineBuilder<TInput> RequiredInput<TInput>();
    }
}
