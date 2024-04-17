// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Requests;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;
    using Elvex.Toolbox;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Placement;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IBlackboardStoragePullRequestBuilderVGrain" />
    [StatelessWorker]
    [PreferLocalPlacement]
    internal sealed class BlackboardStorageRequestBuilderVGrain : VGrainBase<IBlackboardStorageRequestBuilderVGrain>, IBlackboardStoragePullRequestBuilderVGrain, IBlackboardStoragePushRequestBuilderVGrain
    {
        #region Fields

        private readonly IDemocriteSerializer _democriteSerializer;
        private readonly IBlackboardProvider _blackboardProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardStorageRequestBuilderVGrain"/> class.
        /// </summary>
        public BlackboardStorageRequestBuilderVGrain(ILogger<IBlackboardStorageRequestBuilderVGrain> logger,
                                                     IBlackboardProvider blackboardProvider,
                                                     IDemocriteSerializer democriteSerializer)
            : base(logger)
        {
            this._democriteSerializer = democriteSerializer;
            this._blackboardProvider = blackboardProvider;
        }

        #endregion

        #region Nested

        private sealed class RequestBuilder<TInput>
        {
            public TInput? Data { get; set; }

            public Guid BoardUid { get; set; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        [ReadOnly]
        public async Task<IPullDataRecordRequest> GetPullTargetAsync<TContextInfo>(IExecutionContext<TContextInfo> context)
            where TContextInfo : IDataRecordPullRequestOption
        {
            return await GetPullTargetImpl(NoneType.Instance, context.Configuration, context);
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<IPullDataRecordRequest<TInput>> GetPullTargetFromInputAsync<TInput, TContextInfo>(TInput data,
                                                                                                            IExecutionContext<TContextInfo> context)
            where TContextInfo : IDataRecordPullRequestOption
        {
            return await GetPullTargetWithInputImpl(data, context.Configuration, context);
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<IPullDataRecordRequest<TInput>> GetPullTargetFromInputAsync<TInput>(TInput data, IExecutionContext context)
        {
            return await GetPullTargetWithInputImpl<TInput, IDataRecordPullRequestOption>(data, null, context);
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<IPullDataRecordRequest> GetPullTargetAsync(IExecutionContext context)
        {
            return await GetPullTargetImpl<NoneType, IDataRecordPullRequestOption>(NoneType.Instance, null, context);
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<IPushDataRecordRequest<TInput>> GetPushTargetFromInputAsync<TInput>(TInput data, IExecutionContext context)
        {
            return await GetPushTargetImpl(data, (IDataRecordPushRequestOption?)null, context);
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<IPushDataRecordRequest<TInput>> GetPushTargetFromInputAsync<TInput, TContextInfo>(TInput data, IExecutionContext<TContextInfo> context)
            where TContextInfo : IDataRecordPushRequestOption
        {
            return await GetPushTargetImpl(data, context.Configuration, context);
        }

        #region Tools

        /// <summary>
        /// Gets the push target implementation.
        /// </summary>
        private async Task<IPushDataRecordRequest<TInput>> GetPushTargetImpl<TInput, TContextInfo>(TInput data,
                                                                                                   TContextInfo? contextInfo,
                                                                                                   IExecutionContext context)
            where TContextInfo : IDataRecordPushRequestOption
        {
            var pullRequest = new RequestBuilder<TInput>
            {
                Data = data
            };

            var contextData = ExtractCacheContext(contextInfo, context);
            await PopulateBoardId(pullRequest, contextInfo, context, contextData);

            var push = (contextInfo?.ForceValueCreation ?? false) ? DataRecordPushRequestTypeEnum.OnlyNew : (contextInfo?.PushActionType ?? DataRecordPushRequestTypeEnum.Push);
            if (push == DataRecordPushRequestTypeEnum.None)
                push = DataRecordPushRequestTypeEnum.Push;

            var request = new PushDataRecordRequest<TInput>(pullRequest.BoardUid,
                                                            push,
                                                            (contextInfo?.ForceValueCreation ?? false) ? Guid.NewGuid() : (contextData?.DataRecordUid ?? Guid.NewGuid()),
                                                            contextInfo?.LogicalType,
                                                            contextInfo?.NewRecordStatus,
                                                            data);
            return request;
        }

        /// <summary>
        /// Gets the pull target implementation with input passing
        /// </summary>
        private async Task<IPullDataRecordRequest<TInput>> GetPullTargetWithInputImpl<TInput, TContextInfo>(TInput data,
                                                                                                            TContextInfo? contextInfo,
                                                                                                            IExecutionContext context)
            where TContextInfo : IDataRecordPullRequestOption
        {
            var result = await GetPullTargetImpl(data, contextInfo, context);
            return (IPullDataRecordRequest<TInput>)result;
        }

        /// <summary>
        /// Gets the pull target implementation.
        /// </summary>
        private async Task<IPullDataRecordRequest> GetPullTargetImpl<TInput, TContextInfo>(TInput data,
                                                                                           TContextInfo? contextInfo,
                                                                                           IExecutionContext context)
            where TContextInfo : IDataRecordPullRequestOption
        {
            var pullRequest = new RequestBuilder<TInput>
            {
                Data = data
            };

            var contextData = ExtractCacheContext(contextInfo, context);
            await PopulateBoardId(pullRequest, contextInfo, context, contextData);

            var logicPattern = contextInfo?.LogicalTypePattern;
            var filterStatus = contextInfo?.RecordStatusFilter;

            IPullDataRecordRequest request;

            if (NoneType.IsEqualTo<TInput>())
                request = new PullDataRecordRequest(pullRequest.BoardUid, null, logicPattern, filterStatus);
            else
                request = new PullDataRecordRequest<TInput>(pullRequest.BoardUid, null, logicPattern, filterStatus, data);

            return request;
        }

        /// <summary>
        /// Extracts the cache context.
        /// </summary>
        private DataRecordRequestDataContext? ExtractCacheContext<TContextInfo>(TContextInfo? contextInfo,
                                                                                IExecutionContext context)
            where TContextInfo : IDataRecordRequestOption
        {
            DataRecordRequestDataContext? reqContextCache = null;

            if (!(contextInfo?.DontExtractDataFromContext ?? false))
                reqContextCache = context.TryGetContextData<DataRecordRequestDataContext>(this._democriteSerializer);

            return reqContextCache;
        }

        /// <summary>
        /// Populates the board identifier DeferredId.
        /// </summary>
        private async Task PopulateBoardId<TInput, TContextInfo>(RequestBuilder<TInput> data,
                                                                 TContextInfo? contextInfo,
                                                                 IExecutionContext context,
                                                                 DataRecordRequestDataContext? reqContextCache)
            where TContextInfo : IDataRecordRequestOption
        {
            IBlackboardRef? blackboardRef = null;

            var boardId = contextInfo?.BoardId;
            var boardName = contextInfo?.BoardName;
            var boardTemplateName = contextInfo?.BoardTemplateName;

            if (boardId is null && reqContextCache is not null && reqContextCache.Value.BoardId != Guid.Empty)
                boardId = reqContextCache.Value.BoardId;

            if (boardId is not null && boardId.Value != Guid.Empty)
                blackboardRef = await this._blackboardProvider.GetBlackboardAsync(boardId.Value, context.CancellationToken);
            else if (!string.IsNullOrEmpty(boardName) && !string.IsNullOrEmpty(boardTemplateName))
                blackboardRef = await this._blackboardProvider.GetBlackboardAsync(boardName, boardTemplateName, context.CancellationToken);

            if (blackboardRef is null)
                throw new BlackboardMissingDemocriteException(boardId, boardName, boardTemplateName);

            data.BoardUid = blackboardRef.Uid;
        }
        #endregion

        #endregion
    }
}
