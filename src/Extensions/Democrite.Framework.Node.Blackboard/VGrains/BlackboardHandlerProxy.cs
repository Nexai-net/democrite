// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Node.Abstractions.Services;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using Elvex.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
        public async Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                await this._boardGrain.ChangeRecordDataStatusAsync(uid, recordStatus, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByTypeAsync<TDataProjection>(string? logicTypeFilter, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.GetAllStoredDataByTypeAsync<TDataProjection>(logicTypeFilter, null, null, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByTypeAsync<TDataProjection>(string? logicTypeFilter,
                                                                                                                                   string? displayNameFilter,
                                                                                                                                   CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.GetAllStoredDataByTypeAsync<TDataProjection>(logicTypeFilter, displayNameFilter, null, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByTypeAsync<TDataProjection>(string? logicTypeFilter,
                                                                                                                                   string? displayNameFilter,
                                                                                                                                   RecordStatusEnum? statusFilter,
                                                                                                                                   CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.GetAllStoredDataByTypeAsync<TDataProjection>(logicTypeFilter, displayNameFilter, statusFilter, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataAsync(grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataByTypeAsync(string? logicTypeFilter,
                                                                                                        CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataByTypeAsync(logicTypeFilter, null, null, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataByTypeAsync(string logicTypeFilter,
                                                                                                        string displayNameFilter,
                                                                                                        CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataByTypeAsync(logicTypeFilter, displayNameFilter, null, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataByTypeAsync(string? logicTypeFilter,
                                                                                                        string? displayNameFilter,
                                                                                                        RecordStatusEnum? statusFilter,
                                                                                                        CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.GetAllStoredMetaDataByTypeAsync(logicTypeFilter, displayNameFilter, statusFilter, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.GetStoredDataAsync<TDataProjection>(dataUid, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.PrepareDataSlotAsync(uid, logicType, displayName, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record,
                                                     DataRecordPushRequestTypeEnum pushType,
                                                     CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                return await this._boardGrain.PushDataAsync(record, pushType, grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PushDataAsync<TData>(TData? record,
                                                     Guid uid,
                                                     string logicType,
                                                     string displayName,
                                                     RecordStatusEnum recordStatus,
                                                     DataRecordPushRequestTypeEnum pushType,
                                                     CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                var now = this._timeManager.UtcNow;
                return await this._boardGrain.PushDataAsync(new DataRecordContainer<TData?>(logicType,
                                                                                            uid,
                                                                                            displayName,
                                                                                            record,
                                                                                            recordStatus,
                                                                                            now,
                                                                                            string.Empty,
                                                                                            now,
                                                                                            string.Empty),
                                                            pushType,
                                                            grainToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PushNewDataAsync<TData>(TData? record, [NotNull] string logicType, string displayName, RecordStatusEnum? recordStatus, DataRecordPushRequestTypeEnum pushType, CancellationToken token)
        {
            using (var grainToken = GrainCancellationTokenExtensions.ToGrainCancellationToken(token))
            {
                var now = this._timeManager.UtcNow;
                return await this._boardGrain.PushDataAsync(new DataRecordContainer<TData?>(logicType,
                                                                                            Guid.Empty,
                                                                                            displayName,
                                                                                            record,
                                                                                            recordStatus ?? RecordStatusEnum.Ready,
                                                                                            now,
                                                                                            string.Empty,
                                                                                            now,
                                                                                            string.Empty),
                                                            pushType,
                                                            grainToken.Token);
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
