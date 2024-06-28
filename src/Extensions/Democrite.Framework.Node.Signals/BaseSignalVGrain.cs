// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class of any signal handler
    /// </summary>
    [DemocriteSystemVGrain]
    internal abstract class BaseSignalVGrain<TVGrainInterface> : VGrainBase<SignalHandlerState, SignalHandlerStateSurrogate, SignalHandlerStateSurrogateConverter, TVGrainInterface>
         where TVGrainInterface : IVGrain, ISignalHandler
    {
        #region Fields

        private readonly IRemoteGrainServiceFactory _remoteGrainServiceFactory;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSignalSignalVGrain"/> class.
        /// </summary>
        public BaseSignalVGrain(ILogger<TVGrainInterface> logger,
                                IPersistentState<SignalHandlerStateSurrogate> persistentState,
                                IGrainFactory grainFactory,
                                IRemoteGrainServiceFactory remoteGrainServiceFactory)
            : base(logger, persistentState)
        {
            this._remoteGrainServiceFactory = remoteGrainServiceFactory;
            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask InitializeAsync(GrainCancellationToken token)
        {
            return EnsureInitializedAsync(token.CancellationToken);
        }

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(DedicatedGrainId<ISignalReceiver> grainId, GrainCancellationToken token)
        {
            await EnsureInitializedAsync(token.CancellationToken);
            var subscriptionId = this.State!.AddOrUpdateSubscription(grainId);

            await PushStateAsync(default);

            return subscriptionId;
        }

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(DedicatedGrainId<ISignalReceiverReadOnly> grainId, GrainCancellationToken token)
        {
            await EnsureInitializedAsync(token.CancellationToken);
            var subscriptionId = this.State!.AddOrUpdateSubscription(grainId);

            await PushStateAsync(default);

            return subscriptionId;
        }

        /// <inheritdoc />
        public async Task UnsuscribeAsync(Guid subscritionId, GrainCancellationToken token)
        {
            await EnsureInitializedAsync(token.CancellationToken);

            this.State!.RemoveSuscription(subscritionId);
            await PushStateAsync(token.CancellationToken);
        }

        /// <inheritdoc />
        public async Task UnsuscribeAsync(DedicatedGrainId<ISignalReceiver> grainId, GrainCancellationToken token)
        {
            await EnsureInitializedAsync(token.CancellationToken);
            var subscriptionId = this.State!.GetSuscription(grainId);

            if (subscriptionId is not null)
                await UnsuscribeAsync(subscriptionId.Value, token);
        }

        /// <inheritdoc />
        public async Task UnsuscribeAsync(DedicatedGrainId<ISignalReceiverReadOnly> grainId, GrainCancellationToken token)
        {
            await EnsureInitializedAsync(token.CancellationToken);
            var subscriptionId = this.State!.GetSuscription(grainId);

            if (subscriptionId is not null)
                await UnsuscribeAsync(subscriptionId.Value, token);
        }

        #region Tools

        /// <summary>
        /// Fire implementation that send <see cref="SignalMessage"/> to all suscribers
        /// </summary>
        protected async Task<Guid> FireSignal(SignalMessage signal)
        {
            await EnsureInitializedAsync(default);

            var subscriptions = this.State!.Subscriptions;

            foreach (var sub in subscriptions)
            {
                ISignalReceiver? grain = null;
                ISignalReceiverReadOnly? grainReadOnly = null;

                var targetGrainId = (IDedicatedGrainId?)sub.TargetGrainId ?? (IDedicatedGrainId?)sub.TargetReadOnlyGrainId;

                if (targetGrainId!.IsGrainService)
                {
                    if (sub.IsTargetReadOnly)
                        grainReadOnly = this._remoteGrainServiceFactory.GetRemoteGrainService<ISignalReceiverReadOnly>(targetGrainId.Target, targetGrainId.GrainInterface.ToType());
                    else
                        grain = this._remoteGrainServiceFactory.GetRemoteGrainService<ISignalReceiver>(targetGrainId.Target, targetGrainId.GrainInterface.ToType());
                }
                else
                {
                    if (sub.IsTargetReadOnly)
                        grainReadOnly = this._grainFactory.GetGrain<ISignalReceiverReadOnly>(targetGrainId.Target);
                    else
                        grain = this._grainFactory.GetGrain<ISignalReceiver>(targetGrainId.Target);
                }

                if (grain is not null)
                    await grain.ReceiveSignalAsync(signal!);

                if (grainReadOnly is not null)
                    await grainReadOnly.ReceiveSignalAsync(signal!);
            }

            return signal.Uid;
        }

        /// <summary>
        /// Ensures this instance is initialized.
        /// </summary>
        protected async ValueTask EnsureInitializedAsync(CancellationToken token)
        {
            using (var tokenGrp = CancellationTokenSource.CreateLinkedTokenSource(token, this.VGrainLifecycleToken))
            {
                await OnEnsureInitializedAsync(tokenGrp.Token);
            }
        }

        /// <summary>
        /// Ensures this instance is initialized.
        /// </summary>
        protected virtual ValueTask OnEnsureInitializedAsync(CancellationToken token)
        {
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public sealed override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            await EnsureInitializedAsync(cancellationToken);
        }

        #endregion

        #endregion
    }
}
