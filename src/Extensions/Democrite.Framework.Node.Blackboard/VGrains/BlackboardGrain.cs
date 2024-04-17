// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Events;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;
    using Democrite.Framework.Node.Blackboard.Models;
    using Democrite.Framework.Node.Blackboard.Models.Surrogates;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.Services;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using Orleans;
    using Orleans.Runtime;

    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;

    using static Democrite.Framework.Node.Blackboard.Models.BlackboardLogicalTypeHandler;

    /// <summary>
    /// Blackboard grain 
    /// </summary>
    /// <seealso cref="IVGrain" />
    /// <seealso cref="IGrainWithGuidKey" />
    [DemocriteSystemVGrain]
    internal sealed class BlackboardGrain : VGrainBase<BlackboardGrainState, BlackboardGrainStateSurrogate, BlackboardGrainStateConverter, IBlackboardGrain>, IBlackboardGrain
    {
        #region Fields

        private static readonly IReadOnlyDictionary<BlackboardCommandTypeEnum, Func<BlackboardGrain, BlackboardCommand, CommandExecutionContext, Task<bool>>> s_rootCommandExecutor;

        private static readonly IReadOnlyDictionary<BlackboardCommandStorageActionTypeEnum, Func<BlackboardGrain, BlackboardCommandStorage, CommandExecutionContext, Task<bool>>> s_rootCommandStorageExecutor;
        private static readonly IReadOnlyDictionary<BlackboardCommandTriggerActionTypeEnum, Func<BlackboardGrain, BlackboardCommandTrigger, CommandExecutionContext, Task<bool>>> s_rootCommandTriggerExecutor;
        private static readonly IReadOnlyDictionary<BlackboardLifeStatusEnum, Func<BlackboardGrain, BlackboardCommandLifeStatusChange, CommandExecutionContext, Task<bool>>> s_lifeStatusCommandTriggerExecutor;

        private static readonly MethodInfo s_cmdTriggerSequence;
        private static readonly MethodInfo s_addCommandToStorage;
        private static readonly MethodInfo s_solveGenericQueryRequest;
        private static readonly MethodInfo s_cmdSignalWithData;

        private static readonly IConcretTypeSurrogate s_defaultControllerConcretTypeSurrogate;
        private static readonly ConcretType s_defaultControllerConcretType;
        private static readonly Type s_defaultController;

        private readonly IBlackboardDataLogicalTypeRuleValidatorProvider _blackboardDataLogicalTypeRuleValidatorProvider;
        private readonly Dictionary<BlackboardControllerTypeEnum, BlackboardControllerHandler> _controllers;
        private readonly IDemocriteExecutionHandler _democriteExecutionHandler;
        private readonly IVGrainDemocriteSystemProvider _systemVGrainProvider;
        private readonly IDemocriteSerializer _democriteSerializer;
        private readonly IGrainOrleanFactory _grainOrleanFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _saveStateLocker;
        private readonly ISignalService _signalService;
        private readonly IObjectConverter _converter;
        private readonly ITimeManager _timeManager;

        private readonly CancellationTokenSource _lifetimeToken;
        private readonly DelayTimer _delaySaving;

        // Extra protection to ensure metadata are always synchronized with data stored
        private readonly SemaphoreSlim _metaDataLocker;
        private readonly SortedSet<BlackboardLogicalTypeHandler> _logicalHandlers;

        private IDeferredHandlerVGrain? _deferredHandler;

        private TaskScheduler? _activationScheduler;
        private int _saveCounter;
        private bool _initializing;
        private bool _sealing;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainBase{TVGrainState, TStateSurrogate, TConverter}" /> class.
        /// </summary>
        static BlackboardGrain()
        {
            s_defaultController = typeof(IDefaultBlackboardControllerGrain);
            s_defaultControllerConcretType = (ConcretType)s_defaultController.GetAbstractType();
            s_defaultControllerConcretTypeSurrogate = ConcretBaseTypeConverter.ConvertToSurrogate(s_defaultControllerConcretType);

            Expression<Func<BlackboardGrain, BlackboardCommandStorageAddRecord<int>, CommandExecutionContext, Task<bool>>> cmdAddToStorage = (g, cmd, ctx) => g.Command_AddToStorageAsync<int>(cmd, ctx);
            s_addCommandToStorage = ((MethodCallExpression)cmdAddToStorage.Body).Method.GetGenericMethodDefinition();

            Expression<Func<BlackboardGrain, BlackboardCommandTriggerSequence<int>, CommandExecutionContext, Task<bool>>> cmdtriggerSequence = (g, cmd, ctx) => g.Command_TriggerSequence<int>(cmd, ctx);
            s_cmdTriggerSequence = ((MethodCallExpression)cmdtriggerSequence.Body).Method.GetGenericMethodDefinition();

            Expression<Func<BlackboardGrain, BlackboardCommandTriggerSignal<int>, CommandExecutionContext, Task<bool>>> cmdSignalWithData = (g, cmd, ctx) => g.Command_TriggerSignalWithData<int>(cmd, ctx);
            s_cmdSignalWithData = ((MethodCallExpression)cmdSignalWithData.Body).Method.GetGenericMethodDefinition();

            s_rootCommandExecutor = new Dictionary<BlackboardCommandTypeEnum, Func<BlackboardGrain, BlackboardCommand, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardCommandTypeEnum.Storage, (g, cmd, ctx) => CommandExecuteAsync(s_rootCommandStorageExecutor!, c => c.StorageAction, g, (BlackboardCommandStorage)cmd, ctx) },
                { BlackboardCommandTypeEnum.Trigger, (g, cmd, ctx) => CommandExecuteAsync(s_rootCommandTriggerExecutor!, c => c.TriggerActionType, g, (BlackboardCommandTrigger)cmd, ctx) },
                { BlackboardCommandTypeEnum.LifeStatusChange, (g, cmd, ctx) => CommandExecuteAsync(s_lifeStatusCommandTriggerExecutor!, c => c.NewStatus, g, (BlackboardCommandLifeStatusChange)cmd, ctx) },

                { BlackboardCommandTypeEnum.Reject, CommandExecuteRejectAsync },
                { BlackboardCommandTypeEnum.RetryDeferred, (g, cmd, ctx) => g.CommandExecuteRetryDeferredAsync(cmd, ctx) },

            }.ToFrozenDictionary();

            s_rootCommandStorageExecutor = new Dictionary<BlackboardCommandStorageActionTypeEnum, Func<BlackboardGrain, BlackboardCommandStorage, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardCommandStorageActionTypeEnum.Add, (g, cmd, ctx) => CommandGenericCall(g, cmd, ctx, s_addCommandToStorage) },
                { BlackboardCommandStorageActionTypeEnum.Remove, (g, cmd, ctx) => g.Command_RemoveFromStorageAsync(cmd, ctx) },
                { BlackboardCommandStorageActionTypeEnum.Decommission, (g, cmd, ctx) => g.Command_DecommissionFromStorageAsync(cmd, ctx) },
                { BlackboardCommandStorageActionTypeEnum.ChangeStatus, (g, cmd, ctx) => g.Command_ChangeStatusFromStorageAsync(cmd, ctx) },

            }.ToFrozenDictionary();

            s_rootCommandTriggerExecutor = new Dictionary<BlackboardCommandTriggerActionTypeEnum, Func<BlackboardGrain, BlackboardCommandTrigger, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardCommandTriggerActionTypeEnum.Sequence, (g, cmd, ctx) => CommandGenericCall(g, cmd, ctx, s_cmdTriggerSequence) },
                { BlackboardCommandTriggerActionTypeEnum.Signal, (g, cmd, ctx) => g.Command_TriggerSignal((BlackboardCommandTriggerSignal)cmd, ctx) }
            }.ToFrozenDictionary();

            s_lifeStatusCommandTriggerExecutor = new Dictionary<BlackboardLifeStatusEnum, Func<BlackboardGrain, BlackboardCommandLifeStatusChange, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardLifeStatusEnum.Running, (g, cmd, ctx) => g.Command_LifeStatus_Initialize((BlackboardCommandLifeInitializeChange)cmd, ctx) },
                { BlackboardLifeStatusEnum.Sealed, (g, cmd, ctx) => g.Command_LifeStatus_Sealed(cmd, ctx) },
                { BlackboardLifeStatusEnum.Done, (g, cmd, ctx) => g.Command_LifeStatus_Changed(cmd, ctx) },
            }.ToFrozenDictionary();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

            Expression<Func<BlackboardGrain, Task<BlackboardQueryResponse>>> solveQueryExpr = (BlackboardGrain grain) => grain.SolveQueryRequestAsync<int>(null, null, null);
            s_solveGenericQueryRequest = ((System.Linq.Expressions.MethodCallExpression)solveQueryExpr.Body).Method.GetGenericMethodDefinition();

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardGrain"/> class.
        /// </summary>
        public BlackboardGrain(ILogger<IBlackboardGrain> logger,
                               [PersistentState(BlackboardConstants.BlackboardStorageStateKey, BlackboardConstants.BlackboardStorageConfigurationKey)] IPersistentState<BlackboardGrainStateSurrogate> persistentState,
                               IGrainOrleanFactory grainFactory,
                               IServiceProvider serviceProvider,
                               ITimeManager timeManager,
                               IRepositoryFactory repositoryFactory,
                               IBlackboardDataLogicalTypeRuleValidatorProvider blackboardDataLogicalTypeRuleValidatorProvider,
                               IDemocriteExecutionHandler democriteExecutionHandler,
                               IObjectConverter converter,
                               IVGrainDemocriteSystemProvider systemVGrainProvider,
                               IDemocriteSerializer democriteSerializer,
                               ISignalService signalService)

            : base(logger, persistentState)
        {
            this._saveStateLocker = new SemaphoreSlim(1);

            this._signalService = signalService;
            this._democriteSerializer = democriteSerializer;
            this._systemVGrainProvider = systemVGrainProvider;
            this._democriteExecutionHandler = democriteExecutionHandler;
            this._lifetimeToken = new CancellationTokenSource();
            this._controllers = new Dictionary<BlackboardControllerTypeEnum, BlackboardControllerHandler>();

            this._delaySaving = DelayTimer.Create(DelayPushStateAsync, lifeTimeToken: this._lifetimeToken.Token, startDelay: TimeSpan.FromSeconds(2));
            this._metaDataLocker = new SemaphoreSlim(1);

            this._repositoryFactory = repositoryFactory;
            this._grainOrleanFactory = grainFactory;
            this._serviceProvider = serviceProvider;
            this._timeManager = timeManager;
            this._blackboardDataLogicalTypeRuleValidatorProvider = blackboardDataLogicalTypeRuleValidatorProvider;

            this._converter = converter;

            this._logicalHandlers = new SortedSet<BlackboardLogicalTypeHandler>(BlackboardLogicalTypeHandlerComparer.Default);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> SealedAsync(GrainCancellationToken token)
        {
            CheckInitializationRequired();

            if (this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.Sealed)
                return false;

            return await CommandExecutionStartPointAsync(token.CancellationToken, new BlackboardCommandLifeStatusChange(BlackboardLifeStatusEnum.Sealed, this.State!.CurrentLifeStatus));

            //this.State!.CurrentLifeStatus = BlackboardLifeStatusEnum.Sealed;
            //await PushStateAsync(token.CancellationToken);

            //this._sealing = true;

            //try
            //{
            //    IReadOnlyCollection<BlackboardRecordMetadata> metaDatas;
            //    using (this._metaDataLocker.Lock())
            //    {
            //        metaDatas = this.State!.Registry.RecordMetadatas.Values.ToArray();
            //    }

            //    var logicalTypeToRemain = this._logicalHandlers.Where(h => h.RemainOnSealed)
            //                                                   .ToArray();

            //    var toKeepOnSealed = metaDatas.Where(m => m.Status == RecordStatusEnum.Ready && logicalTypeToRemain.Any(h => h.Match(m.LogicalType)))
            //                                  .ToArray();

            //    var toRemove = metaDatas.Except(toKeepOnSealed)
            //                            .ToArray();

            //    var stateController = await GetControllerAsync<IBlackboardStateControllerGrain>(BlackboardControllerTypeEnum.State, token.CancellationToken);

            //    BlackboardCommand[]? sealingCommands = null;

            //    if (stateController is not null)
            //    {
            //        sealingCommands = (await stateController.OnSealed(toKeepOnSealed, toRemove, token))?.ToArray();
            //    }
            //    else
            //    {
            //        sealingCommands = toRemove.Select(t => new BlackboardCommandStorageRemoveRecord(t.Uid)).ToArray();
            //    }

            //    if (sealingCommands is not null)
            //        await this.CommandExecutionStartPointAsync(token.CancellationToken, sealingCommands);

            //    return true;
            //}
            //finally
            //{
            //    this._sealing = false;
            //}
        }

        /// <inheritdoc />
        public async Task<bool> InitializeAsync(GrainCancellationToken token, IReadOnlyCollection<DataRecordContainer>? initData = null)
        {
            if (this.State!.CurrentLifeStatus != BlackboardLifeStatusEnum.WaitingInitialization)
                return false;

            return await CommandExecutionStartPointAsync(token.CancellationToken, new BlackboardCommandLifeInitializeChange(BlackboardLifeStatusEnum.WaitingInitialization, initData));

            //this._initializing = true;
            //try
            //{
            //    //this.State.TemplateCopy.ConfigurationDefinition.InitializationRequired

            //    BlackboardCommand[]? initCommands = null;

            //    var stateController = await GetControllerAsync<IBlackboardStateControllerGrain>(BlackboardControllerTypeEnum.State, token.CancellationToken);

            //    if (stateController is null)
            //    {
            //        initCommands = initData?.Select(d => d.CreateAddCommand(true, true))
            //                                .ToArray();
            //    }
            //    else
            //    {
            //        initCommands = (await stateController.OnInitialize(initData ?? EnumerableHelper<DataRecordContainer>.ReadOnly, token))?.ToArray();
            //    }

            //    if (initCommands is not null)
            //        await this.CommandExecutionStartPointAsync(token.CancellationToken, initCommands);

            //    this.State.CurrentLifeStatus = BlackboardLifeStatusEnum.Running;
            //    await PushStateAsync(default);
            //}
            //finally
            //{
            //    this._initializing = false;
            //}
            //return true;
        }

        /// <inheritdoc />
        public Task<BlackboardLifeStatusEnum> GetStatusAsync(GrainCancellationToken token)
        {
            return Task.FromResult(this.State!.CurrentLifeStatus);
        }

        /// <inheritdoc />
        public async Task BuildFromTemplateAsync(Guid blackboardTemplateUid, BlackboardId blackboardId)
        {
            if (this.State!.IsBuild == false)
            {
                var templateProvider = this._serviceProvider.GetRequiredService<IBlackboardTemplateDefinitionProvider>();
                var tmpl = await templateProvider.GetFirstValueByIdAsync(blackboardTemplateUid, default);

#pragma warning disable IDE0270 // Use coalesce expression
                if (tmpl is null)
                    throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), blackboardTemplateUid.ToString());
#pragma warning restore IDE0270 // Use coalesce expression

                this.State.CurrentLifeStatus = (tmpl.ConfigurationDefinition?.InitializationRequired == true ? BlackboardLifeStatusEnum.WaitingInitialization : BlackboardLifeStatusEnum.Running);
                this.State!.BuildUsingTemplate(tmpl, blackboardId);

                await PushStateAsync(default);
            }

            this._deferredHandler ??= await this._systemVGrainProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this.Logger);

            var allControllers = Enum.GetValues<BlackboardControllerTypeEnum>()
                                     .Where(b => b != BlackboardControllerTypeEnum.None)
                                     .ToArray();

            if (this._controllers.Count == 0)
            {
                var initController = new Dictionary<(GrainId, ControllerBaseOptions?), BlackboardControllerHandler>();
                var indexedCfgController = this.State!.TemplateCopy?.Controllers?.GroupBy(grp => grp.ControllerType)
                                                                                 .ToDictionary(grp => grp.Key, v => v.Last()) ?? new Dictionary<BlackboardControllerTypeEnum, BlackboardTemplateControllerDefinition>();

                foreach (var kvControllerType in indexedCfgController)
                {
                    var controllerType = kvControllerType.Key;
                    BlackboardTemplateControllerDefinition? controllerDefinition = null;

                    if (!indexedCfgController.TryGetValue(controllerType, out controllerDefinition))
                        controllerDefinition = new BlackboardTemplateControllerDefinition(Guid.NewGuid(), controllerType, s_defaultControllerConcretType, DefaultControllerOptions.Default);

                    var grainDefType = controllerDefinition.AgentInterfaceType.ToType();

                    var controllerGrain = this._grainOrleanFactory.GetGrain(grainDefType, this.GetPrimaryKey()).AsReference<IBlackboardBaseControllerGrain>()!;

                    var key = (controllerGrain.GetGrainId(), controllerDefinition.Options);

                    BlackboardControllerHandler? handler = null;
                    initController.TryGetValue(key, out handler);

                    if (handler == null)
                    {
                        handler = new BlackboardControllerHandler(this.State.BlackboardId.Uid,
                                                                  grainDefType,
                                                                  this._grainOrleanFactory,
                                                                  controllerDefinition.Options,
                                                                  this.State!.TemplateCopy!);
                        initController.Add(key, handler);
                    }

                    this._controllers.Add(controllerType, handler);
                }
            }

            var logicalPatternsRules = this.State.TemplateCopy!.LogicalTypes
                                                               .GroupBy(k => k.LogicalTypePattern ?? ".*")
                                                               .ToDictionary(k => k.Key, v => v.Select(kv => kv).ToReadOnly());

            if (logicalPatternsRules.Keys.Count != this._logicalHandlers.Count)
            {
                // TODO : found a solution on the logical Pattern order

                var updatedItem = new HashSet<BlackboardLogicalTypeHandler>();

                foreach (var kvLogicalType in logicalPatternsRules)
                {
                    var handler = this._logicalHandlers.FirstOrDefault(h => h.Equals(kvLogicalType.Key));

                    if (handler is null)
                        handler = new BlackboardLogicalTypeHandler(kvLogicalType.Key);

                    var rules = kvLogicalType.Value;

                    var order = kvLogicalType.Value.OfType<BlackboardOrderLogicalTypeRule>().FirstOrDefault();
                    var storage = kvLogicalType.Value.OfType<BlackboardStorageLogicalTypeRule>().FirstOrDefault();
                    var remain = kvLogicalType.Value.OfType<BlackboardRemainOnSealedLogicalTypeRule>().FirstOrDefault();

                    var extractedRules = new BlackboardLogicalTypeBaseRule?[]
                                         {
                                             order,
                                             storage,
                                             remain
                                         }
                                         .Where(d => d is not null)
                                         .OfType<BlackboardLogicalTypeBaseRule>()
                                         .ToArray();

                    rules = rules.Except(extractedRules).ToArray();

                    handler.Update(this._repositoryFactory,
                                   this._blackboardDataLogicalTypeRuleValidatorProvider,
                                   order,
                                   storage?.Storage ?? this.State.TemplateCopy.DefaultStorageConfig,
                                   remain,
                                   rules);

                    updatedItem.Add(handler);
                }

                var toRemoves = this._logicalHandlers.Except(updatedItem).ToArray();

                this._logicalHandlers.Clear();

                foreach (var handler in updatedItem)
                    this._logicalHandlers.Add(handler);

                foreach (var toRm in toRemoves)
                    toRm.Dispose();
            }
        }

        /// <inheritdoc />
        public Task<BlackboardId> GetIdentityAsync()
        {
            return Task.FromResult(this.State!.BlackboardId);
        }

        #region IBlackboardGrain

        /// <inheritdoc />
        public Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, GrainCancellationToken token)
        {
            CheckInitializationRequiredOrSealedStatus();

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? recordStatusFilter, GrainCancellationToken token)
        {
            CheckInitializationRequired();

            var metaData = GetAllStoredMetaDatByFilters(logicTypeFilter, displayNameFilter, recordStatusFilter);
            return await GetAllStoredDataByMetaDataAsync<TDataProjection>(metaData.Select(m => m.Value), token.CancellationToken);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(GrainCancellationToken token)
        {
            CheckInitializationRequired();
            return GetAllStoredMetaDataFilteredAsync(null, null, null, token);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? recordStatusFilter, GrainCancellationToken token)
        {
            CheckInitializationRequired();
            var metaData = GetAllStoredMetaDatByFilters(logicTypeFilter, displayNameFilter, recordStatusFilter);

            return Task.FromResult(metaData.Select(kv => new MetaDataRecordContainer(kv.Value.LogicalType,
                                                                                     kv.Value.Uid,
                                                                                     kv.Value.DisplayName,
                                                                                     kv.Value.ContainsType is not null ? ConcretBaseTypeConverter.ConvertFromSurrogate(kv.Value.ContainsType!) : (ConcretType?)null,
                                                                                     kv.Value.Status,
                                                                                     kv.Value.UTCCreationTime,
                                                                                     kv.Value.CreatorIdentity,
                                                                                     kv.Value.UTCLastUpdateTime,
                                                                                     kv.Value.LastUpdaterIdentity,
                                                                                     kv.Value.CustomMetadata)).ToReadOnly());
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, GrainCancellationToken token)
        {
            CheckInitializationRequired();
            var metaData = GetAllStoredMetaDatByFilters(filter);

            return Task.FromResult(metaData.Select(kv => new MetaDataRecordContainer(kv.Value.LogicalType,
                                                                                     kv.Value.Uid,
                                                                                     kv.Value.DisplayName,
                                                                                     kv.Value.ContainsType is not null ? ConcretBaseTypeConverter.ConvertFromSurrogate(kv.Value.ContainsType!) : (ConcretType?)null,
                                                                                     kv.Value.Status,
                                                                                     kv.Value.UTCCreationTime,
                                                                                     kv.Value.CreatorIdentity,
                                                                                     kv.Value.UTCLastUpdateTime,
                                                                                     kv.Value.LastUpdaterIdentity,
                                                                                     kv.Value.CustomMetadata)).ToReadOnly());
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(ConditionExpressionDefinition filter, GrainCancellationToken token)
        {
            CheckInitializationRequired();
            var metaData = GetAllStoredMetaDatByFilters(filter);

            return GetAllStoredDataByMetaDataAsync<TDataProjection>(metaData.Select(m => m.Value).ToArray(), token.CancellationToken);
        }

        /// <inheritdoc />
        public async Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, GrainCancellationToken token)
        {
            CheckInitializationRequired();
            return (await GetStoredDataImplAsync<TDataProjection>(token.CancellationToken, dataUid.AsEnumerable().ToArray()))?.SingleOrDefault();
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(IReadOnlyCollection<Guid> dataUids, GrainCancellationToken token)
        {
            CheckInitializationRequired();
            return GetStoredDataImplAsync<TDataProjection>(token.CancellationToken, dataUids);
        }

        /// <inheritdoc />
        public async Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, GrainCancellationToken token)
        {
            CheckInitializationRequiredOrSealedStatus();
            return await PushDataAsync<NoneType>(new DataRecordContainer<NoneType?>(logicType,
                                                                                    uid,
                                                                                    displayName,
                                                                                    null,
                                                                                    RecordStatusEnum.Preparation,
                                                                                    this._timeManager.UtcNow,
                                                                                    null,
                                                                                    this._timeManager.UtcNow,
                                                                                    null,
                                                                                    null),
                                                 DataRecordPushRequestTypeEnum.OnlyNew,
                                                 token);
        }

        /// <inheritdoc />
        public Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, GrainCancellationToken token)
        {
            CheckInitializationRequiredOrSealedStatus();
            return CommandExecutionStartPointAsync(token.CancellationToken,
                                                   new BlackboardCommandStorageAddRecord<TData>(record,
                                                                                                InsertIfNew: pushType != DataRecordPushRequestTypeEnum.UpdateOnly,
                                                                                                @Override: pushType != DataRecordPushRequestTypeEnum.OnlyNew));
        }

        /// <inheritdoc />
        public Task<BlackboardQueryResponse> QueryAsync(BlackboardQueryCommand command, GrainCancellationToken token)
        {
            CheckInitializationRequiredOrSealedStatus();
            return SolveQueryRequestAsync<NoneType>(command, null, token);
        }

        /// <inheritdoc />
        public Task<BlackboardQueryResponse> QueryAsync<TResponse>(BlackboardQueryRequest<TResponse> request, GrainCancellationToken token)
        {
            CheckInitializationRequiredOrSealedStatus();
            return SolveQueryRequestAsync<TResponse>(request, null, token);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDataAsync(GrainCancellationToken token, IIdentityCard identity, params Guid[] slotIds)
        {
            CheckInitializationRequiredOrSealedStatus();
            // TODO : check identity

            var ctx = new CommandExecutionContext(token.CancellationToken);
            var rmCmds = slotIds.Select(uid => new BlackboardCommandStorageRemoveRecord(uid)).ToArray();

            return await CommandExecutionStartPointAsync(token.CancellationToken, rmCmds);
        }

        /// <summary>
        /// Override this activato get the activation context scheduler
        /// </summary>
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            this._activationScheduler = TaskScheduler.Current;
            return base.OnActivateAsync(cancellationToken);
        }

        #endregion

        #region Tools

        private static bool QueryRequestCommandFilter(BlackboardCommand c)
        {
            return c.ActionType == BlackboardCommandTypeEnum.Reject ||
                   c.ActionType == BlackboardCommandTypeEnum.Reponse ||
                   c.ActionType == BlackboardCommandTypeEnum.Deferred;
        }

        /// <summary>
        /// Resove call generic execution
        /// </summary>
        private static async Task<bool> CommandGenericCall(BlackboardGrain grain, BlackboardCommand cmd, CommandExecutionContext ctx, MethodInfo s_commandExec)
        {
            var mthd = s_commandExec.MakeGenericMethodWithCache(cmd.GetType().GetGenericArguments());
            return await (Task<bool>)mthd.Invoke(grain, new object[] { cmd, ctx })!;
        }

        /// <summary>
        /// Commands the execution.
        /// </summary>
        private Task<bool> CommandExecutionStartPointAsync(CancellationToken token, params BlackboardCommand[] commands)
        {
            return CommandExecutionStartPointAsync(token, commands, null);
        }

        /// <summary>
        /// Commands the execution.
        /// </summary>
        private async Task<bool> CommandExecutionStartPointAsync(CancellationToken token, IReadOnlyCollection<BlackboardCommand> commands, CommandExecutionContext? parentContext)
        {
            using (var execContext = parentContext is not null ? new CommandExecutionContext(parentContext) : new CommandExecutionContext(token))
            {
                var oldEvents = execContext.Events?.ToArray();

                var result = await CommandExecutionerAsync(commands, execContext);
                token.ThrowIfCancellationRequested();

                var events = execContext.Events?.ToArray();

                var newEvents = events ?? EnumerableHelper<BlackboardEvent>.ReadOnlyArray;

                if (oldEvents is not null)
                    newEvents = newEvents.Except(oldEvents).ToArray();

                if (newEvents is not null && this.Logger.IsEnabled(LogLevel.Debug))
                {
                    foreach (var e in newEvents)
                    {
                        this.Logger.OptiLog(LogLevel.Debug,
                                            "[BLACKBOARD] [EVENT] [{blackboardId}:{blackboardName} - {blacboardTemplateName}] {event}",
                                            this.State?.BlackboardId,
                                            this.State?.Name,
                                            this.State?.TemplateCopy?.UniqueTemplateName,
                                            e);
                    }
                }

                if (result)
                {
                    await BlackboardGrainStateSaving(false, token).ConfigureAwait(false);
                }

                if (newEvents is not null && newEvents.Length > 0)
                {
                    var eventController = await GetControllerAsync<IBlackboardEventControllerGrain>(BlackboardControllerTypeEnum.Event, token);

                    if (eventController is not null)
                    {
                        var eventActions = await eventController.ReactToEventsAsync(new BlackboardEventBook(newEvents), token.ToGrainCancellationTokenSource().Token);

                        if (eventActions is not null && eventActions.Any())
                            await CommandExecutionStartPointAsync(token, eventActions, execContext);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Blackboards the grain state saving.
        /// </summary>
        private async Task BlackboardGrainStateSaving(bool force, CancellationToken token)
        {
            // Force save all the 10 push this is to prevent any data lost
            if (force && Interlocked.Increment(ref this._saveCounter) > 10)
            {
                await this._delaySaving.StopAsync(token);
                await DelayPushStateAsync(token);
                return;
            }

            // otherwise restart the timer to delay the save

            // Restart in fire and forget mode the timer to save to prevent save state each time
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => this._delaySaving.StartAsync()).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Commands the execution.
        /// </summary>
        private async Task<bool> CommandExecutionerAsync(IReadOnlyCollection<BlackboardCommand> commands, CommandExecutionContext context)
        {
            using (var localCmdExecContext = new CommandExecutionContext(context))
            {
                foreach (var cmd in commands)
                {
                    Exception? exception = null;
                    try
                    {
                        if (s_rootCommandExecutor.TryGetValue(cmd.ActionType, out var exec))
                        {
                            var execSucceed = await exec(this, cmd, context);
                            if (execSucceed)
                                continue;

                            throw new BlackboardCommandExecutionException(cmd, "Execution failed", null);
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    // TODO : Apply rollback ??
                    throw new BlackboardCommandExecutionException(cmd, exception?.Message ?? string.Empty, exception);
                }

                return true;
            }
        }

        private async ValueTask<IRepository<DataRecordContainer, Guid>> GetDedicatedRepositoryAsync(string logicalType, CancellationToken token)
        {
            var repo = this._logicalHandlers!.FirstOrDefault(f => f.Match(logicalType ?? string.Empty));

            IRepository<DataRecordContainer, Guid>? repository = null;

            if (repo is not null)
                repository = await repo.GetRepositoryAsync(token);

            if (repository is null)
            {
                this.Logger.OptiLog(LogLevel.Critical,
                                    "[{blackboard}:{blackboardTemplate}] Couldn't found repository associate to logical type {logicalType}",
                                    this.State!.BlackboardId.BoardName,
                                    this.State!.BlackboardId.BoardTemplateKey,
                                    logicalType);

                throw new ArgumentNullException("[{0}:{1}] Couldn't found repository associate to logical type {2}".WithArguments(this.State!.BlackboardId.BoardName,
                                                                                                                                  this.State!.BlackboardId.BoardTemplateKey,
                                                                                                                                  logicalType));
            }

            return repository;
        }

        /// <summary>
        /// Ensures the record validate rule asynchronous.
        /// </summary>
        private async ValueTask<BlackboardProcessingIssue?> EnsureRecordValidateRuleAsync<TData>(DataRecordContainer<TData> record, CancellationToken token)
        {
            var handler = this._logicalHandlers!.FirstOrDefault(f => f.Match(record?.LogicalType ?? string.Empty));

            if (handler is null || handler.RuleSolver is null)
                return null;

            var issue = await handler.RuleSolver!.ValidateAsync(record, this.State!.Registry.RecordMetadatas);
            return issue;
        }

        /// <summary>
        /// Gets all stored meta dat by filters.
        /// </summary>
        private IEnumerable<KeyValuePair<Guid, BlackboardRecordMetadata>> GetAllStoredMetaDatByFilters(string? logicTypeFilter,
                                                                                                       string? displayNameFilter,
                                                                                                       RecordStatusEnum? recordStatusFilter)
        {
            CheckInitializationRequired();

            IEnumerable<KeyValuePair<Guid, BlackboardRecordMetadata>> metaData = this.State!.Registry.RecordMetadatas;

            if (!string.IsNullOrEmpty(logicTypeFilter))
            {
                var regFilter = new Regex(logicTypeFilter);
                metaData = metaData.Where(m => regFilter.IsMatch(m.Value.LogicalType));
            }

            if (!string.IsNullOrEmpty(displayNameFilter))
            {
                var regDisplayNameFilter = new Regex(displayNameFilter);
                metaData = metaData.Where(m => regDisplayNameFilter.IsMatch(m.Value.DisplayName));
            }

            if (recordStatusFilter is not null && recordStatusFilter.Value != RecordStatusEnum.None)
                metaData = metaData.Where(m => (m.Value.Status & recordStatusFilter) == recordStatusFilter);

            return metaData;
        }

        /// <summary>
        /// Gets all stored meta dat by filters.
        /// </summary>
        private IEnumerable<KeyValuePair<Guid, BlackboardRecordMetadata>> GetAllStoredMetaDatByFilters(ConditionExpressionDefinition conditionExpression)
        {
            CheckInitializationRequired();

            IEnumerable<KeyValuePair<Guid, BlackboardRecordMetadata>> metaData = this.State!.Registry.RecordMetadatas;

            var filter = conditionExpression.ToExpression<BlackboardRecordMetadata, bool>().Compile();

            return metaData.Where(kv => filter(kv.Value));
        }

        /// <inheritdoc />
        protected override async Task OnActivationSetupState(BlackboardGrainState? state, CancellationToken ct)
        {
            if ((this._controllers.Count == 0 || this._logicalHandlers.Count == 0) && state is not null && state.TemplateCopy is not null)
                await BuildFromTemplateAsync(state.TemplateCopy.Uid, state.BlackboardId);

            await base.OnActivationSetupState(state, ct);
        }

        /// <summary>
        /// Delays the push state asynchronous.
        /// </summary>
        private async Task DelayPushStateAsync(CancellationToken token)
        {
            await Task.Factory.StartNew(async () =>
            {
                await this._saveStateLocker.WaitAsync(token);
                try
                {
                    Interlocked.Exchange(ref this._saveCounter, 0);
                    await PushStateAsync(token);
                }
                finally
                {
                    this._saveStateLocker.Release();
                }
            }, token, TaskCreationOptions.None, scheduler: this._activationScheduler!);
        }

        /// <inheritdoc cref="IBlackboard.GetStoredDataAsync{TDataProjection}(Guid, GrainCancellationToken)"/>
        private async Task<IReadOnlyCollection<DataRecordContainer<TData?>>> GetStoredDataImplAsync<TData>(CancellationToken token, IReadOnlyCollection<Guid> uids)
        {
            CheckInitializationRequired();

            var metaDatas = uids.Select(uid =>
                                {
                                    if (!this.State!.Registry.RecordMetadatas.TryGetValue(uid, out var data))
                                        return (BlackboardRecordMetadata?)null;
                                    return data;
                                }).Where(d => d is not null)
                                .Select(d => d!.Value)
                                .ToArray();

            if (!metaDatas.Any())
                return EnumerableHelper<DataRecordContainer<TData?>>.ReadOnly;

            return await GetAllStoredDataByMetaDataAsync<TData>(metaDatas, token);
        }

        /// <inheritdoc />
        private async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByMetaDataAsync<TDataProjection>(IEnumerable<BlackboardRecordMetadata> metaDatas,
                                                                                                                                        CancellationToken token)
        {
            CheckInitializationRequired();

            var metaDatasWithRepoTasks = metaDatas.Select(kv => (RepoTask: GetDedicatedRepositoryAsync(kv.LogicalType, token), kv))
                                                  .ToArray();

            await (metaDatasWithRepoTasks.Select(t => t.RepoTask).ToArray()).SafeWhenAllAsync(token);

            var fetchTasks = metaDatasWithRepoTasks.GroupBy(kv => kv.RepoTask.Result)
                                                   .Select(kv => kv.Key.GetByIdsValueAsync(kv.Select(k => k.kv.Uid).Distinct().ToArray(), token).AsTask())
                                                   .ToArray();

            await fetchTasks.SafeWhenAllAsync(token);

            var results = fetchTasks.Where(f => f.IsCompletedSuccessfully && f.Result is not null)
                                    .SelectMany(f => f.Result)
                                    .Distinct()
                                    .ToArray();

            var resultProject = new List<DataRecordContainer<TDataProjection?>>();

            foreach (var result in results)
            {
                if (result.TryProjectTo<TDataProjection>(this._converter, out var alreadyWellTyped) && alreadyWellTyped is not null)// is DataRecordContainer<TDataProjection?> alreadyWellTyped)
                {
                    resultProject.Add(alreadyWellTyped!);
                    continue;
                }

                throw new NotSupportedException("Data projection is not yet supported");
            }

            return resultProject;
        }

        /// <summary>
        /// Gets the controller after initialization if needed
        /// </summary>
        private async ValueTask<TSpecializedController?> GetControllerAsync<TSpecializedController>(BlackboardControllerTypeEnum controllerType, CancellationToken token)
            where TSpecializedController : IBlackboardBaseControllerGrain
        {
            if (this._controllers.TryGetValue(controllerType, out var controller))
                return await controller.GetController<TSpecializedController>(token);

            return default;
        }

        /// <summary>
        /// Commands the execute storage asynchronous.
        /// </summary>
        private static async Task<bool> CommandExecuteAsync<TKey, TRootCommand>(IReadOnlyDictionary<TKey, Func<BlackboardGrain, TRootCommand, CommandExecutionContext, Task<bool>>> processors,
                                                                                Func<TRootCommand, TKey> getKeyFromCommand,
                                                                                BlackboardGrain grain,
                                                                                TRootCommand command,
                                                                                CommandExecutionContext context)
            where TRootCommand : BlackboardCommand
        {
            if (processors.TryGetValue(getKeyFromCommand(command), out var storageAction))
                return await storageAction(grain, command, context);

            throw new KeyNotFoundException($"[Blacboard] No action executor for type {command.ToDebugDisplayName()}");
        }

        /// <summary>
        /// Command called when the controller decided to reject the primary action
        /// </summary>
        private static Task<bool> CommandExecuteRejectAsync(BlackboardGrain grain, BlackboardCommand command, CommandExecutionContext context)
        {
            grain.Logger.OptiLog(LogLevel.Warning, "Command rejected by the controller after the folling issue {issue}", ((BlackboardCommandRejectAction)command).SourceIssue);
            return Task.FromResult(false);
        }

        /// <summary>
        /// Commands the retry deferred asynchronous.
        /// </summary>
        private async Task<bool> CommandExecuteRetryDeferredAsync(BlackboardCommand _, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            var pendingRequests = this.State?.GetQueries() ?? EnumerableHelper<BlackboardDeferredQueryState>.ReadOnly;

            bool requestSolved = false;

            using (var grainCancellation = ctx.CancellationToken.ToGrainCancellationTokenSource())
            {
                foreach (var req in pendingRequests)
                {
                    try
                    {
                        BlackboardQueryResponse? response = null;
                        var query = req.GetRequest();

                        if (query.Type == BlackboardQueryTypeEnum.Request)
                        {
                            Debug.Assert(req.ResponseType is not null);

                            response = await SolveGenericQueryAsync(this, query, req.ResponseType!, req.DeferredId, grainCancellation.Token);
                        }
                        else if (query.Type == BlackboardQueryTypeEnum.Command)
                        {
                            response = await QueryAsync((BlackboardQueryCommand)query, grainCancellation.Token);
                        }

                        if (response is not null)
                            requestSolved |= response.Type == QueryReponseTypeEnum.Direct;
                    }
                    catch (Exception ex)
                    {
                        this.Logger.OptiLog(LogLevel.Error, "RetryDeferred - {grainId} - {request} - {exception}", GetGrainId(), req, ex);
                    }
                }
            }

            return requestSolved;
        }

        /// <summary>
        /// Check and resolved possible issue related to <paramref name="record"/> be added
        /// </summary>
        /// <returns>
        ///     <c>true</c> to continue the insert; otherwise <c>false</c>
        /// </returns>
        private async ValueTask<IReadOnlyCollection<BlackboardCommand>?> CheckAndResolveAnyPushIssue<TData>(BlackboardCommandStorageAddRecord<TData?> cmd, DataRecordContainer<TData?> record, CommandExecutionContext ctx)
        {
            BlackboardProcessingIssue? pushIssue = null;

            if (this.State!.Registry.RecordMetadatas.TryGetValue(record.Uid, out var existing) && cmd.Override == false)
                pushIssue = new ConflictBlackboardProcessingRuleIssue(existing.AsEnumerable().ToReadOnly(), record);

            if (pushIssue is null)
                pushIssue = await EnsureRecordValidateRuleAsync(record, ctx.CancellationToken);

            if (pushIssue is not null)
            {
                var controller = await GetControllerAsync<IBlackboardStorageControllerGrain>(BlackboardControllerTypeEnum.Storage, ctx.CancellationToken);

                if (controller is null)
                    throw new BlackboardPushValidationException(record, "Missing controller for type " + BlackboardControllerTypeEnum.Storage + " : " + pushIssue.ToDebugDisplayName(), null);

                IReadOnlyCollection<BlackboardCommand>? solvingActions = null;
                Exception? exception = null;

                if (pushIssue is BlackboardProcessingStorageIssue storageIssue)
                {
                    try
                    {
                        using (var grainCancelToken = ctx.CancellationToken.ToGrainCancellationTokenSource())
                        {
                            solvingActions = await controller.ResolvePushIssueAsync(storageIssue, record, grainCancelToken.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }

                if (exception != null || controller is null || solvingActions is null || !solvingActions.Any())
                    throw new BlackboardPushValidationException(record, pushIssue.ToDebugDisplayName(), exception);

                return solvingActions;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        private static async Task<BlackboardQueryResponse> SolveGenericQueryAsync(BlackboardGrain grain, BlackboardBaseQuery request, ConcretBaseType response, DeferredId? deferredId, GrainCancellationToken token)
        {
            var tsk = (Task)s_solveGenericQueryRequest.MakeGenericMethod(response.ToType()).Invoke(grain, new object?[] { request, deferredId, token })!;
            await tsk;

            return tsk.GetResult<BlackboardQueryResponse>()!;
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task<BlackboardQueryResponse> SolveQueryRequestAsync<TResponse>(BlackboardBaseQuery query, DeferredId? deferredId, GrainCancellationToken token)
        {
            CheckInitializationRequiredOrSealedStatus();

            var ctrller = await GetControllerAsync<IBlackboardEventControllerGrain>(BlackboardControllerTypeEnum.Event, token.CancellationToken);

            if (ctrller is null)
                return BlackboardQueryRejectedResponse.NoController;

            bool saveState = false;
            var queryUid = query.QueryUid;

            DeferredStatusMessage? runningDeferred = null;

            // If not deferred id provided check all recorded
            // This check different when the query come from direct call or retry
            if (deferredId is null)
            {
                var existingDeferreds = await this._deferredHandler!.GetLastDeferredStatusBySourceIdAsync(queryUid, this.IdentityCard);

                // Get from existing deferred works the older not solved
                runningDeferred = existingDeferreds.OrderByDescending(e => e.UTCLastUpdateStatus)
                                                   .Cast<DeferredStatusMessage?>() // Cast to force null if not founded
                                                   .FirstOrDefault<DeferredStatusMessage?>(d => d!.Value.Status == DeferredStatusEnum.Initialize || d!.Value.Status == DeferredStatusEnum.Alive);
            }
            else
            {
                runningDeferred = await this._deferredHandler!.GetLastDeferredStatusAsync(deferredId.Value.Uid);
            }

            var ctrlResponses = await ctrller.ProcessRequestAsync<TResponse>(query, token);

            if (ctrlResponses is null)
                ctrlResponses = BlackboardCommandRejectAction.Default.AsEnumerable().ToArray();

            BlackboardQueryResponse? queryResponse = null;

            var result = ctrlResponses.Where(QueryRequestCommandFilter)
                                      .GroupBy(rep => rep.ActionType)
                                      .ToDictionary(k => k.Key, v => v.Last());

            if (result.TryGetValue(BlackboardCommandTypeEnum.Reponse, out var direct) && direct is BlackboardCommandResponse<TResponse> reponseCmd)
            {
                queryResponse = new BlackboardQueryDirectResponse<TResponse>(reponseCmd.Response, queryUid);
                if (runningDeferred is not null)
                    await this._deferredHandler.FinishDeferredWorkStatusAsync(runningDeferred.Value.DeferredId.Uid, this.IdentityCard, reponseCmd.Response);
            }
            else if (result.TryGetValue(BlackboardCommandTypeEnum.Reject, out var rejected) && rejected is BlackboardCommandRejectAction rejectCmd)
            {
                if (runningDeferred is not null)
                {
                    await this._deferredHandler.ExceptionDeferredWorkStatusAsync(runningDeferred.Value.DeferredId.Uid,
                                                                                 this.IdentityCard,
                                                                                 new BlackboardQueryRejectedException(queryUid,
                                                                                                                      rejectCmd.SourceIssue.ToDebugDisplayName(),
                                                                                                                      null).ToDemocriteInternal());
                }
                queryResponse = new BlackboardQueryRejectedResponse(rejectCmd.SourceIssue?.ToDebugDisplayName());
            }
            else if (result.TryGetValue(BlackboardCommandTypeEnum.Deferred, out var deferred) && deferred is BlackboardCommandDeferredResponse deferredCmd)
            {
                DeferredId deferredWorkUid;

                if (runningDeferred is not null)
                {
                    deferredWorkUid = runningDeferred.Value.DeferredId;
                    await this._deferredHandler.KeepDeferredWorkStatusAsync(queryUid, this.IdentityCard);
                }
                else
                {
                    var newDeferredId = await this._deferredHandler.CreateDeferredWorkAsync(queryUid, this.IdentityCard, (ConcretBaseType)typeof(TResponse).GetAbstractType());
                    deferredWorkUid = new DeferredId(newDeferredId, queryUid);
                }

                saveState = this.State!.AddOrUpdateQuery(BlackboardDeferredQueryState.Create(deferredWorkUid, query, this._democriteSerializer));

                queryResponse = new BlackboardQueryDeferredResponse(deferredWorkUid);
            }

            if (queryResponse is not null && queryResponse.Type != QueryReponseTypeEnum.Deferred)
            {
                if (deferredId is not null)
                    saveState = this.State!.RemoveQuery(deferredId.Value.Uid);
                else if (runningDeferred is not null)
                    saveState = this.State!.RemoveQuery(runningDeferred.Value.DeferredId.Uid);
                else
                    // TODO : check by request.Uid == DeferredId.SourceId instead of UID
                    saveState = this.State!.RemoveQuery(query);
            }

            if (saveState)
                await BlackboardGrainStateSaving(true, token.CancellationToken);
            //await this.PushStateAsync(token.CancellationToken);

            // Check remain command that are not dedicated to query result
            // Those remain command are executed at the end to prevent any deadlock loop due to a command that could retry the deferred request
            var remainActions = ctrlResponses.Where(c => !QueryRequestCommandFilter(c))
                                             .ToArray();

            if (remainActions.Any())
                await this.CommandExecutionStartPointAsync(token.CancellationToken, remainActions);

            if (queryResponse is null)
                throw new NotSupportedException("No Response - not supported yet");

            return queryResponse;
        }

        /// <summary>
        /// Checks the initialization required or sealed status.
        /// </summary>
        private void CheckInitializationRequiredOrSealedStatus()
        {
            CheckInitializationRequired();
            CheckSealedStatus();
        }

        /// <summary>
        /// Checks the initialization required or sealed status.
        /// </summary>
        private void CheckInitializationRequired()
        {
            if (this._initializing)
                return;

            var status = this.State!.CurrentLifeStatus;

            if (this.State!.TemplateCopy?.ConfigurationDefinition?.InitializationRequired == true && (status == BlackboardLifeStatusEnum.None || status == BlackboardLifeStatusEnum.WaitingInitialization))
                throw new EntityRequiredInitializationException(this, this.State!.TemplateCopy!.DisplayName + ":" + this.GetPrimaryKeyString());
        }

        /// <summary>
        /// Checks the initialization required or sealed status.
        /// </summary>
        private void CheckSealedStatus()
        {
            if (this._sealing)
                return;

            var status = this.State!.CurrentLifeStatus;

            if (status == BlackboardLifeStatusEnum.Sealed)
                throw new EntitySealedException(this, this.State!.TemplateCopy!.DisplayName + ":" + this.GetPrimaryKeyString());
        }

        #region Command Actions

        /// <summary>
        /// Call after all validation and conflic resolved to save the data itself
        /// </summary>
        private async Task<bool> Command_AddToStorageAsync<TData>(BlackboardCommandStorageAddRecord<TData?> cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            if (cmd is null || cmd.Record is null)
                return false;

            var record = cmd.Record;

            if (record.RecordContainerType != RecordContainerTypeEnum.Direct)
                throw new InvalidOperationException("Only record type Direct could be push as update or new entry");

            var solvingActions = await CheckAndResolveAnyPushIssue(cmd, record, ctx);

            if (solvingActions is not null)
                return await CommandExecutionerAsync(solvingActions, ctx);

            var successResult = true;
            var repository = await GetDedicatedRepositoryAsync(record.LogicalType, ctx.CancellationToken);

            await this._metaDataLocker.WaitAsync(ctx.CancellationToken);
            try
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();

                if (record.Uid == Guid.Empty)
                    record.ForceUid(Guid.NewGuid());

                if (!await repository.PushDataRecordAsync(record, true, ctx.CancellationToken))
                    throw new BlackboardPushException(record, "Push failed", null);

                var entry = this.State!.Registry.Push(record, this._timeManager);
                ctx.EnqueueEvent(new BlackboardEventStorage(BlackboardEventStorageTypeEnum.Add, entry.Uid, entry));
            }
            catch (Exception ex)
            {
                successResult = true;
                throw new BlackboardPushException(record, ex.Message, ex);
            }
            finally
            {
                this._metaDataLocker.Release();
            }
            return successResult;
        }

        /// <summary>
        /// Command in charge to remove a record from the storage
        /// </summary>
        private async Task<bool> Command_RemoveFromStorageAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            var rmCommand = cmd as BlackboardCommandStorageRemoveRecord;

            ArgumentNullException.ThrowIfNull(rmCommand);

            await this._metaDataLocker.WaitAsync(ctx.CancellationToken);
            try
            {
                if (!this.State!.Registry.RecordMetadatas.TryGetValue(rmCommand.Uid, out var recordMetadata))
                    return false;

                var repository = await GetDedicatedRepositoryAsync(recordMetadata.LogicalType, ctx.CancellationToken);

                ctx.CancellationToken.ThrowIfCancellationRequested();

                if (!await repository.DeleteRecordAsync(recordMetadata.Uid, ctx.CancellationToken))
                    throw new BlackboardCommandExecutionException(cmd, "Remove failed", null);

                var removed = this.State!.Registry.Pop(recordMetadata.Uid);
                ctx.EnqueueEvent(new BlackboardEventStorage(BlackboardEventStorageTypeEnum.Remove, rmCommand.Uid, removed));
            }
            catch (Exception ex)
            {
                throw new BlackboardCommandExecutionException(cmd, ex.Message, ex);
            }
            finally
            {
                this._metaDataLocker.Release();
            }

            return true;
        }

        /// <summary>
        /// Command in charge to decommission a record from the storage
        /// </summary>
        private Task<bool> Command_DecommissionFromStorageAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            var decoCommand = cmd as BlackboardCommandStorageDecommissionRecord;

            ArgumentNullException.ThrowIfNull(decoCommand);

            return Command_ChangeStatusFromStorageAsync(new BlackboardCommandStorageChangeStatusRecord(decoCommand.Uid, RecordStatusEnum.Decommissioned), ctx);
        }

        /// <summary>
        /// Command in charge to change status a record from the storage
        /// </summary>
        private async Task<bool> Command_ChangeStatusFromStorageAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            var chgCommand = cmd as BlackboardCommandStorageChangeStatusRecord;

            ArgumentNullException.ThrowIfNull(chgCommand);

            await this._metaDataLocker.WaitAsync(ctx.CancellationToken);
            try
            {
                if (!this.State!.Registry.RecordMetadatas.TryGetValue(chgCommand.Uid, out var recordMetadata))
                    return false;

                var repository = await GetDedicatedRepositoryAsync(recordMetadata.LogicalType, ctx.CancellationToken);

                ctx.CancellationToken.ThrowIfCancellationRequested();

                var item = await repository.GetByIdValueAsync(chgCommand.Uid, ctx.CancellationToken);

                if (item == null || item.Status == chgCommand.NewRecordStatus)
                    return false;

                var utcNow = this._timeManager.UtcNow;
                var newItem = item.WithNewStatus(chgCommand.NewRecordStatus, utcNow);

                if (newItem is not null)
                    await repository.PushDataRecordAsync(newItem, false, ctx.CancellationToken);

                var newItemMetaData = this.State!.Registry.ChangeStatus(recordMetadata.Uid, chgCommand.NewRecordStatus, utcNow);
                ctx.EnqueueEvent(new BlackboardEventStorage(BlackboardEventStorageTypeEnum.ChangeStatus, chgCommand.Uid, newItemMetaData));
            }
            catch (Exception ex)
            {
                throw new BlackboardCommandExecutionException(cmd, ex.Message, ex);
            }
            finally
            {
                this._metaDataLocker.Release();
            }

            return true;
        }

        /// <summary>
        /// Trigger a seqeunce based on configuration pass in arguments
        /// </summary>
        private async Task<bool> Command_TriggerSequence<TInput>(BlackboardCommandTriggerSequence<TInput> cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            IExecutionLauncher? exec = null;

            if (!NoneType.IsEqualTo<TInput>())
            {
                exec = this._democriteExecutionHandler.Sequence<TInput>(cmd.SequenceId)
                                                      .SetInput(cmd.Input);
            }
            else
            {
                exec = this._democriteExecutionHandler.Sequence(cmd.SequenceId);
            }

            // Fire to prevent deadlock
            await exec.Fire();
            return true;
        }

        /// <summary>
        /// Commands the trigger signal.
        /// </summary>
        private async Task<bool> Command_TriggerSignal(BlackboardCommandTriggerSignal cmd, CommandExecutionContext ctx)
        {
            if (cmd.CarryData)
                return await CommandGenericCall(this, cmd, ctx, s_cmdSignalWithData);

            var fireId = await this._signalService.Fire(cmd.SignalId, ctx.CancellationToken);
            return fireId != Guid.Empty;
        }

        /// <summary>
        /// Commands the trigger signal.
        /// </summary>
        private async Task<bool> Command_TriggerSignalWithData<TData>(BlackboardCommandTriggerSignal<TData> cmd, CommandExecutionContext ctx)
            where TData : struct
        {
            var fireId = await this._signalService.Fire<TData>(cmd.SignalId, cmd.Data, ctx.CancellationToken);
            return fireId != Guid.Empty;
        }

        /// <summary>
        /// Commands the life status initialize.
        /// </summary>
        private async Task<bool> Command_LifeStatus_Initialize(BlackboardCommandLifeInitializeChange cmd, CommandExecutionContext ctx)
        {
            if (this.State!.CurrentLifeStatus != BlackboardLifeStatusEnum.WaitingInitialization)
                return false;

            this._initializing = true;
            try
            {
                BlackboardCommand[]? initCommands = null;

                var stateController = await GetControllerAsync<IBlackboardStateControllerGrain>(BlackboardControllerTypeEnum.State, ctx.CancellationToken);

                if (stateController is null)
                {
                    initCommands = cmd.InitData?.Select(d => d.CreateAddCommand(true, true))
                                            .ToArray();
                }
                else
                {
                    using (var tokenSource = ctx.CancellationToken.ToGrainCancellationTokenSource())
                    {
                        initCommands = (await stateController.OnInitialize(cmd.InitData ?? EnumerableHelper<DataRecordContainer>.ReadOnly, tokenSource.Token))?.ToArray();
                    }
                }

                if (initCommands is not null)
                    await this.CommandExecutionStartPointAsync(ctx.CancellationToken, initCommands, ctx);

                await Command_LifeStatus_Changed(cmd, ctx);
            }
            finally
            {
                this._initializing = false;
            }
            return true;
        }

        /// <summary>
        /// Commands the life status sealed.
        /// </summary>
        private async Task<bool> Command_LifeStatus_Changed(BlackboardCommandLifeStatusChange cmd, CommandExecutionContext ctx)
        {
            if (cmd.OldStatus != this.State!.CurrentLifeStatus)
                throw new InvalidOperationException("Invalid previous state");

            this.State!.CurrentLifeStatus = cmd.NewStatus;
            //await PushStateAsync(ctx.CancellationToken);
            await BlackboardGrainStateSaving(true, ctx.CancellationToken);

            ctx.EnqueueEvent(new BlackboardEventLifeStatusChanged(cmd.NewStatus, cmd.OldStatus));

            return true;
        }

        /// <summary>
        /// Commands the life status sealed.
        /// </summary>
        private async Task<bool> Command_LifeStatus_Sealed(BlackboardCommandLifeStatusChange cmd, CommandExecutionContext ctx)
        {
            if (this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.Sealed)
                return false;

            await Command_LifeStatus_Changed(cmd, ctx);

            this._sealing = true;

            try
            {
                IReadOnlyCollection<BlackboardRecordMetadata> metaDatas;
                using (this._metaDataLocker.Lock())
                {
                    metaDatas = this.State!.Registry.RecordMetadatas.Values.ToArray();
                }

                var logicalTypeToRemain = this._logicalHandlers.Where(h => h.RemainOnSealed)
                                                               .ToArray();

                var toKeepOnSealed = metaDatas.Where(m => m.Status == RecordStatusEnum.Ready && logicalTypeToRemain.Any(h => h.Match(m.LogicalType)))
                                              .ToArray();

                var toRemove = metaDatas.Except(toKeepOnSealed)
                                        .ToArray();

                var stateController = await GetControllerAsync<IBlackboardStateControllerGrain>(BlackboardControllerTypeEnum.State, ctx.CancellationToken);

                BlackboardCommand[]? sealingCommands = null;

                if (stateController is not null)
                {
                    using (var tokenSource = ctx.CancellationToken.ToGrainCancellationTokenSource())
                    {
                        sealingCommands = (await stateController.OnSealed(toKeepOnSealed, toRemove, tokenSource.Token))?.ToArray();
                    }
                }
                else
                {
                    sealingCommands = toRemove.Select(t => new BlackboardCommandStorageRemoveRecord(t.Uid)).ToArray();
                }

                if (sealingCommands is not null)
                    await this.CommandExecutionStartPointAsync(ctx.CancellationToken, sealingCommands, ctx);

                return true;
            }
            finally
            {
                this._sealing = false;
            }
        }

        #endregion

        /// <inheritdoc />
        protected override void DisposeResourcesEnd()
        {
            this._delaySaving.DisposeAsync().AsTask().ContinueWith((_) =>
            {
                this._metaDataLocker.Dispose();
                this._saveStateLocker.Dispose();
            });

            base.DisposeResourcesEnd();
        }

        #endregion

        #endregion
    }
}
