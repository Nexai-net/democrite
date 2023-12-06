// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Node.Signals;
    using Democrite.Framework.Toolbox.Abstractions.Attributes;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

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

        private readonly SemaphoreSlim _concurrencyLock;
        private readonly HashSet<Guid> _subscriptionIds;

        private readonly TimeSpan _stimulationTimeout;

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
                                   IGrainFactory grainFactory,
                                   TimeSpan? stimulationTimeout = null)
            : base(logger, persistentState)
        {
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

            this._subscriptionIds = new HashSet<Guid>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time handler.
        /// </summary>
        protected ITimeManager TimeHandler { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task InitializeAsync(DoorDefinition doorDefinition)
        {
            await this._concurrencyLock.WaitAsync();

            try
            {
                if (EqualityComparer<DoorDefinition>.Default.Equals(this._doordDefinition, doorDefinition))
                    return;

                this._doordDefinition = (TDoordDef)doorDefinition;
                try
                {
                    foreach (var signalSource in doorDefinition.SignalSourceIds)
                    {
                        var subscrptionId = await this._signalService.SubscribeAsync(signalSource, this);
                        this._subscriptionIds.Add(subscrptionId);
                    }

                    foreach (var doorSource in doorDefinition.DoorSourceIds)
                    {
                        var subscrptionId = await this._signalService.SubscribeAsync(doorSource, this);
                        this._subscriptionIds.Add(subscrptionId);
                    }

                    this.State!.InitializeSignalSupport(doorDefinition);

                    if (this._signalVGrain == null)
                        this._signalVGrain = this._grainFactory.GetGrain<IDoorSignalVGrain>(doorDefinition.DoorId.Uid);

                    await OnInitializeAsync(this._doordDefinition);
                }
                catch (Exception ex)
                {
                    this.Logger.OptiLog(LogLevel.Error, "Door initialization failed {exception}", ex);
                }
            }
            finally
            {
                this._concurrencyLock.Release();
            }
        }

        /// <inheritdoc />
        public async Task ReceiveSignalAsync(SignalMessage signal)
        {
            await this._concurrencyLock.WaitAsync();

            try
            {
                this.State!.UpdateSignalStatus(signal);
                await PushStateAsync(default);

                await StimulateDoorAsync(this._doordDefinition!);

                if (this._doordDefinition != null)
                    this.State!.CleanHistory(this.TimeHandler.UtcNow.Subtract(this._doordDefinition.Interval));

                await PushStateAsync(default);
            }
            finally
            {
                this._concurrencyLock.Release();
            }
        }

        /// <inheritdoc />
        public sealed override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            var doorId = this.GetPrimaryKey();

            var info = await this._doorDefinitionProvider.TryGetFirstValueAsync(doorId);

            if (info.Result == false || info.value == null)
                throw new DoorNotFoundException(doorId.ToString());

            await InitializeAsync(info.value);
        }

        #region Tools

        /// <summary>
        /// Gets the last not consumed active signal since interval (>).
        /// </summary>
        /// <remarks>
        ///     Must be used only on thread safe context like OnInitializeAsync or OnDoorStimulateAsync
        ///     By default the interval is compute by <see cref="ITimeManager.UtcNow"/> - <see cref="DoordDefinition.Interval"/>
        /// </remarks>
        /// <param name="forcedUtcMinTime">Force min datetime to UTC to gatter signal.</param>
        protected IReadOnlyCollection<SignalMessage> GetLastActiveSignalSince(DateTime? forcedUtcMinDateTime = null)
        {
            EnsureExecutionContextIsThreadSafe();

            var minPeriodTime = forcedUtcMinDateTime ?? this.TimeHandler.UtcNow - this._doordDefinition!.Interval;
            return this.State!.GetLastActiveSignalSince(minPeriodTime);
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
        protected virtual ValueTask OnInitializeAsync(TDoordDef doordDefinition)
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
        protected abstract ValueTask<(bool result, IReadOnlyCollection<SignalMessage> responsibleSignals)> OnDoorStimulateAsync(TDoordDef doordDef, CancellationToken token);

        /// <summary>
        /// Manuallies the trigger door stimulation.
        /// </summary>
        protected async Task ManuallyTriggerStimulation()
        {
            await this._concurrencyLock.WaitAsync();

            try
            {
                await StimulateDoorAsync(this._doordDefinition!);
            }
            finally
            {
                this._concurrencyLock.Release();
            }
        }

        /// <summary>
        /// Call when door received some signal to compute decision if yes or not the door must trigged
        /// </summary>
        [ThreadSafe]
        private async Task StimulateDoorAsync(TDoordDef doordDef)
        {
            try
            {
                var token = CancellationHelper.Timeout(this._stimulationTimeout);
                var mustFire = await OnDoorStimulateAsync(doordDef, token);

                if (mustFire.result == false)
                    return;

                if (this._signalVGrain != null)
                {
                    if (mustFire.responsibleSignals != null && mustFire.responsibleSignals.Count > 0)
                        this.State!.UseToFire(mustFire.responsibleSignals);

                    var fireId = Guid.NewGuid();

                    var signal = new SignalMessage(fireId,
                                          this.TimeHandler.UtcNow,
                                          new SignalSource(fireId,
                                                           doordDef.DoorId.Uid,
                                                           doordDef.DoorId.Name,
                                                           true,
                                                           this.TimeHandler.UtcNow,
                                                           GetGrainId(),
                                                           this.MetaData,
                                                           mustFire.responsibleSignals?.Select(s => s.From)));

                    await this._signalVGrain.Fire(signal);
                }
            }
            catch (OperationCanceledException)
            {
            }
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

        /// <inheritdoc />
        protected override void DisposeResourcesEnd()
        {
            this._concurrencyLock.Dispose();
            base.DisposeResourcesEnd();
        }

        #endregion

        #endregion
    }
}
