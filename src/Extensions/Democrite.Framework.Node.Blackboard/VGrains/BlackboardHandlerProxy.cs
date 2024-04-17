﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Abstractions.Services;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Serialization.Invocation;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Proxy local class to <see cref="IBlackboardGrain"/>
    /// </summary>
    /// <seealso cref="IBlackboardRef" />
    internal sealed class BlackboardHandlerProxy : IBlackboardRef
    {
        #region Fields

        private readonly ILogger<BlackboardHandlerProxy> _logger;
        private readonly IGrainFactory _grainFactory;
        private readonly GrainId _blackboardGrainId;
        private readonly ITimeManager _timeManager;

        private IBlackboardGrain _boardGrain;
        private BlackboardId _boardId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardHandlerProxy"/> class.
        /// </summary>
        public BlackboardHandlerProxy(IGrainOrleanFactory grainFactory,
                                      ILogger<BlackboardHandlerProxy> logger,
                                      GrainId blackboardGrainId,
                                      ITimeManager timeManager)
        {
            this._grainFactory = grainFactory;
            this._blackboardGrainId = blackboardGrainId;
            this._logger = logger;
            this._timeManager = timeManager;

            // Flag the field as not null event it is initialized later on
            // this field MUST not be null is other method other than InitializeAsync
            this._boardGrain = null!;
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
        public async Task<bool> InitializeAsync(CancellationToken token, IReadOnlyCollection<DataRecordContainer>? initData = null)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.InitializeAsync(grainToken.Token, initData);
            }
        }

        /// <inheritdoc />
        public Task<bool> InitializeAsync(CancellationToken token, params DataRecordContainer[] initData)
        {
            return InitializeAsync(token, (IReadOnlyCollection<DataRecordContainer>)(initData ?? EnumerableHelper<DataRecordContainer>.ReadOnlyArray));
        }

        /// <inheritdoc />
        public async Task<bool> SealedAsync(CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.SealedAsync(grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<BlackboardLifeStatusEnum> GetStatusAsync(CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetStatusAsync(grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                await this._boardGrain.ChangeRecordDataStatusAsync(uid, recordStatus, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDataAsync(CancellationToken token, IIdentityCard identity, params Guid[] slotIds)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.DeleteDataAsync(grainToken.Token, identity, slotIds);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredDataFilteredAsync<TDataProjection>(logicTypeFilter, null, null, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter,
                                                                                                                                   string? displayNameFilter,
                                                                                                                                   CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredDataFilteredAsync<TDataProjection>(logicTypeFilter, displayNameFilter, null, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter,
                                                                                                                                   string? displayNameFilter,
                                                                                                                                   RecordStatusEnum? statusFilter,
                                                                                                                                   CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredDataFilteredAsync<TDataProjection>(logicTypeFilter, displayNameFilter, statusFilter, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataAsync(grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter,
                                                                                                        CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataFilteredAsync(logicTypeFilter, null, null, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter,
                                                                                                          string? displayNameFilter,
                                                                                                          CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataFilteredAsync(logicTypeFilter, displayNameFilter, null, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter,
                                                                                                        string? displayNameFilter,
                                                                                                        RecordStatusEnum? statusFilter,
                                                                                                        CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataFilteredAsync(logicTypeFilter, displayNameFilter, statusFilter, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataFilteredAsync(filter, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(ConditionExpressionDefinition filter, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetAllStoredDataFilteredAsync<TDataProjection>(filter, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(Expression<Func<BlackboardRecordMetadata, bool>> filter, CancellationToken token)
        {
            return GetAllStoredDataFilteredAsync<TDataProjection>(filter.Serialize(), token);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(Expression<Func<BlackboardRecordMetadata, bool>> filter, CancellationToken token)
        {
            return GetAllStoredMetaDataFilteredAsync(filter.Serialize(), token);
        }

        /// <inheritdoc />
        public async Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, CancellationToken token)
        {
            return (await GetStoredDataAsync<TDataProjection>(token, dataUid))?.SingleOrDefault();
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(CancellationToken token, params Guid[] dataUids)
        {
            return GetStoredDataAsync<TDataProjection>(dataUids, token);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(IReadOnlyCollection<Guid> dataUids, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.GetStoredDataAsync<TDataProjection>(dataUids, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.PrepareDataSlotAsync(uid, logicType, displayName, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record,
                                                     DataRecordPushRequestTypeEnum pushType,
                                                     CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.PushDataAsync(record, pushType, grainToken.Token);
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
                                                     CancellationToken token = default)
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
                                                            grainToken.Token);
            }
        }

        /// <inheritdoc />
        public Task<bool> PushNewDataAsync<TData>(TData? record,
                                                  [NotNull] string logicType,
                                                  string? displayName = null,
                                                  RecordStatusEnum recordStatus = RecordStatusEnum.Ready,
                                                  DataRecordPushRequestTypeEnum pushType = DataRecordPushRequestTypeEnum.Push,
                                                  RecordMetadata? customMetadata = null,
                                                  CancellationToken token = default)
        {
            return PushDataAsync(record, Guid.NewGuid(), logicType, displayName, recordStatus, pushType, customMetadata);
        }

        /// <inheritdoc />
        public async Task<BlackboardQueryResponse> QueryAsync<TResponse>(BlackboardQueryRequest<TResponse> request, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.QueryAsync<TResponse>(request, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<BlackboardQueryResponse> QueryAsync(BlackboardQueryCommand command, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationTokenSource(token))
            {
                return await this._boardGrain.QueryAsync(command, grainToken.Token);
            }
        }

        /// <inheritdoc />
        internal async Task InitializeAsync(CancellationToken _)
        {
            if (this._boardGrain is null)
            {
                this._boardGrain = this._grainFactory.GetGrain<IBlackboardGrain>(this._blackboardGrainId);
                this._boardId = await this._boardGrain.GetIdentityAsync();
            }
        }

        #endregion
    }
}
