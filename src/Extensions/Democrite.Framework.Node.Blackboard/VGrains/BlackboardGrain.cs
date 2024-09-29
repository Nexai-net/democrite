// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Doors;
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
    using Elvex.Toolbox.Abstractions.Helpers;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans;
    using Orleans.Concurrency;
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
    internal sealed class BlackboardGrain : VGrainBase<BlackboardGrainState, BlackboardGrainStateSurrogate, BlackboardGrainStateConverter, IBlackboardGrain>, IBlackboardGrain, ISignalReceiver
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

        private readonly ISignalService _signalService;
        private readonly IObjectConverter _converter;
        private readonly ITimeManager _timeManager;

        private readonly ILoggerFactory _loggerFactory;
        private readonly CancellationTokenSource _lifetimeToken;

        // Extra protection to ensure metadata are always synchronized with data stored
        private readonly ReaderWriterLockSlim _metaDataLocker;
        private readonly SortedSet<BlackboardLogicalTypeHandler> _logicalHandlers;

        private IDeferredHandlerVGrain? _deferredHandler;

        //private TaskScheduler? _activationScheduler;
        private bool _initializing;
        private bool _sealing;

        private int _saveContextCounter;
        private bool _needSaving;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainBase{TVGrainState, TStateSurrogate, TConverter}" /> class.
        /// </summary>
        static BlackboardGrain()
        {
            //s_allControllerTypes = Enum.GetValues<BlackboardControllerTypeEnum>()
            //                           .Where(b => b != BlackboardControllerTypeEnum.None)
            //                           .ToArray();

            s_defaultController = typeof(IDefaultBlackboardControllerGrain);
            s_defaultControllerConcretType = (ConcretType)s_defaultController.GetAbstractType();
            s_defaultControllerConcretTypeSurrogate = ConcretBaseTypeConverter.ConvertToSurrogate(s_defaultControllerConcretType);

            Expression<Func<BlackboardGrain, BlackboardCommandStorageAddRecord<int>, CommandExecutionContext, Task<bool>>> cmdAddToStorage = (g, cmd, ctx) => g.Command_AddToStorageAsync<int>(cmd, ctx);
            s_addCommandToStorage = ((MethodCallExpression)cmdAddToStorage.Body).Method.GetGenericMethodDefinition();

            Expression<Func<BlackboardGrain, BlackboardCommandTriggerSequence<int>, CommandExecutionContext, Task<bool>>> cmdtriggerSequence = (g, cmd, ctx) => g.Command_TriggerSequence<int>(cmd, ctx);
            s_cmdTriggerSequence = ((MethodCallExpression)cmdtriggerSequence.Body).Method.GetGenericMethodDefinition();

            Expression<Func<BlackboardGrain, BlackboardCommandTriggerSignal<int>, CommandExecutionContext, Task<bool>>> cmdSignalWithData = (g, cmd, ctx) => g.Command_TriggerSignalWithData<int>(cmd, ctx);
            s_cmdSignalWithData = ((MethodCallExpression)cmdSignalWithData.Body).Method.GetGenericMethodDefinition();

            Expression<Func<BlackboardGrain, BlackboardBaseQuery, DeferredId?, GrainCancellationToken, Task<BlackboardQueryResponse?>>> solveQueryExpr = (grain, q, d, t) => grain.SolveQueryRequestAsync<int>(q, d, null, t);
            s_solveGenericQueryRequest = ((MethodCallExpression)solveQueryExpr.Body).Method.GetGenericMethodDefinition();

            s_rootCommandExecutor = new Dictionary<BlackboardCommandTypeEnum, Func<BlackboardGrain, BlackboardCommand, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardCommandTypeEnum.Storage, (g, cmd, ctx) => CommandExecuteAsync(s_rootCommandStorageExecutor!, c => c.StorageAction, g, (BlackboardCommandStorage)cmd, ctx) },
                { BlackboardCommandTypeEnum.Trigger, (g, cmd, ctx) => CommandExecuteAsync(s_rootCommandTriggerExecutor!, c => c.TriggerActionType, g, (BlackboardCommandTrigger)cmd, ctx) },
                { BlackboardCommandTypeEnum.LifeStatusChange, (g, cmd, ctx) => CommandExecuteAsync(s_lifeStatusCommandTriggerExecutor!, c => c.NewStatus, g, (BlackboardCommandLifeStatusChange)cmd, ctx) },

                { BlackboardCommandTypeEnum.Reject, CommandExecuteRejectAsync },
                { BlackboardCommandTypeEnum.RetryDeferred, (g, cmd, ctx) => g.CommandExecuteRetryDeferredAsync(cmd, ctx) },
                { BlackboardCommandTypeEnum.Signal, (g, cmd, ctx) => g.CommandExecuteSignalAsync(cmd, ctx) }
            }.ToFrozenDictionary();

            s_rootCommandStorageExecutor = new Dictionary<BlackboardCommandStorageActionTypeEnum, Func<BlackboardGrain, BlackboardCommandStorage, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardCommandStorageActionTypeEnum.Add, (g, cmd, ctx) => CommandGenericCall(g, cmd, ctx, s_addCommandToStorage) },
                { BlackboardCommandStorageActionTypeEnum.Prepare, (g, cmd, ctx) => g.Command_PrepareSlotFromStorageAsync(cmd, ctx) },
                { BlackboardCommandStorageActionTypeEnum.Remove, (g, cmd, ctx) => g.Command_RemoveFromStorageAsync(cmd, ctx) },
                { BlackboardCommandStorageActionTypeEnum.Decommission, (g, cmd, ctx) => g.Command_DecommissionFromStorageAsync(cmd, ctx) },
                { BlackboardCommandStorageActionTypeEnum.ChangeStatus, (g, cmd, ctx) => g.Command_ChangeStatusFromStorageAsync(cmd, ctx) },
                { BlackboardCommandStorageActionTypeEnum.ChangeMetaData, (g, cmd, ctx) => g.Command_ChangeMetaDataFromStorageAsync(cmd, ctx) },

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardGrain"/> class.
        /// </summary>
        public BlackboardGrain(ILogger<IBlackboardGrain> logger,
                               [PersistentState(BlackboardConstants.BlackboardStateStorageKey, BlackboardConstants.BlackboardStateStorageConfigurationKey)] IPersistentState<BlackboardGrainStateSurrogate> persistentState,
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
            this._loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;

            this._signalService = signalService;
            this._democriteSerializer = democriteSerializer;
            this._systemVGrainProvider = systemVGrainProvider;
            this._democriteExecutionHandler = democriteExecutionHandler;
            this._lifetimeToken = new CancellationTokenSource();
            this._controllers = new Dictionary<BlackboardControllerTypeEnum, BlackboardControllerHandler>();

            this._metaDataLocker = new ReaderWriterLockSlim();

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
        public async Task<bool> SealedAsync(GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequired();

            if (this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.Sealed)
                return false;

            await using (StartModifyContext())
            {
                return await CommandExecutionStartPointAsync(token.CancellationToken,
                                                             GetCallContextLogger(callContextId),
                                                             new BlackboardCommandLifeStatusChange(BlackboardLifeStatusEnum.Sealed, this.State!.CurrentLifeStatus));
            }
        }

        /// <inheritdoc />
        public async Task InitializaCallAsync()
        {
            if (this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.None)
            {
                using (var source = CancellationToken.None.ToGrainCancellationTokenSource())
                {
                    await InitializeAsync(source.Token);
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> InitializeAsync(GrainCancellationToken token, IReadOnlyCollection<DataRecordContainer>? initData = null, Guid? callContextId = null)
        {
            if (this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.WaitingInitialization || this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.None)
            {
                await using (StartModifyContext())
                {
                    var localLogger = GetCallContextLogger(callContextId);
                    var initResult = await CommandExecutionStartPointAsync(token.CancellationToken, localLogger, new BlackboardCommandLifeInitializeChange(this.State!.CurrentLifeStatus, initData));

                    if (initResult)
                    {
                        var signalCmds = this.State!.TemplateCopy?
                                                    .Controllers?
                                                    .Where(c => c.ControllerType == BlackboardControllerTypeEnum.Event)
                                                    .Select(c => c.Options)
                                                    .OfType<EventControllerOptions>()
                                                    .SelectMany(options => options.ListenSignals.Select(s => new BlackboardCommandSignalSubscription(s.Uid,
                                                                                                                                         s.Name ?? s.Uid.ToString(),
                                                                                                                                         false,
                                                                                                                                         BlackboardCommandSignalTypeEnum.Attach))

                                                                                  .Concat(options.ListenDoors.Select(s => new BlackboardCommandSignalSubscription(s.Uid,
                                                                                                                                                      s.Name ?? s.Uid.ToString(),
                                                                                                                                                      true,
                                                                                                                                                      BlackboardCommandSignalTypeEnum.Attach))))
                                                    .ToArray() ?? EnumerableHelper<BlackboardCommandSignalSubscription>.ReadOnlyArray;

                        if (signalCmds.Any())
                            await CommandExecutionStartPointAsync(token.CancellationToken, localLogger, signalCmds);
                    }

                    return initResult;
                }
            }
            return false;
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<BlackboardLifeStatusEnum> GetStatusAsync(GrainCancellationToken token, Guid? callContextId = null)
        {
            return Task.FromResult(this.State!.CurrentLifeStatus);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     No CONTROLLER MUST be called from this method, this could create a dead lock.
        ///     
        ///     Controller that inherite from BlackboardBaseControllerGrain will try to get the IBlackboardRef proxy
        ///     during activation.
        ///     
        /// </remarks>
        public async Task BuildFromTemplateAsync(Guid blackboardTemplateUid, BlackboardId blackboardId, GrainCancellationToken token, Guid? callContextId)
        {
            var grainId = base.GetGrainId();

            try
            {
                this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Start BuildFromTemplateAsync({blackboardTemplateUid}, {blackboardId})", grainId, blackboardTemplateUid, blackboardId);
                if (this.State!.IsBuild == false)
                {
                    this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Start Building", grainId);

                    var templateProvider = this._serviceProvider.GetRequiredService<IBlackboardTemplateDefinitionProvider>();
                    var tmpl = await templateProvider.GetByKeyAsync(blackboardTemplateUid, token.CancellationToken);

#pragma warning disable IDE0270 // Use coalesce expression
                    if (tmpl is null)
                        throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), blackboardTemplateUid.ToString());
#pragma warning restore IDE0270 // Use coalesce expression

                    if (tmpl.ConfigurationDefinition?.InitializationRequired == true)
                        this.State.CurrentLifeStatus = BlackboardLifeStatusEnum.WaitingInitialization;

                    this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Start BuildUsingTemplate state", grainId);
                    this.State!.BuildUsingTemplate(tmpl, blackboardId);

                    await PushStateAsync(token.CancellationToken);
                    this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- State Saved after build", grainId);

                    this.Logger = this._loggerFactory.CreateLogger("[Blackboard {0} name: {1} template: {2}] ".WithArguments(grainId, this.State?.Name, this.State?.TemplateCopy?.Uid));
                }

                this._deferredHandler ??= await this._systemVGrainProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this.Logger);

                token.CancellationToken.ThrowIfCancellationRequested();

                if (this._controllers.Count == 0)
                {
                    this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Building controllers", grainId);
                    token.CancellationToken.ThrowIfCancellationRequested();

                    var initController = new Dictionary<(GrainId, ControllerBaseOptions?), BlackboardControllerHandler>();
                    var indexedCfgController = this.State!.TemplateCopy?.Controllers?.GroupBy(grp => grp.ControllerType)
                                                                                     .ToDictionary(grp => grp.Key, v => v.Last()) ?? new Dictionary<BlackboardControllerTypeEnum, BlackboardTemplateControllerDefinition>();

                    var primaryKey = this.GetPrimaryKey();
                    foreach (var kvControllerType in indexedCfgController)
                    {
                        this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Building Controller -- {type} -- {controller}", grainId, kvControllerType.Key, kvControllerType.Value.DisplayName);

                        var controllerType = kvControllerType.Key;
                        BlackboardTemplateControllerDefinition? controllerDefinition = null;

                        if (!indexedCfgController.TryGetValue(controllerType, out controllerDefinition))
                            controllerDefinition = new BlackboardTemplateControllerDefinition(Guid.NewGuid(), controllerType, s_defaultControllerConcretType, DefaultControllerOptions.Default, null);

                        var grainDefType = controllerDefinition.AgentInterfaceType.ToType();

                        var controllerGrain = this._grainOrleanFactory.GetGrain(grainDefType, primaryKey).AsReference<IBlackboardBaseControllerGrain>()!;
                        token.CancellationToken.ThrowIfCancellationRequested();

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

                var logicalPatternsRules = this.State!.TemplateCopy!.LogicalTypes
                                                                    .GroupBy(k => k.LogicalTypePattern ?? ".*")
                                                                    .ToDictionary(k => k.Key, v => v.Select(kv => kv).ToReadOnly());

                if (logicalPatternsRules.Keys.Count != this._logicalHandlers.Count)
                {
                    this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Building Logical Handlers ", grainId);

                    // TODO : found a solution on the logical Pattern order

                    var updatedItem = new HashSet<BlackboardLogicalTypeHandler>();

                    foreach (var kvLogicalType in logicalPatternsRules)
                    {
                        var handler = this._logicalHandlers.FirstOrDefault(h => h.Equals(kvLogicalType.Key));

                        if (handler is null)
                            handler = new BlackboardLogicalTypeHandler(kvLogicalType.Key, this._repositoryFactory);

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

                        this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Update Logical Handlers {key}", grainId, kvLogicalType.Key);

                        handler.Update(this._blackboardDataLogicalTypeRuleValidatorProvider,
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

                    try
                    {
                        foreach (var toRm in toRemoves)
                            toRm.Dispose();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Clean Up old logical handlers failed -- {exception}", grainId, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.OptiLog(LogLevel.Debug, "[Blackboard {grainId}] -- Failed BuildFromTemplateAsync({blackboardTemplateUid}, {blackboardId}) : {exception}", grainId, blackboardTemplateUid, blackboardId, ex);
                throw;
            }
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<BlackboardId> GetIdentityAsync()
        {
            return Task.FromResult(this.State!.BlackboardId);
        }

        #region IBlackboardGrain

        /// <inheritdoc />
        public async Task ChangeRecordDataStatusAsync(Guid uid, RecordStatusEnum recordStatus, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequiredOrSealedStatus();
            await using (StartModifyContext())
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(string? logicTypeFilter,
                                                                                                                                     string? displayNameFilter,
                                                                                                                                     RecordStatusEnum? recordStatusFilter,
                                                                                                                                     uint? limit,
                                                                                                                                     GrainCancellationToken token,
                                                                                                                                     Guid? callContextId = null)
        {
            CheckInitializationRequired();

            var metaDatas = EnumerableHelper<BlackboardRecordMetadata>.Enumerable;

            this._metaDataLocker.EnterReadLock();
            try
            {
                var metaData = GetAllStoredMetaDatByFilters(logicTypeFilter, displayNameFilter, recordStatusFilter, limit);
                metaDatas = metaData.Select(m => m.Value);
            }
            finally
            {
                this._metaDataLocker.ExitReadLock();
            }

            return await GetAllStoredDataByMetaDataAsync<TDataProjection>(metaDatas, callContextId, token.CancellationToken);
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetStoredMetaDataAsync(IReadOnlyCollection<Guid> dataUids, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequired();

            this._metaDataLocker.EnterReadLock();
            try
            {
                var metaData = GetMetadataByIds(dataUids);

                var records = metaData.Select(kv => new MetaDataRecordContainer(kv.LogicalType,
                                                                                kv.Uid,
                                                                                kv.DisplayName,
                                                                                kv.ContainsType is not null ? ConcretBaseTypeConverter.ConvertFromSurrogate(kv.ContainsType!) : (ConcretType?)null,
                                                                                kv.Status,
                                                                                kv.UTCCreationTime,
                                                                                kv.CreatorIdentity,
                                                                                kv.UTCLastUpdateTime,
                                                                                kv.LastUpdaterIdentity,
                                                                                kv.CustomMetadata)).ToReadOnly();

                return Task.FromResult(records);
            }
            finally
            {
                this._metaDataLocker.ExitReadLock();
            }
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequired();
            return GetAllStoredMetaDataFilteredAsync(null, null, null, limit: null, token, callContextId);
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(string? logicTypeFilter,
                                                                                                    string? displayNameFilter,
                                                                                                    RecordStatusEnum? recordStatusFilter,
                                                                                                    uint? limit,
                                                                                                    GrainCancellationToken token,
                                                                                                    Guid? callContextId = null)
        {
            CheckInitializationRequired();

            this._metaDataLocker.EnterReadLock();
            try
            {
                var metaData = GetAllStoredMetaDatByFilters(logicTypeFilter, displayNameFilter, recordStatusFilter, limit);

                var records = metaData.Select(kv => new MetaDataRecordContainer(kv.Value.LogicalType,
                                                                                kv.Value.Uid,
                                                                                kv.Value.DisplayName,
                                                                                kv.Value.ContainsType is not null ? ConcretBaseTypeConverter.ConvertFromSurrogate(kv.Value.ContainsType!) : (ConcretType?)null,
                                                                                kv.Value.Status,
                                                                                kv.Value.UTCCreationTime,
                                                                                kv.Value.CreatorIdentity,
                                                                                kv.Value.UTCLastUpdateTime,
                                                                                kv.Value.LastUpdaterIdentity,
                                                                                kv.Value.CustomMetadata)).ToReadOnly();

                return Task.FromResult(records);
            }
            finally
            {
                this._metaDataLocker.ExitReadLock();
            }
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataFilteredAsync(ConditionExpressionDefinition filter, uint? limit, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequired();

            this._metaDataLocker.EnterReadLock();
            try
            {
                var metaData = GetAllStoredMetaDatByFilters(filter, limit);

                var records = metaData.Select(kv => new MetaDataRecordContainer(kv.Value.LogicalType,
                                                                                kv.Value.Uid,
                                                                                kv.Value.DisplayName,
                                                                                kv.Value.ContainsType is not null ? ConcretBaseTypeConverter.ConvertFromSurrogate(kv.Value.ContainsType!) : (ConcretType?)null,
                                                                                kv.Value.Status,
                                                                                kv.Value.UTCCreationTime,
                                                                                kv.Value.CreatorIdentity,
                                                                                kv.Value.UTCLastUpdateTime,
                                                                                kv.Value.LastUpdaterIdentity,
                                                                                kv.Value.CustomMetadata)).ToReadOnly();

                return Task.FromResult(records);
            }
            finally
            {
                this._metaDataLocker.ExitReadLock();
            }
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataFilteredAsync<TDataProjection>(ConditionExpressionDefinition filter,
                                                                                                                                     uint? limit,
                                                                                                                                     GrainCancellationToken token,
                                                                                                                                     Guid? callContextId = null)
        {
            CheckInitializationRequired();

            var metaDatas = EnumerableHelper<BlackboardRecordMetadata>.Enumerable;

            this._metaDataLocker.EnterReadLock();
            try
            {
                var metaData = GetAllStoredMetaDatByFilters(filter, limit);
                metaDatas = metaData.Select(m => m.Value);
            }
            finally
            {
                this._metaDataLocker.ExitReadLock();
            }

            return await GetAllStoredDataByMetaDataAsync<TDataProjection>(metaDatas, callContextId, token.CancellationToken);
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequired();
            return (await GetStoredDataImplAsync<TDataProjection>(token.CancellationToken, callContextId, dataUid.AsEnumerable().ToArray()))?.SingleOrDefault();
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetStoredDataAsync<TDataProjection>(IReadOnlyCollection<Guid> dataUids, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequired();
            return GetStoredDataImplAsync<TDataProjection>(token.CancellationToken, callContextId, dataUids);
        }

        /// <inheritdoc />
        public async Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequiredOrSealedStatus();

            await using (StartModifyContext())
            {
                return await CommandExecutionStartPointAsync(token.CancellationToken,
                                                             GetCallContextLogger(callContextId),
                                                             new BlackboardCommandStoragePrepareRecord(uid, logicType, displayName));
            }
        }

        /// <inheritdoc />
        public async Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequiredOrSealedStatus();
            await using (StartModifyContext())
            {
                return await CommandExecutionStartPointAsync(token.CancellationToken,
                                                             GetCallContextLogger(callContextId),
                                                             new BlackboardCommandStorageAddRecord<TData>(record,
                                                                                                          InsertIfNew: pushType != DataRecordPushRequestTypeEnum.UpdateOnly,
                                                                                                          @Override: pushType != DataRecordPushRequestTypeEnum.OnlyNew));
            }
        }

        /// <inheritdoc />
        public async Task FireQueryAsync(BlackboardQueryCommand command, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequiredOrSealedStatus();
            await SolveQueryRequestAsync<NoneType>(command, null, callContextId, token);
        }

        /// <inheritdoc />
        public async Task<BlackboardQueryResponse?> QueryAsync(BlackboardQueryCommand command, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequiredOrSealedStatus();
            var response = await SolveQueryRequestAsync<NoneType>(command, null, callContextId, token);
            return response;
        }

        /// <inheritdoc />
        public async Task<BlackboardQueryResponse> QueryAsync<TResponse>(BlackboardQueryRequest<TResponse> request, GrainCancellationToken token, Guid? callContextId = null)
        {
            CheckInitializationRequiredOrSealedStatus();
            var response = await SolveQueryRequestAsync<TResponse>(request, null, callContextId, token);
            return response!;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDataAsync(GrainCancellationToken token, Guid? callContextId, IIdentityCard identity, params Guid[] slotIds)
        {
            CheckInitializationRequiredOrSealedStatus();
            // TODO : check identity

            await using (StartModifyContext())
            {
                var ctx = new CommandExecutionContext(token.CancellationToken, GetCallContextLogger(callContextId));
                var rmCmds = slotIds.Select(uid => new BlackboardCommandStorageRemoveRecord(uid)).ToArray();

                return await CommandExecutionStartPointAsync(token.CancellationToken, rmCmds, ctx, ctx.Logger);
            }
        }

        /// <inheritdoc />
        public async Task ReceiveSignalAsync(SignalMessage message)
        {
            if (this._initializing == false &&
                this._sealing == false &&
                (this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.WaitingInitialization ||
                 this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.None ||
                 this.State!.CurrentLifeStatus == BlackboardLifeStatusEnum.Sealed))
            {
                this.Logger.OptiLog(LogLevel.Critical, "[Grain Id: {grainId}] Blackboard not ready for receiving signal messages {BlackboardStatus}", base.GetGrainId(), this.State!.CurrentLifeStatus);
                return;
            }

            var eventController = await GetControllerAsync<IBlackboardEventControllerGrain>(BlackboardControllerTypeEnum.Event, default);

            if (eventController is not null)
            {
                using (var grainCancelSource = this._lifetimeToken.ToGrainCancellationTokenSource())
                {
                    var cmds = await eventController.ManagedSignalMessageAsync(message, grainCancelSource.Token);

                    if (cmds is not null && cmds.Any())
                        await CommandExecutionStartPointAsync(grainCancelSource.Token.CancellationToken, cmds, null, this.Logger);
                }
            }
        }

        #endregion

        #region Tools

        private ILogger GetCallContextLogger(Guid? callContextId)
        {
            if (callContextId is null)
                return this.Logger;

            return this._loggerFactory.CreateLogger(string.Format("[Blackboard {0}][CallContextId {1}] ", this.GetPrimaryKey(), callContextId));
        }

        private static bool QueryRequestCommandFilter(BlackboardCommand c)
        {
            return c.ActionType == BlackboardCommandTypeEnum.Reject ||
                   c.ActionType == BlackboardCommandTypeEnum.Response ||
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
        private Task<bool> CommandExecutionStartPointAsync(CancellationToken token, ILogger logger, params BlackboardCommand[] commands)
        {
            return CommandExecutionStartPointAsync(token, commands, null, logger);
        }

        /// <summary>
        /// Commands the execution.
        /// </summary>
        private async Task<bool> CommandExecutionStartPointAsync(CancellationToken token, IReadOnlyCollection<BlackboardCommand> commands, CommandExecutionContext? parentContext, ILogger? logger)
        {
            await using (StartModifyContext())
            using (var execContext = parentContext is not null ? new CommandExecutionContext(parentContext) : new CommandExecutionContext(token, logger ?? this.Logger))
            {
                var result = await CommandExecutionerAsync(commands, execContext);
                token.ThrowIfCancellationRequested();

                var events = execContext.ConsumeEvents();

                if (events is not null && this.Logger.IsEnabled(LogLevel.Debug))
                {
                    foreach (var e in events)
                    {
                        this.Logger.OptiLog(LogLevel.Debug,
                                            "[BLACKBOARD] [EVENT] [{blackboardId}:{blackboardName} - {blacboardTemplateName}] {event}",
                                            this.State?.BlackboardId,
                                            this.State?.Name,
                                            this.State?.TemplateCopy?.UniqueTemplateName,
                                            e);
                    }
                }

                if (events is not null && events.Count > 0)
                {
                    var eventController = await GetControllerAsync<IBlackboardEventControllerGrain>(BlackboardControllerTypeEnum.Event, token, parentContext);

                    if (eventController is not null)
                    {
                        var eventActions = await eventController.ReactToEventsAsync(new BlackboardEventBook(events), token.ToGrainCancellationTokenSource().Token);

                        if (eventActions is not null && eventActions.Any())
                            await CommandExecutionStartPointAsync(token, eventActions, execContext, execContext.Logger);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Starts the modify context.
        /// </summary>
        private IAsyncDisposable StartModifyContext()
        {
            Interlocked.Increment(ref this._saveContextCounter);
            return new DisposableAsyncAction(EndModifyContext);
        }

        /// <summary>
        /// Saves the blackboard state.
        /// </summary>
        private async ValueTask EndModifyContext()
        {
            if (Interlocked.Decrement(ref this._saveContextCounter) > 0)
                return;

            if (this._needSaving)
            {
                await PushStateAsync(this._lifetimeToken.Token);
                this._needSaving = false;
            }
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
                                                                                                       RecordStatusEnum? recordStatusFilter,
                                                                                                       uint? limit)
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

            if (limit is not null && limit >= 1)
                metaData = metaData.Take((int)limit);

            return metaData;
        }

        /// <summary>
        /// Gets all stored meta dat by filters.
        /// </summary>
        private IEnumerable<KeyValuePair<Guid, BlackboardRecordMetadata>> GetAllStoredMetaDatByFilters(ConditionExpressionDefinition conditionExpression, uint? limit)
        {
            CheckInitializationRequired();

            IEnumerable<KeyValuePair<Guid, BlackboardRecordMetadata>> metaData = this.State!.Registry.RecordMetadatas;

            var filter = conditionExpression.ToExpression<BlackboardRecordMetadata, bool>().Compile();

            var filterMetas = metaData.Where(kv => filter(kv.Value));

            if (limit is not null && limit >= 1)
                filterMetas = filterMetas.Take((int)limit);

            return filterMetas;
        }

        /// <inheritdoc />
        protected override async Task OnActivationSetupState(BlackboardGrainState? state, CancellationToken ct)
        {
            if (this._loggerFactory is not NullLoggerFactory)
            {
                var grainId = base.GetGrainId().GetGuidKey();
                this.Logger = this._loggerFactory.CreateLogger("[Blackboard {0} name: {1} template: {2}] ".WithArguments(grainId, this.State?.Name, this.State?.TemplateCopy?.Uid));
            }

            if ((this._controllers.Count == 0 || this._logicalHandlers.Count == 0) && state is not null && state.TemplateCopy is not null)
            {
                try
                {
                    await BuildFromTemplateAsync(state.TemplateCopy.Uid, state.BlackboardId, ct.ToGrainCancellationTokenSource().Token, null);
                    ct.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    this.Logger.OptiLog(LogLevel.Critical, "TIMEOUT ON BUILD {grainId}", base.GetGrainId().GetGuidKey());
                }
            }

            await base.OnActivationSetupState(state, ct);
        }

        /// <inheritdoc cref="IBlackboard.GetStoredDataAsync{TDataProjection}(Guid, GrainCancellationToken)"/>
        private async Task<IReadOnlyCollection<DataRecordContainer<TData?>>> GetStoredDataImplAsync<TData>(CancellationToken token, Guid? callContextId, IReadOnlyCollection<Guid> uids)
        {
            CheckInitializationRequired();

            this._metaDataLocker.EnterReadLock();

            BlackboardRecordMetadata[] metaDatas;
            try
            {
                metaDatas = GetMetadataByIds(uids);

                if (!metaDatas.Any())
                    return EnumerableHelper<DataRecordContainer<TData?>>.ReadOnly;
            }
            finally
            {
                this._metaDataLocker.ExitReadLock();
            }
            return await GetAllStoredDataByMetaDataAsync<TData>(metaDatas, callContextId, token);
        }

        /// <inheritdoc />
        private async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByMetaDataAsync<TDataProjection>(IEnumerable<BlackboardRecordMetadata> metaDatas,
                                                                                                                                        Guid? callContextId,
                                                                                                                                        CancellationToken token)
        {
            CheckInitializationRequired();

            var logger = GetCallContextLogger(callContextId);

            try
            {

                logger.OptiLog(LogLevel.Debug, "-- Get repository");

                var metaDatasWithRepoTasks = metaDatas.Select(kv => (RepoTask: GetDedicatedRepositoryAsync(kv.LogicalType, token), kv))
                                                      .ToArray();

                await (metaDatasWithRepoTasks.Select(t => t.RepoTask).ToArray()).SafeWhenAllAsync(token);

                var fetchTasks = metaDatasWithRepoTasks.GroupBy(kv => kv.RepoTask.Result)
                                                       .Select(kv => kv.Key.GetValueByIdAsync(kv.Select(k => k.kv.Uid).Distinct().ToArray(), token).AsTask())
                                                       .ToArray();

                await fetchTasks.SafeWhenAllAsync(token);

                var indexedResults = fetchTasks.Where(f => f.IsCompletedSuccessfully && f.Result is not null)
                                        .SelectMany(f => f.Result)
                                        .Distinct()
                                        .GroupBy(k => k.Uid)
                                        .ToDictionary(k => k.Key, v => v.OrderByDescending(vv => vv.UTCLastUpdateTime).First());

                var resultProject = new List<DataRecordContainer<TDataProjection?>>();

                foreach (var metadata in metaDatas)
                {
                    DataRecordContainer? container = null;

                    // If no data have been record in the repository storage then send at lead the metadata information
                    if (!indexedResults.TryGetValue(metadata.Uid, out container))
                    {
                        var newContainer = new DataRecordContainer<TDataProjection>(metadata.LogicalType,
                                                                                    metadata.Uid,
                                                                                    metadata.DisplayName,
                                                                                    default,
                                                                                    metadata.Status,
                                                                                    metadata.UTCCreationTime,
                                                                                    metadata.CreatorIdentity,
                                                                                    metadata.UTCLastUpdateTime,
                                                                                    metadata.LastUpdaterIdentity,
                                                                                    metadata.CustomMetadata);

                        resultProject.Add(newContainer!);
                        continue;
                    }

                    if (container.TryProjectTo<TDataProjection>(this._converter, out var alreadyWellTyped) && alreadyWellTyped is not null)// is DataRecordContainer<TDataProjection?> alreadyWellTyped)
                    {
                        resultProject.Add(alreadyWellTyped!);
                        continue;
                    }

                    throw new NotSupportedException("Data projection is not yet supported");
                }

                return resultProject;
            }
            catch (Exception ex)
            {
                logger.OptiLog(LogLevel.Debug, "-- Get Data Failed {exception}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the controller after initialization if needed
        /// </summary>
        private async ValueTask<TSpecializedController?> GetControllerAsync<TSpecializedController>(BlackboardControllerTypeEnum controllerType, CancellationToken token, CommandExecutionContext? parentContext = null)
            where TSpecializedController : IBlackboardBaseControllerGrain
        {
            if (this._controllers.TryGetValue(controllerType, out var controller))
            {
                var iniCmds = await controller.GetController<TSpecializedController>(token);

                if (iniCmds.InitControllerActions is not null && iniCmds.InitControllerActions.Any())
                    await CommandExecutionStartPointAsync(token, iniCmds.InitControllerActions, parentContext, parentContext?.Logger);

                return iniCmds.Controller;
            }

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
            if (processors.TryGetValue(getKeyFromCommand(command), out var actions))
                return await actions(grain, command, context);

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
        /// Method that managed message attach and dettach action
        /// </summary>
        private async Task<bool> CommandExecuteSignalAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
            if (cmd is BlackboardCommandSignalSubscription signalCmd)
            {
                if (signalCmd.Type == BlackboardCommandSignalTypeEnum.Attach)
                {
                    SubscriptionId subscription;

                    if (signalCmd.IsDoor)
                        subscription = await this._signalService.SubscribeAsync(new DoorId(signalCmd.SignalUid, signalCmd.DisplayName), this, ctx.CancellationToken);
                    else
                        subscription = await this._signalService.SubscribeAsync(new SignalId(signalCmd.SignalUid, signalCmd.DisplayName), this, ctx.CancellationToken);

                    this.State!.AddSubscription(subscription);
                }
                else
                {
                    var subscription = this.State!.GetSubscription(signalCmd.SignalUid);

                    if (subscription is null)
                        return false;

                    await this._signalService.UnsubscribeAsync(subscription.Value, ctx.CancellationToken);
                    this.State!.RemoveSubscription(subscription.Value);
                }

                return true;
            }
            return false;
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
                var controller = await GetControllerAsync<IBlackboardStorageControllerGrain>(BlackboardControllerTypeEnum.Storage, ctx.CancellationToken, ctx);

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
            var tsk = (Task)s_solveGenericQueryRequest.MakeGenericMethod(response.ToType()).Invoke(grain, new object?[] { request, deferredId, request.QueryUid, token })!;
            await tsk;

            return tsk.GetResult<BlackboardQueryResponse>()!;
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task<BlackboardQueryResponse?> SolveQueryRequestAsync<TResponse>(BlackboardBaseQuery query, DeferredId? deferredId, Guid? callContextId, GrainCancellationToken token)
        {
            CheckInitializationRequiredOrSealedStatus();

            var logger = GetCallContextLogger(callContextId);

            try
            {
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

                if (result.TryGetValue(BlackboardCommandTypeEnum.Response, out var direct) && direct is BlackboardCommandResponse<TResponse> reponseCmd)
                {
                    queryResponse = new BlackboardQueryDirectResponse<TResponse>(reponseCmd.Response, queryUid);
                    if (runningDeferred is not null)
                        await this._deferredHandler.FinishDeferredWorkWithDataAsync(runningDeferred.Value.DeferredId.Uid, this.IdentityCard, reponseCmd.Response);
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
                    this._needSaving |= true;

                // Check remain command that are not dedicated to query result
                // Those remain command are executed at the end to prevent any deadlock loop due to a command that could retry the deferred request
                var remainActions = ctrlResponses.Where(c => !QueryRequestCommandFilter(c))
                                                 .ToArray();

                if (remainActions.Any())
                    await this.CommandExecutionStartPointAsync(token.CancellationToken, logger, remainActions);

                if (queryResponse is null && (query.ExpectedResponseType is not null || query.ExpectedResponseType != NoneType.AbstractTrait))
                    throw new InvalidOperationException(string.Format("[Blackboard] [Query: {0}] - No Response - Expect response type {1}", query.QueryUid, query.ExpectedResponseType));

                return queryResponse;
            }
            catch (Exception ex)
            {
                logger.OptiLog(LogLevel.Error, "Query Failed -- {exception}", ex);
                throw;
            }
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

            if (status == BlackboardLifeStatusEnum.None || status == BlackboardLifeStatusEnum.WaitingInitialization)
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

        /// <summary>
        /// Gets the metadata by ids.
        /// </summary>
        private BlackboardRecordMetadata[] GetMetadataByIds(IReadOnlyCollection<Guid> uids)
        {
            return uids.Select(uid =>
            {
                if (!this.State!.Registry.RecordMetadatas.TryGetValue(uid, out var data))
                    return (BlackboardRecordMetadata?)null;
                return data;
            }).Where(d => d is not null)
              .Select(d => d!.Value)
              .ToArray();
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
            IRepository<DataRecordContainer, Guid>? repository = null;

            if (!NoneType.IsEqualTo<TData>())
                repository = await GetDedicatedRepositoryAsync(record.LogicalType, ctx.CancellationToken);

            try
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();

                if (record.Uid == Guid.Empty)
                    record.ForceUid(Guid.NewGuid());

                if (!NoneType.IsEqualTo<TData>() && !await repository!.PushDataRecordAsync(record, true, ctx.CancellationToken))
                    throw new BlackboardPushException(record, "Push failed", null);

                this._metaDataLocker.EnterWriteLock();

                try
                {
                    var entry = this.State!.Registry.Push(record, this._timeManager);
                    this._needSaving |= true;

                    ctx.EnqueueEvent(new BlackboardEventStorage(BlackboardEventStorageTypeEnum.Add, entry.Uid, entry));
                }
                finally
                {
                    this._metaDataLocker.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                successResult = true;
                throw new BlackboardPushException(record, ex.Message, ex);
            }

            return successResult;
        }

        /// <summary>
        /// Commands the prepare slot from storage asynchronous.
        /// </summary>
        private Task<bool> Command_PrepareSlotFromStorageAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            var prepareCommand = cmd as BlackboardCommandStoragePrepareRecord;
            ArgumentNullException.ThrowIfNull(prepareCommand);

            return Command_AddToStorageAsync<NoneType>(new BlackboardCommandStorageAddRecord<NoneType?>(new DataRecordContainer<NoneType?>(prepareCommand.LogicType,
                                                                                                                                           prepareCommand.Uid,
                                                                                                                                           prepareCommand.DisplayName,
                                                                                                                                           null,
                                                                                                                                           RecordStatusEnum.Preparation,
                                                                                                                                           this._timeManager.UtcNow,
                                                                                                                                           null,
                                                                                                                                           this._timeManager.UtcNow,
                                                                                                                                           null,
                                                                                                                                           null), true, false), ctx);
        }

        /// <summary>
        /// Command in charge to remove a record from the storage
        /// </summary>
        private async Task<bool> Command_RemoveFromStorageAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            var rmCommand = cmd as BlackboardCommandStorageRemoveRecord;

            ArgumentNullException.ThrowIfNull(rmCommand);

            BlackboardRecordMetadata recordMetadata = default;

            this._metaDataLocker.EnterWriteLock();
            try
            {
                if (!this.State!.Registry.RecordMetadatas.TryGetValue(rmCommand.Uid, out recordMetadata))
                    return false;
            }
            finally
            {
                this._metaDataLocker.ExitWriteLock();
            }

            try
            {
                var repository = await GetDedicatedRepositoryAsync(recordMetadata.LogicalType, ctx.CancellationToken);

                ctx.CancellationToken.ThrowIfCancellationRequested();

                if (!await repository.DeleteRecordAsync(recordMetadata.Uid, ctx.CancellationToken))
                {
                    if (recordMetadata.Status == RecordStatusEnum.Ready)
                        throw new BlackboardCommandExecutionException(cmd, "Remove failed", null);
                }

                this._metaDataLocker.EnterWriteLock();
                try
                {
                    var removed = this.State!.Registry.Pop(recordMetadata.Uid);

                    if (removed is not null)
                        this._needSaving |= true;

                    ctx.EnqueueEvent(new BlackboardEventStorage(BlackboardEventStorageTypeEnum.Remove, rmCommand.Uid, removed));
                }
                finally
                {
                    this._metaDataLocker.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                throw new BlackboardCommandExecutionException(cmd, ex.Message, ex);
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
        private Task<bool> Command_ChangeMetaDataFromStorageAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            var chgCommand = cmd as BlackboardCommandStorageChangeMetaData;

            ArgumentNullException.ThrowIfNull(chgCommand);

            this._metaDataLocker.EnterWriteLock();
            try
            {
                var utcNow = this._timeManager.UtcNow;

                var newItemMetaData = this.State!.Registry.ChangeMetaData(chgCommand.Uid, chgCommand.NewMetaData, utcNow);
                if (newItemMetaData is null)
                    return TaskHelper.GetFromResultCache(false);

                ctx.EnqueueEvent(new BlackboardEventStorage(BlackboardEventStorageTypeEnum.ChangeStatus, chgCommand.Uid, newItemMetaData));

                return TaskHelper.GetFromResultCache(true);
            }
            finally
            {
                this._metaDataLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Command in charge to change status a record from the storage
        /// </summary>
        private async Task<bool> Command_ChangeStatusFromStorageAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            var chgCommand = cmd as BlackboardCommandStorageChangeStatusRecord;

            ArgumentNullException.ThrowIfNull(chgCommand);

            BlackboardRecordMetadata recordMetadata = default;

            this._metaDataLocker.EnterReadLock();
            try
            {
                if (!this.State!.Registry.RecordMetadatas.TryGetValue(chgCommand.Uid, out recordMetadata))
                    return false;
            }
            finally
            {
                this._metaDataLocker.ExitReadLock();
            }

            try
            {
                var repository = await GetDedicatedRepositoryAsync(recordMetadata.LogicalType, ctx.CancellationToken);

                ctx.CancellationToken.ThrowIfCancellationRequested();

                var item = await repository.GetValueByIdAsync(chgCommand.Uid, ctx.CancellationToken);

                if (item == null || item.Status == chgCommand.NewRecordStatus)
                    return false;

                var utcNow = this._timeManager.UtcNow;
                var newItem = item.WithNewStatus(chgCommand.NewRecordStatus, utcNow);

                if (newItem is not null)
                {
                    var pushResult = await repository.PushDataRecordAsync(newItem, false, ctx.CancellationToken);
                    if (pushResult)
                        this._needSaving |= true;
                }

                this._metaDataLocker.EnterWriteLock();
                try
                {
                    var newItemMetaData = this.State!.Registry.ChangeStatus(recordMetadata.Uid, chgCommand.NewRecordStatus, utcNow);
                    ctx.EnqueueEvent(new BlackboardEventStorage(BlackboardEventStorageTypeEnum.ChangeStatus, chgCommand.Uid, newItemMetaData));
                }
                finally
                {
                    this._metaDataLocker.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                throw new BlackboardCommandExecutionException(cmd, ex.Message, ex);
            }

            return true;
        }

        /// <summary>
        /// Trigger a seqeunce based on configuration pass in arguments
        /// </summary>
        private async Task<bool> Command_TriggerSequence<TInput>(BlackboardCommandTriggerSequence<TInput> cmd, CommandExecutionContext ctx)
        {
            CheckInitializationRequiredOrSealedStatus();

            IExecutionFlowLauncher? exec = null;

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
        /// Commands the trigger message.
        /// </summary>
        private async Task<bool> Command_TriggerSignal(BlackboardCommandTriggerSignal cmd, CommandExecutionContext ctx)
        {
            if (cmd.CarryData)
                return await CommandGenericCall(this, cmd, ctx, s_cmdSignalWithData);

            var fireId = await this._signalService.Fire(cmd.SignalId, ctx.CancellationToken);
            return fireId != Guid.Empty;
        }

        /// <summary>
        /// Commands the trigger message.
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
            if (this.State!.CurrentLifeStatus != BlackboardLifeStatusEnum.WaitingInitialization && this.State!.CurrentLifeStatus != BlackboardLifeStatusEnum.None)
                return false;

            this._initializing = true;
            try
            {
                BlackboardCommand[]? initCommands = null;

                var stateController = await GetControllerAsync<IBlackboardStateControllerGrain>(BlackboardControllerTypeEnum.State, ctx.CancellationToken, ctx);

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
                    await this.CommandExecutionStartPointAsync(ctx.CancellationToken, initCommands, ctx, ctx.Logger);

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
        private Task<bool> Command_LifeStatus_Changed(BlackboardCommandLifeStatusChange cmd, CommandExecutionContext ctx)
        {
            if (cmd.OldStatus != this.State!.CurrentLifeStatus)
                throw new InvalidOperationException("Invalid previous state");

            var old = this.State!.CurrentLifeStatus;
            this.State!.CurrentLifeStatus = cmd.NewStatus;

            this._needSaving |= old != cmd.NewStatus;

            ctx.EnqueueEvent(new BlackboardEventLifeStatusChanged(cmd.NewStatus, cmd.OldStatus));

            return Task.FromResult(true);
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
                this._metaDataLocker.EnterReadLock();

                try
                {
                    metaDatas = this.State!.Registry.RecordMetadatas.Values.ToArray();
                }
                finally
                {
                    this._metaDataLocker.ExitReadLock();
                }

                var logicalTypeToRemain = this._logicalHandlers.Where(h => h.RemainOnSealed)
                                                               .ToArray();

                var toKeepOnSealed = metaDatas.Where(m => m.Status == RecordStatusEnum.Ready && logicalTypeToRemain.Any(h => h.Match(m.LogicalType)))
                                              .ToArray();

                var toRemove = metaDatas.Except(toKeepOnSealed)
                                        .ToArray();

                var stateController = await GetControllerAsync<IBlackboardStateControllerGrain>(BlackboardControllerTypeEnum.State, ctx.CancellationToken, ctx);

                List<BlackboardCommand>? sealingCommands = null;

                if (stateController is not null)
                {
                    using (var tokenSource = ctx.CancellationToken.ToGrainCancellationTokenSource())
                    {
                        sealingCommands = (await stateController.OnSealed(toKeepOnSealed, toRemove, tokenSource.Token))?.ToList<BlackboardCommand>();
                    }
                }
                else
                {
                    sealingCommands = toRemove.Select(t => new BlackboardCommandStorageRemoveRecord(t.Uid)).ToList<BlackboardCommand>();
                }

                foreach (var signalSubscription in this.State!.GetSubscriptions())
                {
                    sealingCommands = sealingCommands.AddOnNull((BlackboardCommand)new BlackboardCommandSignalSubscription(signalSubscription.SignalId,
                                                                                                               "",
                                                                                                               signalSubscription.FromDoor,
                                                                                                               BlackboardCommandSignalTypeEnum.Detach));
                }

                if (sealingCommands is not null)
                    await this.CommandExecutionStartPointAsync(ctx.CancellationToken, sealingCommands, ctx, ctx.Logger);

                return true;
            }
            finally
            {
                this._sealing = false;
            }
        }

        #endregion

        /// <inheritdoc />
        protected override void DisposeResourcesBegin()
        {
            this._lifetimeToken.Cancel();
            base.DisposeResourcesBegin();
        }

        /// <inheritdoc />
        protected override void DisposeResourcesEnd()
        {
            this._metaDataLocker.Dispose();

            base.DisposeResourcesEnd();
        }

        #endregion

        #endregion
    }
}
