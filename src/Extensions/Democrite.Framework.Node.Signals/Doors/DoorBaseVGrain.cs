// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Signals;

    using Elvex.Toolbox.Abstractions.Attributes;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class managing <see cref="IDoorVGrain"/> behavior
    /// </summary>
    /// <typeparam name="TVGrain">The type of the vgrain.</typeparam>
    /// <typeparam name="TDoordDef">The type of the doord definition.</typeparam>
    /// <seealso cref="VGrainBase{DoorHandlerState, DoorHandlerStateSurrogate, DoorHandlerStateSurrogateConverter, TVGrain}" />
    /// <seealso cref="IDoorVGrain" />
    /// <seealso cref="ISignalReceiver" />
    public abstract class DoorBaseVGrain<TVGrain, TDoordDef> : VGrainBase<DoorHandlerState, DoorHandlerStateSurrogate, DoorHandlerStateSurrogateConverter, TVGrain>,
                                                               IDoorVGrain,
                                                               ISignalReceiver
        where TVGrain : IDoorVGrain
        where TDoordDef : DoorDefinition
    {
        #region Fields

        private readonly IDoorDefinitionProvider _doorDefinitionProvider;
        private readonly ISignalService _signalService;
        private readonly IGrainFactory _grainFactory;

        private readonly SemaphoreSlim _concurrencyStateLock;
        private readonly SemaphoreSlim _concurrencyLock;

        private readonly HashSet<SubscriptionId> _subscriptionIds;

        private readonly TimeSpan _stimulationTimeout;

        private IComponentDoorIdentityCard? _identityCard;
        private IDoorSignalVGrain? _signalVGrain;
        private TDoordDef? _doordDefinition;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorBaseVGrain{TVGrain}"/> class.
        /// </summary>
        protected DoorBaseVGrain(ILogger<TVGrain> logger,
                                 IPersistentState<DoorHandlerStateSurrogate> persistentState,
                                 ISignalService signalService,
                                 IDoorDefinitionProvider doorDefinitionProvider,
                                 ITimeManager timeHandler,
                                 IGrainOrleanFactory grainFactory,
                                 TimeSpan? stimulationTimeout = null)
            : base(logger, persistentState)
        {
            this._concurrencyStateLock = new SemaphoreSlim(1);
            this._concurrencyLock = new SemaphoreSlim(1);

            this._stimulationTimeout = stimulationTimeout ?? TimeSpan.FromSeconds(1);

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
                this._stimulationTimeout = TimeSpan.FromMinutes(10);
#endif

            this._grainFactory = grainFactory;
            this.TimeHandler = timeHandler;
            this._signalService = signalService;
            this._doorDefinitionProvider = doorDefinitionProvider;

            this._subscriptionIds = new HashSet<SubscriptionId>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time handler.
        /// </summary>
        protected ITimeManager TimeHandler { get; }

        #endregion

        #region Nested

        /// <summary>
        /// Result returned by method <see cref="OnDoorStimulateAsync(TDoordDef, CancellationToken)"/>
        /// </summary>
        protected readonly record struct StimulationReponse(bool Result,
                                                            IReadOnlyCollection<SignalMessage> ResponsibleSignals,
                                                            bool NeedRebound = false,
                                                            bool RelaySignalsContent = false)
        {
            /// <summary>
            /// Gets the default.
            /// </summary>
            public static StimulationReponse Default { get; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task UpdateAsync(DoorDefinition doorDefinition, GrainCancellationToken token)
        {
            return InitializeImplAsync(doorDefinition, token.CancellationToken);
        }

        /// <inheritdoc />
        [OneWay]
        public async Task ReceiveSignalAsync(SignalMessage signal)
        {
            if (!await this._identityCard!.IsEnable())
                return;

            var state = this.State!;
            var hasPush = false;

            await this._concurrencyStateLock.WaitAsync();
            try
            {
                hasPush = state.Push(signal);
            }
            finally
            {
                this._concurrencyStateLock.Release();
            }

            await this._concurrencyLock.WaitAsync();

            try
            {
                // push only if signal cache state have changed
                if (hasPush)
                    await PushStateAsync(default);

                await HandledSignalContextChanged();
            }
            catch (Exception ex)
            {
                this.Logger.OptiLog(LogLevel.Error, "Error on handling input signal {signal} : {exception}", signal, ex);

                // backup on error
                await PushStateAsync(default);

                throw;
            }
            finally
            {
                this._concurrencyLock.Release();
            }
        }

        /// <summary>
        /// Sealed the activation to way the state restore
        /// </summary>
        public sealed override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
        }

        /// <summary>
        /// Called when when state is in initialization
        /// </summary>
        protected sealed override async Task OnActivationSetupState(DoorHandlerState? state, CancellationToken ct)
        {
            await base.OnActivationSetupState(state, ct);

            var doorId = this.GetPrimaryKey();

            var info = await this._doorDefinitionProvider.GetByKeyAsync(doorId, ct) ?? throw new DoorNotFoundException(doorId.ToString());

            this._identityCard = await base.RegisterAsComponentAsync<IComponentDoorIdentityCard>(info.Uid);

            await InitializeImplAsync(info, ct);
        }

        /// <summary>
        /// Gets the last not consumed active signal since interval (>).
        /// </summary>
        /// <remarks>
        ///     Must be used only on thread safe context like OnInitializeAsync or OnDoorStimulateAsync
        ///     By default the interval is compute by <see cref="ITimeManager.UtcNow"/> - <see cref="DoordDefinition.Interval"/>
        /// </remarks>
        /// <param name="forcedUtcMinTime">Force min datetime to UTC to gatter signal.</param>
        protected IReadOnlyCollection<SignalMessage> GetLastActiveSignalNotConsumed(DateTime? forcedUtcMinDateTime = null)
        {
            EnsureExecutionContextIsThreadSafe();

            var activeWindowLoopUp = forcedUtcMinDateTime;

            if (activeWindowLoopUp == null && this._doordDefinition!.ActiveWindowInterval != null)
                activeWindowLoopUp = this.TimeHandler.UtcNow - this._doordDefinition!.ActiveWindowInterval;

            this._concurrencyStateLock.Wait();
            try
            {
                return this.State!.GetLastActiveSignalSince(activeWindowLoopUp);
            }
            finally
            {
                this._concurrencyStateLock.Release();
            }
        }

        /// <summary>
        /// Gets the last signal received associate to <paramref name="signalUid"/>
        /// </summary>
        /// <remarks>
        ///     Must be used only on thread safe context like OnInitializeAsync or OnDoorStimulateAsync
        /// </remarks>
        protected SignalMessage? GetLastSignalReceived(Guid signalUid)
        {
            EnsureExecutionContextIsThreadSafe();

            return this.State!.GetLastSignalReceived(signalUid);
        }

        /// <summary>
        /// Call on door initialization
        /// </summary>
        /// <remarks>ThreadSafe</remarks>
        [ThreadSafe]
        protected virtual ValueTask OnInitializeAsync(TDoordDef doordDefinition, CancellationToken token)
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Call when door received some signal to compute decision if yes or not the door must trigged
        /// </summary>
        /// <returns>
        ///     <c>ThreadSafe</c> <br />
        ///     <c>result</c>: true if the door must send his signal;otherwise false.  <br/>
        ///     <c>responsibleSignals</c>: Signal responsibles for this door to fire. <br/>
        /// </returns>
        /// <remarks>ThreadSafe</remarks>
        [ThreadSafe]
        protected abstract ValueTask<StimulationReponse> OnDoorStimulateAsync(TDoordDef doordDef, CancellationToken token);

        /// <summary>
        ///     Mark <see cref="SignalMessage"/> are used and will not be reuse later on.
        /// </summary>
        /// <remarks>
        ///     Must be used only on thread safe context like OnInitializeAsync or OnDoorStimulateAsync
        ///     By default the interval is compute by <see cref="ITimeManager.UtcNow"/> - <see cref="DoordDefinition.Interval"/>
        /// </remarks>
        protected void MarkAsUsed(IReadOnlyCollection<SignalMessage> signalUse)
        {
            EnsureExecutionContextIsThreadSafe();
            this.State!.MarkAsUsed(signalUse, this._doordDefinition!.HistoryMaxRetention != 0);
        }

        /// <summary>
        /// Manually the trigger door stimulation.
        /// </summary>
        protected async Task ManuallyTriggerStimulation()
        {
            await this._concurrencyLock.WaitAsync();

            try
            {
                await HandledSignalContextChanged();
            }
            finally
            {
                this._concurrencyLock.Release();
            }
        }

        /// <summary>
        /// Produces the signal content from result.
        /// </summary>
        protected virtual object? ProduceSignalContentFromResult(DoorBaseVGrain<TVGrain, TDoordDef>.StimulationReponse stimulationResult)
        {
            if (stimulationResult.ResponsibleSignals.Count == 0)
                return null;

            if (stimulationResult.ResponsibleSignals.Count == 1)
            {
                var signalResponsible = stimulationResult.ResponsibleSignals.Single();

                if (signalResponsible != null &&
                    signalResponsible.From != null &&
                    !string.IsNullOrEmpty(signalResponsible.From.CarryMessageType))
                {
                    return signalResponsible.From.GetContent();
                }

                return null;
            }

            throw new NotSupportedException("To managed relay of multiples signals to must override this method " + nameof(ProduceSignalContentFromResult));
        }

        /// <summary>
        /// Called when at the end of <see cref="DisposeResourcesEnd"/> to offer child class a safe way to clean resources
        /// </summary>
        protected virtual void OnDisposeResourcesEnd()
        {
        }

        #region Tools

        /// <summary>
        /// Initializes implementation
        /// </summary>
        private async Task InitializeImplAsync(DoorDefinition doorDefinition, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(doorDefinition);

            await this._concurrencyLock.WaitAsync(token);

            try
            {
                if (EqualityComparer<DoorDefinition>.Default.Equals(this._doordDefinition, doorDefinition))
                    return;

                this._doordDefinition = (TDoordDef)doorDefinition;

                // Ensure grain and definition always match
                System.Diagnostics.Debug.Assert(doorDefinition.Uid == this.GetPrimaryKey());

                try
                {
                    foreach (var signalSource in doorDefinition.SignalSourceIds)
                    {
                        var subscrptionId = await this._signalService.SubscribeAsync(signalSource, this, token);
                        this._subscriptionIds.Add(subscrptionId);
                    }

                    foreach (var doorSource in doorDefinition.DoorSourceIds)
                    {
                        var subscrptionId = await this._signalService.SubscribeAsync(doorSource, this, token);
                        this._subscriptionIds.Add(subscrptionId);
                    }

                    await this._concurrencyStateLock.WaitAsync(token);
                    try
                    {
                        this.State!.InitializeSignalSupport(doorDefinition);
                    }
                    finally
                    {
                        this._concurrencyStateLock.Release();
                    }

                    if (this._signalVGrain == null)
                        this._signalVGrain = this._grainFactory.GetGrain<IDoorSignalVGrain>(doorDefinition.DoorId.Uid);

                    await OnInitializeAsync(this._doordDefinition, token);
                }
                catch (Exception ex)
                {
                    this.Logger.OptiLog(LogLevel.Error, "Door initialization failed {exception}", ex);
                    throw;
                }
            }
            finally
            {
                this._concurrencyLock.Release();
            }
        }

        /// <summary>
        /// Call when door received some signal to compute decision if yes or not the door must trigged
        /// </summary>
        /// <returns>
        ///     <c>true</c> is the door signal have been fire at least one.
        /// </returns>
        [ThreadSafe]
        private async Task<bool> StimulateDoorAsync(TDoordDef doordDef)
        {
            bool hadFire = false;

            try
            {
                bool stillToProcess = false;
                do
                {
                    stillToProcess = false;

                    using (var timeoutToken = CancellationHelper.DisposableTimeout(this._stimulationTimeout))
                    using (var token = CancellationTokenSource.CreateLinkedTokenSource(this.VGrainLifecycleToken, timeoutToken.Content))
                    {
                        var stimulationResult = await OnDoorStimulateAsync(doordDef, token.Token);

                        if (stimulationResult.Result == false)
                            break;

                        token.Token.ThrowIfCancellationRequested();

                        if (this._signalVGrain != null)
                        {
                            if (stimulationResult.ResponsibleSignals != null && stimulationResult.ResponsibleSignals.Count > 0)
                                MarkAsUsed(stimulationResult.ResponsibleSignals);

                            var fireId = Guid.NewGuid();

                            var source = CreateSignalSource(fireId, stimulationResult, doordDef);

                            var signal = new SignalMessage(fireId,
                                                           this.TimeHandler.UtcNow,
                                                           source);

                            token.Token.ThrowIfCancellationRequested();
                            await this._signalVGrain.Fire(signal);

                            hadFire = true;
                            stillToProcess = stimulationResult.NeedRebound;
                        }
                    }
                }
                while (stillToProcess);

            }
            catch (OperationCanceledException)
            {
            }

            return hadFire;
        }

        /// <summary>
        /// Creates <see cref="SignalSource"/> that defined the current information responsible of a signal fire.
        /// </summary>
        private SignalSource CreateSignalSource(in Guid fireId,
                                                in DoorBaseVGrain<TVGrain, TDoordDef>.StimulationReponse stimulationResult,
                                                in TDoordDef doordDef)
        {
            Type? contentType = null;
            object? content = null;

            if (stimulationResult.RelaySignalsContent)
            {
                content = ProduceSignalContentFromResult(stimulationResult);
                contentType = content?.GetType();
            }

            return SignalSource.Create(fireId,
                                       doordDef.DoorId.Uid,
                                       doordDef.DoorId.Name ?? doordDef.DoorId.Uid.ToString(),
                                       true,
                                       this.TimeHandler.UtcNow,
                                       GetGrainId(),
                                       this.MetaData,
                                       contentType,
                                       content,
                                       stimulationResult.ResponsibleSignals?.Select(s => s.From));
        }

        /// <summary>
        /// Ensures the execution context is thread safe.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">This method MUST be called in safe context like in OnInitializeAsync or OnDoorStimulateAsync</exception>
        private void EnsureExecutionContextIsThreadSafe()
        {
            // CurrentCount display the remain allowed thread
            if (this._concurrencyLock.CurrentCount > 0)
                throw new InvalidOperationException("This method MUST be called in safe context like in OnInitializeAsync or OnDoorStimulateAsync");
        }

        /// <summary>
        /// Handleds door stimuation when signal context changed (or manual called)
        /// </summary>
        [ThreadSafe]
        private async Task HandledSignalContextChanged()
        {
            var state = this.State!;
            var def = this._doordDefinition!;

            var hadFire = false;

            if (await this._identityCard!.CanBeStimuate())
                hadFire = await StimulateDoorAsync(def);

            var hadCleaned = false;

            if (this._doordDefinition != null)
            {
                if (def.ActiveWindowInterval != null)
                    hadCleaned |= state.ClearHistoryAndNotConsumed(this.TimeHandler.UtcNow.Subtract(def.ActiveWindowInterval.Value));

                if (def.HistoryMaxRetention != null)
                    hadCleaned |= state.ClearHistory(def.HistoryMaxRetention.Value);

                if (def.NotConsumedMaxRetiention != null)
                    hadCleaned |= state.ClearNotConsumed(def.NotConsumedMaxRetiention.Value);
            }

            // push only if signal cache state have changed
            if (hadFire || hadCleaned)
                await PushStateAsync(default);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Sealed to ensure the resource is well cleaned
        /// </remarks>
        protected sealed override void DisposeResourcesEnd()
        {
            this._concurrencyLock.Dispose();
            this._concurrencyStateLock.Dispose();

            base.DisposeResourcesEnd();

            OnDisposeResourcesEnd();
        }

        /// <inheritdoc />
        protected sealed override async ValueTask<DoorHandlerState> OnPullStateAsync(CancellationToken ct)
        {
            await this._concurrencyStateLock.WaitAsync(ct);
            try
            {
                return await base.OnPullStateAsync(ct);
            }
            finally
            {
                this._concurrencyStateLock.Release();
            }
        }

        /// <inheritdoc />
        protected sealed override async ValueTask OnPushStateAsync(DoorHandlerState newState, CancellationToken ct)
        {
            await this._concurrencyStateLock.WaitAsync(ct);
            try
            {
                await base.OnPushStateAsync(newState, ct);
            }
            finally
            {
                this._concurrencyStateLock.Release();
            }
        }

        #endregion

        #endregion
    }
}
