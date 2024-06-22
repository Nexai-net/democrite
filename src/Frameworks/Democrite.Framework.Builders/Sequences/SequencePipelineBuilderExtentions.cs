// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Sequences;

    using Elvex.Toolbox;

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
        public static ISequencePipelineBuilder<TInputMessage> Foreach<TInputMessage, TOutput, TSplitInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder,
                                                                                                                  Expression<Func<TInputMessage, IEnumerable<TSplitInputMessage>>> subInputGetAccess,
                                                                                                                  Func<ISequencePipelineBuilder<TSplitInputMessage>, ISequencePipelineBuilder<TOutput>> foreachConfigStage,
                                                                                                                  Expression<Action<TInputMessage, IEnumerable<TOutput>>> subInputSetAccess,
                                                                                                                  Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction = null,
                                                                                                                  Guid? fixUid = null)
        {
            var eachSequenceBuilder = stageBuilder.CreateSubSequence(nameof(Foreach));
            var pipeline = eachSequenceBuilder.RequiredInput<TSplitInputMessage>();

            var result = foreachConfigStage(pipeline);

            var setAbstractCall = (subInputSetAccess.Body as MethodCallExpression)?.Method.GetAbstractMethod() ?? throw new InvalidOperationException("Set Method must be a simple input.Set(Output)");

            return stageBuilder.EnqueueStage<TInputMessage>(new SequencePipelineForeachStageBuilder<TInputMessage>(eachSequenceBuilder,
                                                                                                                   subInputGetAccess.CreateAccess(),
                                                                                                                   setAbstractCall!,
                                                                                                                   relayInput: true,
                                                                                                                   metaDataBuilderAction,
                                                                                                                   fixUid));
        }

        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        public static ISequencePipelineBuilder<IEnumerable<TOutput>> Foreach<TInputMessage, TOutput, TSplitInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder,
                                                                                                                         TSplitInputMessage _,
                                                                                                                         Func<ISequencePipelineBuilder<TSplitInputMessage>, ISequencePipelineBuilder<TOutput>> foreachConfigStage,
                                                                                                                         Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction = null,
                                                                                                                         Guid? fixUid = null)
        {
            var eachSequenceBuilder = stageBuilder.CreateSubSequence(nameof(Foreach));
            var pipeline = eachSequenceBuilder.RequiredInput<TSplitInputMessage>();

            var result = foreachConfigStage(pipeline);
            return stageBuilder.EnqueueStage<IEnumerable<TOutput>>(new SequencePipelineForeachStageBuilder<TInputMessage>(eachSequenceBuilder, metaDataBuilderAction: metaDataBuilderAction, fixUid: fixUid));
        }

        /// <summary>
        /// Record an virtual grain method to call on the pipeline
        /// </summary>
        public static ISequencePipelineBuilder Foreach<TInputMessage, TSplitInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder,
                                                                                          TSplitInputMessage _,
                                                                                          Func<ISequencePipelineBuilder<TSplitInputMessage>, ISequencePipelineBuilder> foreachConfigStage,
                                                                                          Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction = null,
                                                                                          Guid? fixUid = null)
            where TInputMessage : IEnumerable<TSplitInputMessage>
        {
            return (ISequencePipelineBuilder)stageBuilder.Foreach(_, each => (ISequencePipelineBuilder<NoneType>)foreachConfigStage(each), metaDataBuilderAction, fixUid);
        }

        /// <summary>
        /// Filter <see cref="IEnumerable{TInputMessage}"/> to return only the ones that matche
        /// </summary>
        public static ISequencePipelineBuilder<TInputMessage> Filter<TInputMessage, TItemInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder,
                                                                                                       Expression<Func<TItemInputMessage, bool>> conditions,
                                                                                                       Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction = null,
                                                                                                       Guid? fixUid = null)
            where TInputMessage : IEnumerable<TItemInputMessage>
        {
            var serializable = conditions.Serialize();

            return stageBuilder.EnqueueStage<TInputMessage>(new SequencePipelineFilterStageBuilder<TInputMessage>(serializable, metaDataBuilderAction, fixUid));
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static TSequenceStageBuilder RelayMessage<TPreviousMessage, TSequenceStageBuilder>(this ISequencePipelineStageFireSignalBuilder<TSequenceStageBuilder, TPreviousMessage> stageBuilder)
            where TPreviousMessage : struct
        {
            return stageBuilder.Message<TPreviousMessage>(m => m);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static TSequenceStageBuilder RelayMessages<TPreviousMessage, TSequenceStageBuilder, TMessage>(this ISequencePipelineStageFireSignalBuilder<TSequenceStageBuilder, TPreviousMessage> stageBuilder, TMessage _)
            where TPreviousMessage : IEnumerable<TMessage>
            where TMessage : struct
        {
            return stageBuilder.Messages<TMessage, TPreviousMessage>(m => m);
        }
    }
}
