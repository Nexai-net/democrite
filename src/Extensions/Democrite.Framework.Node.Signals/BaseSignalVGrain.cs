﻿// Copyright (c) Nexai.
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
         where TVGrainInterface : IVGrain
    {
        #region Fields

        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSignalSignalVGrain"/> class.
        /// </summary>
        public BaseSignalVGrain(ILogger<TVGrainInterface> logger,
                                IPersistentState<SignalHandlerStateSurrogate> persistentState,
                                IGrainFactory grainFactory)
            : base(logger, persistentState)
        {
            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask InitializeAsync()
        {
            return EnsureInitializedAsync();
        }

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(GrainId grainId)
        {
            await EnsureInitializedAsync();
            var subscriptionId = this.State!.AddOrUpdateSuscription(grainId);

            await PushStateAsync(default);

            return subscriptionId;
        }

        /// <inheritdoc />
        public async Task UnsuscribeAsync(Guid subscritionId)
        {
            await EnsureInitializedAsync();

            this.State!.RemoveSuscription(subscritionId);
        }

        #region Tools

        /// <summary>
        /// Fire implementation that send <see cref="SignalMessage"/> to all suscribers
        /// </summary>
        protected async Task<Guid> FireSignal(SignalMessage signal)
        {
            await EnsureInitializedAsync();

            var subscriptions = this.State!.Subscriptions;

            foreach (var sub in subscriptions)
            {
                var addressable = this._grainFactory.GetGrain(sub.TargetGrainId);
                await addressable.AsReference<ISignalReceiver>().ReceiveSignalAsync(signal!);
            }

            return signal.Uid;
        }

        /// <summary>
        /// Ensures this instance is initialized.
        /// </summary>
        protected virtual ValueTask EnsureInitializedAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public sealed override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            await EnsureInitializedAsync();
        }

        #endregion

        #endregion
    }
}
