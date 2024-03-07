// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Sequences
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Requests;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Build Helper base usable for pull and push
    /// </summary>
    public interface IBlackboardSequenceHelperBaseBuilder<TWizard, TRequestBuilder, TRequestOption>
    where TRequestOption : IDataRecordRequestOption
    {
        TWizard UseTargetBuilder<TBuilder>()
            where TBuilder : TRequestBuilder;

        TWizard Option<TOption>(Func<TOption> option)
            where TOption : TRequestOption;
    }

    /*
     * 
     *  PULL
     * 
     */

    /// <summary>
    /// Build helper start point for pull
    /// </summary>
    public interface IBlackboardSequenceHelperPullStartBuilder
    {
        IBlackboardSequenceHelperPullBuilder<TOutput> Request<TOutput>(string? LogicalTypePattern = null,
                                                                       RecordStatusEnum? RecordStatusFilter = null);

        IBlackboardSequenceHelperPullBuilder<IReadOnlyCollection<TOutput>> RequestAll<TOutput>(string? LogicalTypePattern = null,
                                                                                               RecordStatusEnum? RecordStatusFilter = null);
    }

    /// <summary>
    /// Build helper start point for pull with an input
    /// </summary>
    public interface IBlackboardSequenceHelperPullStartBuilder<TInput>
    {
        #region Properties

        TInput Input { get; }

        #endregion

        IBlackboardSequenceHelperPullBuilder<TInput, TOutput> Request<TOutput>(string? LogicalTypePattern = null,
                                                                               RecordStatusEnum? RecordStatusFilter = null);

        IBlackboardSequenceHelperPullBuilder<TInput, IReadOnlyCollection<TOutput>> RequestAll<TOutput>(string? LogicalTypePattern = null,
                                                                                                       RecordStatusEnum? RecordStatusFilter = null);
    }

    public interface IBlackboardSequenceHelperPullBaseBuilder<TWizard, TOutput> : IBlackboardSequenceHelperBaseBuilder<TWizard, IBlackboardStoragePullRequestBuilderVGrain, IDataRecordPullRequestOption>, IBlackboardSequenceHelperPullBuilderResult<TOutput>
        where TWizard : IBlackboardSequenceHelperPullBaseBuilder<TWizard, TOutput>
    {
    }

    public interface IBlackboardSequenceHelperPullBuilder<TOutput> : IBlackboardSequenceHelperPullBaseBuilder<IBlackboardSequenceHelperPullBuilder<TOutput>, TOutput>
    {
    }

    public interface IBlackboardSequenceHelperPullBuilder<TInput, TOutput> : IBlackboardSequenceHelperPullBaseBuilder<IBlackboardSequenceHelperPullBuilder<TInput, TOutput>, TOutput>
    {

    }

    public interface IBlackboardSequenceHelperPullBuilderResult<TOutput>
    {
    }


    /*
     * 
     * PUSH
     * 
     */

    /// <summary>
    /// Build helper start point for pushing with an input
    /// </summary>
    public interface IBlackboardSequenceHelperPushBuilder<TInput> : IBlackboardSequenceHelperBaseBuilder<IBlackboardSequenceHelperPushBuilder<TInput>, IBlackboardStoragePushRequestBuilderVGrain, IDataRecordPushRequestOption>
    {

    }

}
