// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;

    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.Logging;

    using Orleans.Placement;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Vgrain in charge to search in a blackboard for request and/or 
    /// </summary>
    /// <seealso cref="VGrainBase{IBlackboardStorageVGrain}" />
    /// <seealso cref="IBlackboardStorageVGrain" />
    [PreferLocalPlacement]
    internal sealed class BlackboardStorageVGrain : VGrainBase<IBlackboardStorageVGrain>, IBlackboardStorageVGrain
    {
        #region Fields

        private readonly Dictionary<Guid, IBlackboardRef> _blackboardAccessCache;

        private readonly IBlackboardProvider _blackboardProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardStorageVGrain"/> class.
        /// </summary>
        public BlackboardStorageVGrain(ILogger<IBlackboardStorageVGrain> logger,
                                       IBlackboardProvider blackboardProvider)
            : base(logger)
        {
            this._blackboardAccessCache = new Dictionary<Guid, IBlackboardRef>();

            this._blackboardProvider = blackboardProvider;
        }

        #endregion

        #region Properties
        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TData?>> PullDataAsync<TData>(IPullDataRecordRequest dataTargetInfo, IExecutionContext executionContext)
        {
            var dataContained = await PullDataContainersAsync<TData>(dataTargetInfo, executionContext);
            return dataContained.GetDatas();
        }

        /// <inheritdoc />
        public async Task<TData?> PullFirstDataAsync<TData>(IPullDataRecordRequest request, IExecutionContext executionContext)
        {
            var dataContained = await PullDataContainersAsync<TData>(request, executionContext);
            return dataContained.GetDatas().FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<DataRecordContainer<TData?>?> PullFirstDataContainerAsync<TData>(IPullDataRecordRequest request, IExecutionContext executionContext)
        {
            var dataContained = await PullDataContainersAsync<TData>(request, executionContext);
            return dataContained.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<DataRecordCollectionContainer<TData?>> PullDataContainersAsync<TData>(IPullDataRecordRequest request, IExecutionContext executionContext)
        {
            var board = await GetBoardAsync(request, executionContext.CancellationToken);

            var allData = EnumerableHelper<DataRecordContainer<TData?>>.ReadOnly;

            if (request.DataTargetUid is not null && request.DataTargetUid.Value != Guid.Empty)
            {
                var result = await board.GetStoredDataAsync<TData>(request.DataTargetUid.Value, executionContext.CancellationToken);
                if (result is not null)
                    allData = result.AsEnumerable().ToArray();
            }
            else
            {
                var results = await board.GetAllStoredDataFilteredAsync<TData>(request.LogicalRecordTypePattern, null, request.RecordStatusFilter, limit: null, executionContext.CancellationToken);
                if (results is not null && results.Any())
                    allData = results;
            }

            return new DataRecordCollectionContainer<TData?>(allData);
        }

        /// <inheritdoc />
        public async Task<bool> PushDataAsync<TData>(IPushDataRecordRequest<TData?> request, IExecutionContext ctx)
        {
            var board = await GetBoardAsync(request, ctx.CancellationToken);

            var result = await board.PushDataAsync(request.Data,
                                                   request.TargetUid ?? Guid.NewGuid(),
                                                   request.LogicalRecordType ?? string.Empty,
                                                   ((request.Data is ISupportDebugDisplayName debugDisplayName) ? debugDisplayName.ToDebugDisplayName() : request.Data?.ToString()) ?? string.Empty,
                                                   request.NewRecordStatus ?? RecordStatusEnum.Ready,
                                                   request?.PushActionType ?? DataRecordPushRequestTypeEnum.Push,
                                                   null,
                                                   ctx.CancellationToken);

            return result;
        }

        #region Tools

        /// <summary>
        /// Gets the board based on request information
        /// </summary>
        private async ValueTask<IBlackboardRef> GetBoardAsync(IDataRecordRequest dataTargetInfo, CancellationToken token)
        {
            // This block is not for not thread safe but grain system is single thread handled is should be safe otherwise a RWLock will be needed
            if (this._blackboardAccessCache.TryGetValue(dataTargetInfo.BoardUid, out var board))
                return board;

            var newBoard = await this._blackboardProvider.GetBlackboardAsync(dataTargetInfo.BoardUid, token);
            this._blackboardAccessCache.Add(dataTargetInfo.BoardUid, newBoard);
            return newBoard;
        }

        #endregion

        #endregion

    }
}
