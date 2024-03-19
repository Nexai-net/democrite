// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    /// <summary>
    /// Final step of a sequence stage build
    /// </summary>
    /// <typeparam name="TInputMessage">The type of the input message.</typeparam>
    /// <typeparam name="TSequenceVGrain">The type of the sequence virtual grain.</typeparam>
    public interface ISequencePipelineStageFinalizerBuilder<TOutputMessage, TSequenceVGrain>
    {
        /// <summary>
        /// Send a copy of the current message to another target
        /// </summary>
        /// <remarks>
        ///     Attention the target reponse will not be waited
        /// </remarks>
        ISequencePipelineStageFinalizerBuilder<TOutputMessage, TSequenceVGrain> SendCopyTo(Action<string> targetDefinition);

        /// <summary>
        /// Gets the endpoint of an actor definition
        /// </summary>
        ISequencePipelineBuilder<TOutputMessage> Return { get; }

        /// <summary>
        /// Gets the endpoint of an actor definition without using the <typeparamref name="TOutputMessage"/>
        /// </summary>
        ISequencePipelineBuilder ReturnNoData { get; }
    }

    /// <summary>
    /// Final step of a sequence stage build
    /// </summary>
    /// <typeparam name="TSequenceVGrain">The type of the sequence virtual grain.</typeparam>
    public interface ISequencePipelineStageFinalizerBuilder<TSequenceVGrain>
    {
        /// <summary>
        /// Send a copy of the current message to another target
        /// </summary>
        /// <remarks>
        ///     Attention the target reponse will not be waited
        /// </remarks>
        ISequencePipelineStageFinalizerBuilder<TSequenceVGrain> SendCopyTo(Action<string> targetDefinition);

        /// <summary>
        /// Gets the endpoint of an actor definition
        /// </summary>
        ISequencePipelineBuilder Return { get; }
    }
}
