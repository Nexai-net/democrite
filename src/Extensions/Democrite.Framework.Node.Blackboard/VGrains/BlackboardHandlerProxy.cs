// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Proxy local class to <see cref="IBlackboardGrain"/>
    /// </summary>
    /// <seealso cref="IBlackboardRef" />
    internal sealed class BlackboardHandlerProxy : IBlackboardRef
    {
        #region Fields

        private static readonly MethodInfo s_getStoreDataGeneric;

        private readonly ILogger<BlackboardHandlerProxy> _logger;
        private readonly ITimeManager _timeManager;
        private readonly IBlackboardGrain _boardGrain;
        private readonly BlackboardId _boardId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="BlackboardHandlerProxy"/> class.
        /// </summary>
        static BlackboardHandlerProxy()
        {
            Expression<Func<BlackboardHandlerProxy, Guid, Task<IReadOnlyCollection<DataRecordContainer<int>>>>> getStoreDataGeneric = (b, g) => b.GetStoredDataAsync<int>(null, default, g);
            s_getStoreDataGeneric = ((MethodCallExpression)getStoreDataGeneric.Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardHandlerProxy"/> class.
        /// </summary>
        public BlackboardHandlerProxy(IGrainOrleanFactory grainFactory,
                                      ILogger<BlackboardHandlerProxy> logger,
                                      GrainId blackboardGrainId,
                                      ITimeManager timeManager,
                                      BlackboardId boardId)
        {
            this._logger = logger;
            this._timeManager = timeManager;
            this._boardId = boardId;

            // Normally it's recommanded to prevent calling external method in CTOR
            // Here we want to ensure the readonly preformance boost and ensure board grain is set before any usage
            this._boardGrain = grainFactory.GetGrain<IBlackboardGrain>(blackboardGrainId);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Guid Uid
        {
            get { return this._boardId.Uid; }
        }

        /// <inheritdoc />
        public string Name
        {
            get { return this._boardId.BoardName; }
        }

        /// <inheritdoc />
        public string TemplateName
        {
            get { return this._boardId.BoardTemplateKey; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> InitializeAsync(CancellationToken token, IReadOnlyCollection<DataRecordContainer>? initData = null, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.InitializeAsync(grainToken.Token, initData, callContextId);
            }
        }

        /// <inheritdoc />
        public Task<bool> InitializeAsync(CancellationToken token, Guid? callContextId = null, params DataRecordContainer[] initData)
        {
            return InitializeAsync(token, (IReadOnlyCollection<DataRecordContainer>)(initData ?? EnumerableHelper<DataRecordContainer>.ReadOnlyArray), callContextId);
        }

        /// <inheritdoc />
        public async Task<bool> SealedAsync(CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.SealedAsync(grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<BlackboardLifeStatusEnum> GetStatusAsync(CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetStatusAsync(grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                await this._boardGrain.ChangeRecordDataStatusAsync(uid, recordStatus, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDataAsync(CancellationToken token, Guid? callContextId, IIdentityCard identity, params Guid[] slotIds)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.DeleteDataAsync(grainToken.Token, callContextId, identity, slotIds);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter,
                                                                                                                                     CancellationToken token,
                                                                                                                                     uint? limit = null,
                                                                                                                                     Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredDataFilteredAsync<TDataProjection>(logicTypeFilter, null, null, limit, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter,
                                                                                                                                     string? displayNameFilter,
                                                                                                                                     CancellationToken token,
                                                                                                                                     uint? limit = null,
                                                                                                                                     Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredDataFilteredAsync<TDataProjection>(logicTypeFilter, displayNameFilter, null, limit: limit, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter,
                                                                                                                                     string? displayNameFilter,
                                                                                                                                     RecordStatusEnum? statusFilter,
                                                                                                                                     uint? limit,
                                                                                                                                     CancellationToken token,
                                                                                                                                     Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredDataFilteredAsync<TDataProjection>(logicTypeFilter, displayNameFilter, statusFilter, limit, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataAsync(grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter,
                                                                                                          CancellationToken token,
                                                                                                          uint? limit = null,
                                                                                                          Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataFilteredAsync(logicTypeFilter, null, null, limit, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter,
                                                                                                          string? displayNameFilter,
                                                                                                          CancellationToken token,
                                                                                                          uint? limit = null,
                                                                                                          Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataFilteredAsync(logicTypeFilter, displayNameFilter, null, limit: limit, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter,
                                                                                                          string? displayNameFilter,
                                                                                                          RecordStatusEnum? statusFilter,
                                                                                                          uint? limit,
                                                                                                          CancellationToken token,
                                                                                                          Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataFilteredAsync(logicTypeFilter,
                                                                                displayNameFilter,
                                                                                statusFilter,
                                                                                limit: limit,
                                                                                grainToken.Token,
                                                                                callContextId);
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, CancellationToken token, Guid? callContextId = null)
        {
            return GetAllStoredMetaDataFilteredAsync(filter, null, token, callContextId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, uint? limit, CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataFilteredAsync(filter, limit, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(ConditionExpressionDefinition filter,
                                                                                                                                     uint? limit,
                                                                                                                                     CancellationToken token,
                                                                                                                                     Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredDataFilteredAsync<TDataProjection>(filter, limit: limit, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(Expression<Func<BlackboardRecordMetadata, bool>> filter,
                                                                                                                               CancellationToken token,
                                                                                                                               uint? limit = null,
                                                                                                                               Guid? callContextId = null)
        {
            return GetAllStoredDataFilteredAsync<TDataProjection>(filter.Serialize(), limit, token, callContextId);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(Expression<Func<BlackboardRecordMetadata, bool>> filter, CancellationToken token, uint? limit = null, Guid? callContextId = null)
        {
            return GetAllStoredMetaDataFilteredAsync(filter.Serialize(), limit, token, callContextId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetStoredMetaDataAsync(IReadOnlyCollection<Guid> dataUids, CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetStoredMetaDataAsync(dataUids, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetStoredMetaDataAsync(Guid? callContextId, CancellationToken token, params Guid[] dataUids)
        {
            return GetStoredMetaDataAsync(dataUids, token, callContextId);
        }

        /// <inheritdoc />
        public async Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, CancellationToken token, Guid? callContextId = null)
        {
            return (await GetStoredDataAsync<TDataProjection>(callContextId, token, dataUid))?.SingleOrDefault();
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(Guid? callContextId, CancellationToken token, params Guid[] dataUids)
        {
            return GetStoredDataAsync<TDataProjection>(dataUids, token);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer>> GetStoredDataAsync(ConcretBaseType dataProjectionType, Guid? callContextId, CancellationToken token, params Guid[] dataUids)
        {
            var speMthd = s_getStoreDataGeneric.MakeGenericMethodWithCache(dataProjectionType.ToType());

            var resultTask = (Task)speMthd.Invoke(this, new object?[] { callContextId, token, dataUids })!;

            await resultTask;

            var resultData = (resultTask.GetResult() as IEnumerable)
                                    ?.OfType<DataRecordContainer>()
                                    .ToArray() ?? EnumerableHelper<DataRecordContainer>.ReadOnlyArray;

            return resultData;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(IReadOnlyCollection<Guid> dataUids, CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetStoredDataAsync<TDataProjection>(dataUids, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.PrepareDataSlotAsync(uid, logicType, displayName, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record,
                                                     DataRecordPushRequestTypeEnum pushType,
                                                     CancellationToken token,
                                                     Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.PushDataAsync(record, pushType, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PushDataAsync<TData>(TData? record,
                                                     Guid uid,
                                                     [NotNull] string logicType,
                                                     string? displayName = null,
                                                     RecordStatusEnum recordStatus = RecordStatusEnum.Ready,
                                                     DataRecordPushRequestTypeEnum pushType = DataRecordPushRequestTypeEnum.Push,
                                                     RecordMetadata? customMetadata = null,
                                                     CancellationToken token = default,
                                                     Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                var now = this._timeManager.UtcNow;
                return await this._boardGrain.PushDataAsync(new DataRecordContainer<TData?>(logicType,
                                                                                            uid,
                                                                                            (string.IsNullOrEmpty(displayName)
                                                                                                ? (record is ISupportDebugDisplayName support)
                                                                                                        ? support.ToDebugDisplayName()
                                                                                                        : typeof(TData).Name + ":" + uid
                                                                                                : displayName),
                                                                                            record,
                                                                                            recordStatus,
                                                                                            now,
                                                                                            string.Empty,
                                                                                            now,
                                                                                            string.Empty,
                                                                                            customMetadata),
                                                            pushType,
                                                            grainToken.Token,
                                                            callContextId);
            }
        }

        /// <inheritdoc />
        public Task<bool> PushNewDataAsync<TData>(TData? record,
                                                  [NotNull] string logicType,
                                                  string? displayName = null,
                                                  RecordStatusEnum recordStatus = RecordStatusEnum.Ready,
                                                  DataRecordPushRequestTypeEnum pushType = DataRecordPushRequestTypeEnum.Push,
                                                  RecordMetadata? customMetadata = null,
                                                  CancellationToken token = default,
                                                  Guid? callContextId = null)
        {
            return PushDataAsync(record, Guid.NewGuid(), logicType, displayName, recordStatus, pushType, customMetadata, token, callContextId);
        }

        /// <inheritdoc />
        public async Task<BlackboardQueryResponse> QueryAsync<TResponse>(BlackboardQueryRequest<TResponse> request, CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.QueryAsync<TResponse>(request, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task<BlackboardQueryResponse?> QueryAsync(BlackboardQueryCommand command, CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.QueryAsync(command, grainToken.Token, callContextId);
            }
        }

        /// <inheritdoc />
        public async Task FireQueryAsync(BlackboardQueryCommand command, CancellationToken token, Guid? callContextId = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                await this._boardGrain.FireQueryAsync(command, grainToken.Token, callContextId);
            }
        }

        #endregion
    }
}
