// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Sequences
{
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Extention methods to apply stages on each inputs
    /// </summary>
    public static class SequencePipelineBuilderExtentions
    {
        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        public static ISequencePipelineBuilder<IEnumerable<TOutput>> Foreach<TInputMessage, TOutput, TSplitInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder,
                                                                                                                         TSplitInputMessage _,
                                                                                                                         Func<ISequencePipelineBuilder<TSplitInputMessage>, ISequencePipelineBuilder<TOutput>> foreachConfigStage,
                                                                                                                         Action<ISequencePipelineStageConfigurator<TInputMessage>>? cfg = null)
        {
            var eachSequenceBuilder = stageBuilder.CreateSubSequence(nameof(Foreach));
            var pipeline = eachSequenceBuilder.RequiredInput<TSplitInputMessage>();

            var result = foreachConfigStage(pipeline);
            return stageBuilder.EnqueueStage<IEnumerable<TOutput>>(new SequencePipelineForeachStageBuilder<TInputMessage>(eachSequenceBuilder, cfg));
        }

        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        public static ISequencePipelineBuilder Foreach<TInputMessage, TSplitInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder,
                                                                                          TSplitInputMessage _,
                                                                                          Func<ISequencePipelineBuilder<TSplitInputMessage>, ISequencePipelineBuilder> foreachConfigStage,
                                                                                          Action<ISequencePipelineStageConfigurator<TInputMessage>>? cfg = null)
            where TInputMessage : IEnumerable<TSplitInputMessage>
        {
            return (ISequencePipelineBuilder)stageBuilder.Foreach(_, each => (ISequencePipelineBuilder<NoneType>)foreachConfigStage(each), cfg);
        }

        /// <summary>
        /// Filter <see cref="IEnumerable{TInputMessage}"/> to return only the ones that matche
        /// </summary>
        public static ISequencePipelineBuilder<TInputMessage> Filter<TInputMessage, TItemInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder,
                                                                                                       Expression<Func<TItemInputMessage, bool>> conditions,
                                                                                                       Action<ISequencePipelineStageConfigurator<TInputMessage>>? cfg = null)
            where TInputMessage : IEnumerable<TItemInputMessage>
        {
            var serializable = conditions.Serialize();

            return stageBuilder.EnqueueStage<TInputMessage>(new SequencePipelineFilterStageBuilder<TInputMessage>(serializable, cfg));
        }
    }
}
