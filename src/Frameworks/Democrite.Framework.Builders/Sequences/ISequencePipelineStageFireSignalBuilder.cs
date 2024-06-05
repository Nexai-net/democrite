// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Build a fire signal stage
    /// </summary>
    public interface ISequencePipelineStageFireSignalBuilder<TSequenceStageBuilder>
    {
        /// <summary>
        /// Specify a fix message
        /// </summary>
        TSequenceStageBuilder NoMessage();

        /// <summary>
        /// Specify a fix message
        /// </summary>
        TSequenceStageBuilder Message<TMessage>(TMessage message)
            where TMessage : struct;

        /// <summary>
        /// Specify a fix message collection
        /// </summary>
        TSequenceStageBuilder Message<TMessage>(IEnumerable<TMessage> messages)
            where TMessage : struct;
    }

    /// <summary>
    /// Build a fire signal stage
    /// </summary>
    public interface ISequencePipelineStageFireSignalBuilder<TSequenceStageBuilder, TPreviousMessage> : ISequencePipelineStageFireSignalBuilder<TSequenceStageBuilder>
    {
        /// <summary>
        /// Specify a message from input
        /// </summary>
        TSequenceStageBuilder Message<TMessage>(Expression<Func<TPreviousMessage, TMessage>> messageAccess)
            where TMessage : struct;

        /// <summary>
        /// Specify a message collection from input
        /// </summary>
        TSequenceStageBuilder Messages<TMessage, TResult>(Expression<Func<TPreviousMessage, TResult>> messageAccess)
            where TMessage : struct
            where TResult : IEnumerable<TMessage>;

    }
}
