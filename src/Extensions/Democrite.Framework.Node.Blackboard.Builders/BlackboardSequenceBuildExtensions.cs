// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Builders to easy usage

namespace Democrite.Framework.Node.Blackboard.Builders
{
    using Democrite.Framework.Builders.Sequences;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Requests;
    using Democrite.Framework.Node.Blackboard.Builders.Sequences;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions method used to encapsulate and 
    /// </summary>
    public static class BlackboardSequenceBuildExtensions
    {
        public static ISequencePipelineBuilder<TOutput> GetValueFromBlackboard<TOutput>(this ISequencePipelineBuilder pipelineBuilder,
                                                                                        Func<IBlackboardSequenceHelperPullStartBuilder, IBlackboardSequenceHelperPullBuilderResult<TOutput>>? builder = null)
        {
            throw new NotImplementedException();
        }

        public static ISequencePipelineBuilder<TOutput> GetValueFromBlackBoard<TPreviousMessage, TOutput>(this ISequencePipelineBuilder<TPreviousMessage> pipelineBuilder,
                                                                                                          Func<IBlackboardSequenceHelperPullStartBuilder<TPreviousMessage>, IBlackboardSequenceHelperPullBuilderResult<TOutput>>? builder = null)
        {
            throw new NotImplementedException();
        }

        public static ISequencePipelineBuilder<TPreviousMessage> PushValueToBlackBoard<TPreviousMessage>(this ISequencePipelineBuilder<TPreviousMessage> pipelineBuilder,
                                                                                                         Action<IBlackboardSequenceHelperPushBuilder<TPreviousMessage>>? builder = null)
        {
            throw new NotImplementedException();
        }
    }
}
