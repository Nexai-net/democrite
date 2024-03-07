// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Events;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Rules;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;
    using Democrite.Framework.Node.Blackboard.Models;
    using Democrite.Framework.Node.Blackboard.Models.Surrogates;
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Abstractions.Models;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Models;
    using Democrite.Framework.Toolbox.Services;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
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

        private static readonly MethodInfo s_triggerSequenceCommandToStorage;
        private static readonly MethodInfo s_addCommandToStorage;

        private static readonly IConcretTypeSurrogate s_defaultControllerConcretTypeSurrogate;
        private static readonly ConcretType s_defaultControllerConcretType;
        private static readonly Type s_defaultController;

        private readonly IBlackboardDataLogicalTypeRuleValidatorProvider _blackboardDataLogicalTypeRuleValidatorProvider;
        private readonly Dictionary<BlackboardControllerTypeEnum, BlackboardControllerHandler> _controllers;
        private readonly IDemocriteExecutionHandler _democriteExecutionHandler;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGrainFactory _grainFactory;
        private readonly IObjectConverter _converter;
        private readonly ITimeManager _timeManager;

        private readonly CancellationTokenSource _lifetimeToken;
        private readonly DelayTimer _delaySaving;

        // Extra protection to ensure metadata are always synchronized with data stored
        private readonly SemaphoreSlim _metaDataLocker;
        private readonly SortedSet<BlackboardLogicalTypeHandler> _logicalHandlers;

        private TaskScheduler? _activationScheduler;
        private int _saveCounter;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="VGrainBase{TVGrainState, TStateSurrogate, TConverter}" /> class.
        /// </summary>
        static BlackboardGrain()
        {
            s_defaultController = typeof(IDefaultBlackboardControllerGrain);
            s_defaultControllerConcretType = (ConcretType)s_defaultController.GetAbstractType();
            s_defaultControllerConcretTypeSurrogate = ConcretBaseTypeConverter.Default.ConvertToSurrogate(s_defaultControllerConcretType);

            var addCommandToStorage = typeof(BlackboardGrain).GetMethod(nameof(Command_AddToStorageAsync), BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(addCommandToStorage != null);
            s_addCommandToStorage = addCommandToStorage;

            var triggerSequenceCommandToStorage = typeof(BlackboardGrain).GetMethod(nameof(Command_TriggerSequence), BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(triggerSequenceCommandToStorage != null);
            s_triggerSequenceCommandToStorage = triggerSequenceCommandToStorage;

            s_rootCommandExecutor = new Dictionary<BlackboardCommandTypeEnum, Func<BlackboardGrain, BlackboardCommand, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardCommandTypeEnum.Storage, CommandExecuteStorageAsync },
                { BlackboardCommandTypeEnum.Trigger, CommandExecuteTriggerAsync },
                { BlackboardCommandTypeEnum.Reject, Command_ExecuteRejectAsync },
            };

            s_rootCommandStorageExecutor = new Dictionary<BlackboardCommandStorageActionTypeEnum, Func<BlackboardGrain, BlackboardCommandStorage, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardCommandStorageActionTypeEnum.Add, (g, cmd, ctx) => CommandGenericCall(g, cmd, ctx, s_addCommandToStorage) },
                { BlackboardCommandStorageActionTypeEnum.Remove, (g, cmd, ctx) => g.Command_RemoveFromStorageAsync(cmd, ctx) },
                { BlackboardCommandStorageActionTypeEnum.Decommission, (g, cmd, ctx) => g.Command_DecommissionFromStorageAsync(cmd, ctx) },
                { BlackboardCommandStorageActionTypeEnum.ChangeStatus, (g, cmd, ctx) => g.Command_ChangeStatusFromStorageAsync(cmd, ctx) },
            };

            s_rootCommandTriggerExecutor = new Dictionary<BlackboardCommandTriggerActionTypeEnum, Func<BlackboardGrain, BlackboardCommandTrigger, CommandExecutionContext, Task<bool>>>()
            {
                { BlackboardCommandTriggerActionTypeEnum.Sequence, (g, cmd, ctx) => CommandGenericCall(g, cmd, ctx, s_triggerSequenceCommandToStorage) }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardGrain"/> class.
        /// </summary>
        public BlackboardGrain(ILogger<IBlackboardGrain> logger,
                               [PersistentState(BlackboardConstants.BlackboardStorageStateKey, BlackboardConstants.BlackboardStorageConfigurationKey)] IPersistentState<BlackboardGrainStateSurrogate> persistentState,
                               IGrainFactory grainFactory,
                               IServiceProvider serviceProvider,
                               ITimeManager timeManager,
                               IRepositoryFactory repositoryFactory,
                               IBlackboardDataLogicalTypeRuleValidatorProvider blackboardDataLogicalTypeRuleValidatorProvider,
                               IDemocriteExecutionHandler democriteExecutionHandler,
                               IObjectConverter converter)

            : base(logger, persistentState)
        {
            this._democriteExecutionHandler = democriteExecutionHandler;
            this._lifetimeToken = new CancellationTokenSource();
            this._controllers = new Dictionary<BlackboardControllerTypeEnum, BlackboardControllerHandler>();

            this._delaySaving = DelayTimer.Create(DelayPushStateAsync, lifeTimeToken: this._lifetimeToken.Token, startDelay: TimeSpan.FromSeconds(2));
            this._metaDataLocker = new SemaphoreSlim(1);

            this._repositoryFactory = repositoryFactory;
            this._grainFactory = grainFactory;
            this._serviceProvider = serviceProvider;
            this._timeManager = timeManager;
            this._blackboardDataLogicalTypeRuleValidatorProvider = blackboardDataLogicalTypeRuleValidatorProvider;

            this._converter = converter;

            this._logicalHandlers = new SortedSet<BlackboardLogicalTypeHandler>(BlackboardLogicalTypeHandlerComparer.Default);
        }

        #endregion

        #region Methods

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

                this.State!.BuildUsingTemplate(tmpl, blackboardId);
                await PushStateAsync(default);
            }

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

                    var controllerGrain = this._grainFactory.GetGrain(grainDefType, this.GetPrimaryKey()).AsReference<IBlackboardBaseControllerGrain>()!;

                    var key = (controllerGrain.GetGrainId(), controllerDefinition.Options);

                    BlackboardControllerHandler? handler = null;
                    initController.TryGetValue(key, out handler);

                    if (handler == null)
                    {
                        handler = new BlackboardControllerHandler(this.State.BlackboardId.Uid,
                                                                  grainDefType,
                                                                  this._grainFactory,
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

                    rules = rules.Where(r => r != order && r != storage).ToArray();

                    handler.Update(this._repositoryFactory,
                                   this._blackboardDataLogicalTypeRuleValidatorProvider,
                                   order,
                                   storage?.Storage ?? this.State.TemplateCopy.DefaultStorageConfig,
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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByTypeAsync<TDataProjection>(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? recordStatusFilter, GrainCancellationToken token)
        {
            var metaData = GetAllStoredMetaDatByFilters(logicTypeFilter, displayNameFilter, recordStatusFilter);
            return await GetAllStoredDataByMetaDataAsync<TDataProjection>(metaData.Select(m => m.Value), token.CancellationToken);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataAsync(GrainCancellationToken token)
        {
            return GetAllStoredMetaDataByTypeAsync(null, null, null, token);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllStoredMetaDataByTypeAsync(string? logicTypeFilter, string? displayNameFilter, RecordStatusEnum? recordStatusFilter, GrainCancellationToken token)
        {
            var metaData = GetAllStoredMetaDatByFilters(logicTypeFilter, displayNameFilter, recordStatusFilter);

            return Task.FromResult(metaData.Select(kv => new MetaDataRecordContainer(kv.Value.LogicalType,
                                                                                     kv.Value.Uid,
                                                                                     kv.Value.DisplayName,
                                                                                     kv.Value.ContainsType is not null ? ConcretBaseTypeConverter.Default.ConvertFromSurrogate(kv.Value.ContainsType!) : (ConcretType?)null,
                                                                                     kv.Value.Status,
                                                                                     kv.Value.UTCCreationTime,
                                                                                     kv.Value.CreatorIdentity,
                                                                                     kv.Value.UTCLastUpdateTime,
                                                                                     kv.Value.LastUpdaterIdentity)).ToReadOnly());
        }

        /// <inheritdoc />
        public Task<DataRecordContainer<TDataProjection?>?> GetStoredDataAsync<TDataProjection>(Guid dataUid, GrainCancellationToken token)
        {
            return GetStoredDataImplAsync<TDataProjection>(dataUid, token.CancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> PrepareDataSlotAsync(Guid uid, string logicType, string displayName, GrainCancellationToken token)
        {
            return await PushDataAsync<NoneType>(new DataRecordContainer<NoneType?>(logicType,
                                                                                    uid,
                                                                                    displayName,
                                                                                    null,
                                                                                    RecordStatusEnum.Preparation,
                                                                                    this._timeManager.UtcNow,
                                                                                    null,
                                                                                    this._timeManager.UtcNow,
                                                                                    null),
                                                 DataRecordPushRequestTypeEnum.OnlyNew,
                                                 token);
        }

        /// <inheritdoc />
        public Task<bool> PushDataAsync<TData>(DataRecordContainer<TData?> record, DataRecordPushRequestTypeEnum pushType, GrainCancellationToken token)
        {
            return CommandExecutionStartPointAsync(token.CancellationToken,
                                                   new BlackboardCommandStorageAddRecord<TData>(record,
                                                                                                InsertIfNew: pushType != DataRecordPushRequestTypeEnum.UpdateOnly,
                                                                                                @Override: pushType != DataRecordPushRequestTypeEnum.OnlyNew));
        }

        #endregion

        #region Tools

        /// <summary>
        /// Resove call generic execution
        /// </summary>
        private static async Task<bool> CommandGenericCall(BlackboardGrain grain, BlackboardCommand cmd, CommandExecutionContext ctx, MethodInfo s_commandExec)
        {
            var mthd = s_commandExec.MakeGenericMethod(cmd.GetType().GetGenericArguments());
            return await (Task<bool>)mthd.Invoke(grain, new object[] { cmd, ctx })!;
        }

        /// <summary>
        /// Commands the execution.
        /// </summary>
        private async Task<bool> CommandExecutionStartPointAsync(CancellationToken token, params BlackboardCommand[] commands)
        {
            using (var execContext = new CommandExecutionContext(token))
            {
                var result = await CommandExecutionerAsync(commands, execContext);
                token.ThrowIfCancellationRequested();

                var events = execContext.Events?.ToArray();

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

                if (result)
                {
                    // Force save all the 10 push this is to prevent any data lost
                    if (Interlocked.Increment(ref this._saveCounter) > 10)
                    {
                        await this._delaySaving.StopAsync(token);
                        await DelayPushStateAsync(token);
                    }

                    // otherwise restart the timer to delay the save

                    // Restart in fire and forget mode the timer to save to prevent save state each time
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() => this._delaySaving.StartAsync()).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }

                if (events is not null && events.Length > 0)
                {
                    var eventController = await GetControllerAsync<IBlackboardEventControllerGrain>(BlackboardControllerTypeEnum.Event, token);

                    if (eventController is not null)
                    {
                        var eventActions = await eventController.ReactToEventsAsync(events, token.ToGrainCancellationToken().Token);

                        if (eventActions is not null && eventActions.Any())
                            await CommandExecutionStartPointAsync(token, eventActions.ToArray());
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Commands the execution.
        /// </summary>
        private async Task<bool> CommandExecutionerAsync(IReadOnlyCollection<BlackboardCommand> commands, CommandExecutionContext context)
        {
            var localCmdExecContext = new CommandExecutionContext(context);

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
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                // TODO : Apply rollback
                throw new BlackboardCommandExecutionException(cmd, exception?.Message ?? string.Empty, exception);
            }

            return true;
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
        /// Override this activato get the activation context scheduler
        /// </summary>
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            this._activationScheduler = TaskScheduler.Current;
            return base.OnActivateAsync(cancellationToken);
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
                Interlocked.Exchange(ref this._saveCounter, 0);
                await PushStateAsync(token);

            }, token, TaskCreationOptions.None, scheduler: this._activationScheduler!);
        }

        /// <inheritdoc cref="IBlackboard.GetStoredDataAsync{TDataProjection}(Guid, GrainCancellationToken)"/>
        private async Task<DataRecordContainer<TData?>?> GetStoredDataImplAsync<TData>(Guid uid, CancellationToken token)
        {
            if (!this.State!.Registry.RecordMetadatas.TryGetValue(uid, out var data))
                return null;

            var result = await GetAllStoredDataByMetaDataAsync<TData>(data.AsEnumerable(), token);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        private async Task<IReadOnlyCollection<DataRecordContainer<TDataProjection?>>> GetAllStoredDataByMetaDataAsync<TDataProjection>(IEnumerable<BlackboardRecordMetadata> metaDatas,
                                                                                                                                        CancellationToken token)
        {
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
        private static async Task<bool> CommandExecuteStorageAsync(BlackboardGrain grain,
                                                                   BlackboardCommand command,
                                                                   CommandExecutionContext context)
        {
            if (command is BlackboardCommandStorage storageCmd && s_rootCommandStorageExecutor.TryGetValue(storageCmd.StorageAction, out var storageAction))
                return await storageAction(grain, storageCmd, context);

            throw new KeyNotFoundException($"[Blacboard] No action executor for type {command.ToDebugDisplayName()}");
        }

        /// <summary>
        /// Commands the execute event asynchronous.
        /// </summary>
        private static async Task<bool> CommandExecuteTriggerAsync(BlackboardGrain grain, BlackboardCommand command, CommandExecutionContext context)
        {
            if (command is BlackboardCommandTrigger triggerCmd && s_rootCommandTriggerExecutor.TryGetValue(triggerCmd.TriggerActionType, out var triggerAction))
                return await triggerAction(grain, triggerCmd, context);

            throw new KeyNotFoundException($"[Blacboard] No action executor for type {command.ToDebugDisplayName()}");
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
                        using (var grainCancelToken = ctx.CancellationToken.ToGrainCancellationToken())
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

        #region Command Actions

        /// <summary>
        /// Call after all validation and conflic resolved to save the data itself
        /// </summary>
        private async Task<bool> Command_AddToStorageAsync<TData>(BlackboardCommandStorageAddRecord<TData?> cmd, CommandExecutionContext ctx)
        {
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
            var decoCommand = cmd as BlackboardCommandStorageDecommissionRecord;

            ArgumentNullException.ThrowIfNull(decoCommand);

            return Command_ChangeStatusFromStorageAsync(new BlackboardCommandStorageChangeStatusRecord(decoCommand.Uid, RecordStatusEnum.Decommissioned), ctx);
        }

        /// <summary>
        /// Command in charge to change status a record from the storage
        /// </summary>
        private async Task<bool> Command_ChangeStatusFromStorageAsync(BlackboardCommand cmd, CommandExecutionContext ctx)
        {
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
        /// Command called when the controller decided to reject the primary action
        /// </summary>
        private static Task<bool> Command_ExecuteRejectAsync(BlackboardGrain grain, BlackboardCommand command, CommandExecutionContext context)
        {
            grain.Logger.OptiLog(LogLevel.Warning, "Command rejected by the controller after the folling issue {issue}", ((RejectActionBlackboardCommand)command).SourceIssue);
            return Task.FromResult(false);
        }

        /// <summary>
        /// Trigger a seqeunce based on configuration pass in arguments
        /// </summary>
        private async Task<bool> Command_TriggerSequence<TInput>(BlackboardCommandTriggerSequence<TInput> cmd, CommandExecutionContext ctx)
        {
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

        #endregion

        #endregion

        #endregion
    }
}
