// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using System.Linq.Expressions;

    /// <summary>
    /// Builder for sequence calling sequence stage
    /// </summary>
    public interface ISequencePipelineNestedSequenceCallBaseBuilder<TWizard>
    {
        #region Properties

        /// <summary>
        /// Return sequence call without any result
        /// </summary>
        ISequencePipelineBuilder ReturnNoData { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the called sequence input.
        /// </summary>
        TWizard SetInput<TInput>(TInput input);

        /// <summary>
        /// Return sequence call with result <typeparamref name="TOutput"/>
        /// </summary>
        ISequencePipelineBuilder<TOutput> Return<TOutput>();

        #endregion
    }

    /// <summary>
    /// Builder for sequence calling sequence stage
    /// </summary>
    public interface ISequencePipelineNestedSequenceCallBuilder : ISequencePipelineNestedSequenceCallBaseBuilder<ISequencePipelineNestedSequenceCallBuilder>
    {
    }

    /// <summary>
    /// Builder for sequence calling sequence stage
    /// </summary>
    public interface ISequencePipelineNestedSequenceCallBuilder<TPreviousMessage> : ISequencePipelineNestedSequenceCallBaseBuilder<ISequencePipelineNestedSequenceCallBuilder<TPreviousMessage>>
    {
        #region Properties

        /// <summary>
        /// Return sequence call with input data
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> ReturnCallInput { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the called sequence input.
        /// </summary>
        ISequencePipelineNestedSequenceCallBuilder<TPreviousMessage> OverrideInput<TInput>(Expression<Func<TPreviousMessage, TInput>> inputProvider);

        /// <summary>
        /// Return sequence call with input data with a specific method to received the result
        /// </summary>
        ISequencePipelineBuilder<TPreviousMessage> ReturnUpdatedInput<TOutput>(Expression<Action<TPreviousMessage, TOutput>> setMethod);

        #endregion
    }
}
