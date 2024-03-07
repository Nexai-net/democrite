// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Sequences;
    using Democrite.Framework.Builders.Steps;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

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
                                                                                                                  Action<ISequencePipelineStageConfigurator<TInputMessage>>? cfg = null)
        {
            var eachSequenceBuilder = stageBuilder.CreateSubSequence(nameof(Foreach));
            var pipeline = eachSequenceBuilder.RequiredInput<TSplitInputMessage>();

            var result = foreachConfigStage(pipeline);

            var setAbstractCall = (subInputSetAccess.Body as MethodCallExpression)?.Method.GetAbstractMethod() ?? throw new InvalidOperationException("Set Method must be a simple input.Set(Output)");

            return stageBuilder.EnqueueStage<TInputMessage>(new SequencePipelineForeachStageBuilder<TInputMessage>(eachSequenceBuilder,
                                                                                                                   subInputGetAccess.CreateAccess(),
                                                                                                                   setAbstractCall!,
                                                                                                                   relayInput: true,
                                                                                                                   configAction: cfg));
        }

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
            return stageBuilder.EnqueueStage<IEnumerable<TOutput>>(new SequencePipelineForeachStageBuilder<TInputMessage>(eachSequenceBuilder, configAction: cfg));
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

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<TInputMessage> FireSignalRelayMessage<TInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder, string signalName)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignal(signalName, i => i);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<IEnumerable<TInputMessage>> FireSignalRelayMessages<TInputMessage>(this ISequencePipelineBuilder<IEnumerable<TInputMessage>> stageBuilder, string signalName)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignals(signalName, i => i);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<IReadOnlyCollection<TInputMessage>> FireSignalRelayMessages<TInputMessage>(this ISequencePipelineBuilder<IReadOnlyCollection<TInputMessage>> stageBuilder, string signalName)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignals(signalName, i => i);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<TInputMessage> FireSignalRelayMessage<TInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder, Guid signalId)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignal(signalId, i => i);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<IEnumerable<TInputMessage>> FireSignalRelayMessages<TInputMessage>(this ISequencePipelineBuilder<IEnumerable<TInputMessage>> stageBuilder, Guid signalId)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignals(signalId, i => i);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<IReadOnlyCollection<TInputMessage>> FireSignalRelayMessages<TInputMessage>(this ISequencePipelineBuilder<IReadOnlyCollection<TInputMessage>> stageBuilder, Guid signalId)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignals(signalId, i => i);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<TInputMessage> FireSignalRelayMessage<TInputMessage>(this ISequencePipelineBuilder<TInputMessage> stageBuilder, SignalId signalId)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignal(signalId, i => i);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<IEnumerable<TInputMessage>> FireSignalRelayMessages<TInputMessage>(this ISequencePipelineBuilder<IEnumerable<TInputMessage>> stageBuilder, SignalId signalId)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignals(signalId, i => i);
        }

        /// <summary>
        /// Fire a designated signal and relay through this signal the current message (Attention this one must be a structure)
        /// </summary>
        public static ISequencePipelineBuilder<IReadOnlyCollection<TInputMessage>> FireSignalRelayMessages<TInputMessage>(this ISequencePipelineBuilder<IReadOnlyCollection<TInputMessage>> stageBuilder, SignalId signalId)
            where TInputMessage : struct
        {
            return stageBuilder.FireSignals(signalId, i => i);
        }
    }
}
