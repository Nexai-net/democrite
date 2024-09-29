// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Triggers;

    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using Orleans.Runtime;
    using Orleans.Streams;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    [DemocriteSystemVGrain]
    public abstract class TriggerBaseHandlerVGrain<TVGrainState, TTriggerDefinition, TVGrainInterface> : VGrainBase<TVGrainState, TVGrainInterface>, ITriggerHandlerVGrain
        where TVGrainState : TriggerState
        where TVGrainInterface : ITriggerHandlerVGrain
        where TTriggerDefinition : TriggerDefinition
    {
        #region Fields

        private static readonly Dictionary<TargetTypeEnum, Func<TriggerBaseHandlerVGrain<TVGrainState, TTriggerDefinition, TVGrainInterface>, Guid[], object?, bool, CancellationToken, Task>> s_targetFinalSenders;

        private static readonly TimeSpan s_inputBuildTimeout = TimeSpan.FromMinutes(1);

        private readonly Dictionary<Guid, IDataSourceProvider?> _dataSourceProviders;

        private readonly Dictionary<Guid, IAsyncStream<object>> _streamCache;
        private readonly ReaderWriterLockSlim _streamCacheLocker;

        private readonly IStreamQueueDefinitionProvider _streamQueueDefinitionProvider;
        private readonly ISequenceDefinitionProvider _sequenceDefinitionProvider;
        private readonly IDataSourceProviderFactory _dataSourceProviderFactory;
        private readonly ITriggerDefinitionProvider _triggerDefinitionProvider;
        private readonly IDemocriteExecutionHandler _democriteExecutionHandler;
        private readonly ISignalDefinitionProvider _signalDefinitionProvider;
        private readonly ISignalService _signalService;

        private TTriggerDefinition? _triggerDefinition;

        /// <summary>
        /// The targets group by type and dedicated source information
        /// </summary>
        private Dictionary<TargetTypeEnum, IReadOnlyCollection<(DataSourceDefinition? DedicatedOutputDef, Guid[] Targets)>>? _targets;
        private bool _force;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TriggerBaseHandlerVGrain{TVGrainState, TTriggerDefinition, TVGrainInterface}"/> class.
        /// </summary>
        static TriggerBaseHandlerVGrain()
        {
            s_targetFinalSenders = new Dictionary<TargetTypeEnum, Func<TriggerBaseHandlerVGrain<TVGrainState, TTriggerDefinition, TVGrainInterface>, Guid[], object?, bool, CancellationToken, Task>>()
            {
                { TargetTypeEnum.Signal, FireFinalSignalTargets },
                { TargetTypeEnum.Sequence, FireFinalSequenceTargets },
                { TargetTypeEnum.Stream, FireFinalStreamTargets },
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerBaseHandlerVGrain{TStates, TVGrainInterface}"/> class.
        /// </summary>
        protected TriggerBaseHandlerVGrain(ILogger<TVGrainInterface> logger,
                                           IPersistentState<TVGrainState> persistentState,
                                           ITriggerDefinitionProvider triggerDefinitionProvider,
                                           ISequenceDefinitionProvider sequenceDefinitionProvider,
                                           ISignalDefinitionProvider signalDefinitionProvider,
                                           IDemocriteExecutionHandler democriteExecutionHandler,
                                           IDataSourceProviderFactory inputSourceProviderFactory,
                                           IStreamQueueDefinitionProvider streamQueueDefinitionProvider,
                                           ISignalService signalService)
            : base(logger, persistentState)
        {
            this._streamCacheLocker = new ReaderWriterLockSlim();
            this._streamCache = new Dictionary<Guid, IAsyncStream<object>>();

            this._dataSourceProviders = new Dictionary<Guid, IDataSourceProvider?>();

            this._triggerDefinitionProvider = triggerDefinitionProvider;
            this._sequenceDefinitionProvider = sequenceDefinitionProvider;
            this._signalDefinitionProvider = signalDefinitionProvider;
            this._democriteExecutionHandler = democriteExecutionHandler;
            this._dataSourceProviderFactory = inputSourceProviderFactory;
            this._streamQueueDefinitionProvider = streamQueueDefinitionProvider;
            this._signalService = signalService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the trigger definition.
        /// </summary>
        public TTriggerDefinition TriggerDefinition
        {
            get { return this._triggerDefinition ?? throw new NullReferenceException("Consume the definition is not allow before activation or update"); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="TriggerBaseHandlerVGrain{TVGrainState, TTriggerDefinition, TVGrainInterface}"/> is enabled.
        /// </summary>
        public bool Enabled { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual async Task UpdateAsync(GrainCancellationToken grainCancellationToken)
        {
            using (var grp = CancellationTokenSource.CreateLinkedTokenSource(grainCancellationToken.CancellationToken, this.VGrainLifecycleToken))
            {
                this._force = true;
                await EnsureTriggerDefinitionAsync(grp.Token);
            }
        }

        /// <inheritdoc />
        public sealed override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            await EnsureTriggerDefinitionAsync(cancellationToken);
        }

        #region Tools

        /// <summary>
        /// Fires target sequence and signal
        /// </summary>
        protected async Task FireTriggerAsync(object? defaultInput = null, bool waitResult = false, CancellationToken token = default)
        {
            if (this.Enabled == false)
                return;

            await EnsureTriggerDefinitionAsync(token);

            using (var timeoutToken = CancellationHelper.DisposableTimeout(s_inputBuildTimeout))
            using (var fireCancelToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Content, token))
            {
                var input = await GetNextInputAsync(this.TriggerDefinition.TriggerGlobalOutputDefinition, fireCancelToken.Token, defaultInput);

                Debug.Assert(this._targets != null);

                foreach (var targetTypes in this._targets!.OrderBy(k => k.Key))
                {
                    if (s_targetFinalSenders.TryGetValue(targetTypes.Key, out var sender))
                    {
                        foreach (var confiTarget in targetTypes.Value)
                        {
                            var dedicatedInput = input;

                            if (confiTarget.DedicatedOutputDef is not null)
                                dedicatedInput = await GetNextInputAsync(confiTarget.DedicatedOutputDef, timeoutToken.Content, dedicatedInput);

                            try
                            {
                                await sender(this, confiTarget.Targets, dedicatedInput, waitResult, fireCancelToken.Token);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.OptiLog(LogLevel.Error, "Trigger target failed {targetType} : error {exception}", targetTypes.Key, ex);
                            }
                        }
                        continue;
                    }

                    throw new NotSupportedException("[" + targetTypes.Key + "] Target type is not managed ");
                }
            }
        }

        /// <summary>
        /// Gets the data source provider related to <see cref="DataSourceDefinition"/>
        /// </summary>
        private async ValueTask<IDataSourceProvider> GetDataSourceProvider(DataSourceDefinition dedicatedOutputDef, CancellationToken token)
        {
            IDataSourceProvider? provider;
            if (!this._dataSourceProviders.TryGetValue(dedicatedOutputDef.Uid, out provider) || (await provider!.IsStillValidAsync(dedicatedOutputDef, token) == false))
            {
                var old = provider;

                provider = this._dataSourceProviderFactory.GetProvider(dedicatedOutputDef);
                this._dataSourceProviders.Add(dedicatedOutputDef.Uid, provider);

                object? state = null;
                if (provider.UseState)
                {
                    this.State!.DataSourceProviderStates?.TryGetValue(dedicatedOutputDef.Uid, out state);
                    await provider.RestoreStateAsync(state);
                }

                if (old is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else if (old is IDisposable disposable)
                    disposable.Dispose();
            }

            return provider;
        }

        /// <summary>
        /// Fetch the next input bases on setup if needed
        /// </summary>
        private async Task<object?> GetNextInputAsync(DataSourceDefinition? dataSourceDefinition,
                                                      CancellationToken timeoutToken,
                                                      object? defaultInput = null)
        {
            if (dataSourceDefinition is not null)
            {
                object? input = null;

                var provider = await GetDataSourceProvider(dataSourceDefinition, timeoutToken);

                input = await provider!.GetNextAsync(timeoutToken);

                if (provider.UseState)
                {
                    this.State!.DataSourceProviderStates[dataSourceDefinition.Uid] = provider.GetState();
                    await OnUpdateDataSourceProiverState(dataSourceDefinition);
                }

                return input;
            }

            return defaultInput;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual ValueTask OnUpdateDataSourceProiverState(DataSourceDefinition dataSourceDefinition)
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Fires Sequence targets.
        /// </summary>
        private static async Task FireFinalSequenceTargets(TriggerBaseHandlerVGrain<TVGrainState, TTriggerDefinition, TVGrainInterface> inst,
                                                         Guid[] sequences,
                                                         object? input,
                                                         bool waitResult,
                                                         CancellationToken token)
        {
            var targetSequences = await inst._sequenceDefinitionProvider.GetByKeyAsync(token, sequences);

            if (input is IEnumerable enumerable && input is not string)
            {
                var tasks = enumerable.Cast<object?>()
                                      .Select(item => inst.FireSequencesTargets(targetSequences, item, waitResult, token))
                                      .ToArray();

                await tasks.SafeWhenAllAsync();
            }
            else
            {
                await inst.FireSequencesTargets(targetSequences, input, waitResult, token);
            }
        }

        /// <summary>
        /// Fires the sequences targets.
        /// </summary>
        private async Task FireSequencesTargets(IReadOnlyCollection<SequenceDefinition> targetSequences,
                                                object? input,
                                                bool waitResult,
                                                CancellationToken token)
        {
            foreach (var target in targetSequences)
            {
                // FireAsync will not wait the end of the execution
                var launcher = this._democriteExecutionHandler.SequenceWithInput(target.Uid)
                                                              .SetInput(input);

                if (waitResult)
                    await launcher.RunAsync(token);
                else
                    await launcher.Fire();
            }
        }

        /// <summary>
        /// Fires signal targets.
        /// </summary>
        private static async Task FireFinalSignalTargets(TriggerBaseHandlerVGrain<TVGrainState, TTriggerDefinition, TVGrainInterface> inst,
                                                         Guid[] signals,
                                                         object? input,
                                                         bool waitResult,
                                                         CancellationToken token)
        {
            var targetSignals = await inst._signalDefinitionProvider.GetByKeyAsync(token, signals);

            if (input is IEnumerable enumerable && input is not string)
            {
                var tasks = enumerable.Cast<object?>()
                                      .Select(item => inst.FireSignalsTargets(targetSignals, item, token))
                                      .ToArray();

                await tasks.SafeWhenAllAsync(token);
            }
            else
            {
                await inst.FireSignalsTargets(targetSignals, input, token);
            }
        }

        /// <summary>
        /// Fires the sequences targets.
        /// </summary>
        /// <remarks>
        ///     Get the definition to ensure the register signal still exists
        /// </remarks>
        private async Task FireSignalsTargets(IReadOnlyCollection<SignalDefinition> targetSignals,
                                              object? input,
                                              CancellationToken token)
        {
            foreach (var target in targetSignals)
            {
                await this._signalService.Fire(target.SignalId, input, token, null);
            }
        }

        /// <summary>
        /// Fires stream targets.
        /// </summary>
        private static async Task FireFinalStreamTargets(TriggerBaseHandlerVGrain<TVGrainState, TTriggerDefinition, TVGrainInterface> inst,
                                                         Guid[] streams,
                                                         object? input,
                                                         bool waitResult,
                                                         CancellationToken token)
        {
            var targetStreams = await inst._streamQueueDefinitionProvider.GetByKeyAsync(token, streams);

            if (input is IEnumerable enumerable && input is not string)
            {
                var tasks = enumerable.Cast<object?>()
                                      .Select(item => inst.FireStreamsTargets(targetStreams, item, waitResult, token))
                                      .ToArray();

                await tasks.SafeWhenAllAsync();
            }
            else
            {
                await inst.FireStreamsTargets(targetStreams, input, waitResult, token);
            }
        }

        /// <summary>
        /// Fires the streams targets.
        /// </summary>
        private Task FireStreamsTargets(IReadOnlyCollection<StreamQueueDefinition> targetStreamQueues,
                                        object? input,
                                        bool _,
                                        CancellationToken token)
        {
            if (input is null)
                return Task.CompletedTask;

            var tasks = new List<Task>();
            foreach (var targetKV in targetStreamQueues.GroupBy(t => t.StreamConfiguration))
            {
                var streamProvider = GrainStreamingExtensions.GetStreamProvider(this, targetKV.Key);

                foreach (var target in targetKV)
                {
                    IAsyncStream<object>? stream = null;

                    this._streamCacheLocker.EnterReadLock();
                    try
                    {
                        if (this._streamCache.TryGetValue(target.Uid, out var cachedStream))
                            stream = cachedStream;
                    }
                    finally
                    {
                        this._streamCacheLocker.ExitReadLock();
                    }

                    var needNew = stream is null;

                    if (needNew)
                    {
                        try
                        {
                            var targetStreamId = target.ToStreamId();
                            stream = streamProvider.GetStream<object>(targetStreamId);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.OptiLog(LogLevel.Error, "Stream Get from trigger : {exception}", ex);
                        }

                        if (stream is null)
                            throw new InvalidOperationException("Impossible to get stream from this information " + target);

                        this._streamCacheLocker.EnterWriteLock();
                        try
                        {
                            this._streamCache[target.Uid] = stream;
                        }
                        finally
                        {
                            this._streamCacheLocker.ExitWriteLock();
                        }
                    }

                    //   tasks.Add(stream!.OnNextAsync(input));
                    stream!.OnNextAsync(input).ConfigureAwait(false);
                }
            }

            //await tasks.SafeWhenAllAsync(token);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Use <see cref="ISignalDefinitionProvider"/> to get <see cref="ICronTriggerHandlerVGrain"/>
        /// </summary>
        protected async Task EnsureTriggerDefinitionAsync(CancellationToken cancellationToken)
        {
            var triggerDefinitionId = this.GetPrimaryKey();

            if (!this._force && this._triggerDefinition != null && this._triggerDefinition.Uid == triggerDefinitionId)
                return;

            if (triggerDefinitionId == Guid.Empty)
                throw new InvalidVGrainIdException(GetGrainId(), "Trigger definition id");

            this._streamCacheLocker.EnterWriteLock();
            try
            {
                this._streamCache.Clear();
            }
            finally
            {
                this._streamCacheLocker.ExitWriteLock();
            }

            this._force = false;

            using (var timeoutToken = CancellationHelper.DisposableTimeout(s_inputBuildTimeout))
            using (var ensureCancelToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Content, this.VGrainLifecycleToken, cancellationToken))
            {
                var definition = (await this._triggerDefinitionProvider.GetByKeyAsync(triggerDefinitionId, ensureCancelToken.Token)) as TTriggerDefinition;

#pragma warning disable IDE0270 // Use coalesce expression
                if (definition == null)
                {
                    var oldDefinition = this._triggerDefinition;
                    var oldEnabled = this.Enabled;

                    this._triggerDefinition = null;
                    this.Enabled = false;

                    if (oldDefinition is not null || oldEnabled)
                        throw new MissingDefinitionException(typeof(TTriggerDefinition), triggerDefinitionId.ToString(), oldDefinition is not null ? "Trigger May be disabled or definition is lost" : "");

                    return;

                }
#pragma warning restore IDE0270 // Use coalesce expression

                this.Enabled = true;

                this._triggerDefinition = definition;

                this._targets = definition.Targets.GroupBy(k => k.Type)
                                          .ToDictionary(k => k.Key, v => v.GroupBy(vv => vv.DedicatedDataProvider)
                                                                          .Select(grp => (grp.Key, grp.Select(g => g.Uid).ToArray()))
                                                                          .ToReadOnly());

                await OnEnsureTriggerDefinitionAsync(ensureCancelToken.Token);
            }
        }

        /// <summary>
        /// Called when trigger grain need to be updated.
        /// </summary>
        protected abstract Task OnEnsureTriggerDefinitionAsync(CancellationToken token);

        /// <inheritdoc />
        protected override void DisposeResourcesEnd()
        {
            this._streamCacheLocker.Dispose();
            base.DisposeResourcesEnd();
        }

        #endregion

        #endregion
    }
}
