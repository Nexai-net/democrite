// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox.Disposables;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ISignalLocalGrainServiceRelay&gt;" />
    internal sealed class SignalLocalServiceRelay : ISignalLocalServiceRelay, ISignalReceiver
    {
        #region Fields

        private readonly ISignalLocalGrainServiceRelayClient _signalLocalGrainServiceRelayClient;
        private readonly SemaphoreSlim _subscribeLocker;

        private readonly Dictionary<Guid, Task<SubscriptionId>> _signalSubscribe;
        private readonly Dictionary<Guid, CallbackInfo> _signalSubscribeCallback;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalLocalServiceRelay"/> class.
        /// </summary>
        public SignalLocalServiceRelay(ISignalLocalGrainServiceRelayClient signalLocalGrainServiceRelayClient)
        {
            this._signalLocalGrainServiceRelayClient = signalLocalGrainServiceRelayClient;
            this._signalSubscribeCallback = new Dictionary<Guid, CallbackInfo>();
            this._signalSubscribe = new Dictionary<Guid, Task<SubscriptionId>>();

            this._subscribeLocker = new SemaphoreSlim(1);
        }

        #endregion

        #region Nested

        private sealed record class CallbackInfo(Guid Uid, Guid SignalId, Func<SignalMessage, ValueTask> Action, Predicate<SignalMessage>? Predicated);

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task ReceiveSignalAsync(SignalMessage signal)
        {
            var callbacks = EnumerableHelper<CallbackInfo>.ReadOnly;

            await this._subscribeLocker.WaitAsync();
            try
            {
                callbacks = this._signalSubscribeCallback.Where(s => s.Value.SignalId == signal.From.SourceDefinitionId && (s.Value.Predicated is null || s.Value.Predicated(signal)))
                                                         .Select(kv => kv.Value)
                                                         .ToArray();
            }
            finally
            {
                this._subscribeLocker.Release();
            }

            List<Exception>? exceptions = null;
            foreach (var callback in callbacks)
            {
                try
                {
                    await callback.Action(signal);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

            if (exceptions is not null && exceptions.Any())
                throw new AggregateException(exceptions);
        }

        /// <inheritdoc />
        public async ValueTask<IDisposable> SubscribeAsync(Func<SignalMessage, ValueTask> action, SignalId signalId, Predicate<SignalMessage>? predicated = null)
        {
            await this._subscribeLocker.WaitAsync();
            try
            {
                Task<SubscriptionId>? subscriptionIdTask = null;

                if (!this._signalSubscribe.TryGetValue(signalId.Uid, out subscriptionIdTask))
                {
                    subscriptionIdTask = this._signalLocalGrainServiceRelayClient.Subscribe(signalId);
                    this._signalSubscribe.Add(signalId.Uid, subscriptionIdTask);
                }

                var subscription = new CallbackInfo(Guid.NewGuid(), signalId.Uid, action, predicated);
                this._signalSubscribeCallback.Add(subscription.Uid, subscription);

                return new DisposableAction<Guid>(Unsubscribe, subscription.Uid);
            }
            finally
            {
                this._subscribeLocker.Release();
            }
        }

        #region Tools

        /// <summary>
        /// Unsubscribes the specified unique identifier.
        /// </summary>
        private void Unsubscribe(Guid subscriptionId)
        {
            Task<SubscriptionId>? unsubscribeTask = null;
            this._subscribeLocker.Wait();
            try
            {
                if (this._signalSubscribeCallback.Remove(subscriptionId, out var callbackInfo))
                {
                    var existingSubscriptions = this._signalSubscribeCallback.Any(s => s.Value.SignalId == callbackInfo.SignalId);

                    if (existingSubscriptions == false && this._signalSubscribe.Remove(callbackInfo.SignalId, out var signalSubscriptionTask))
                    {
                        unsubscribeTask = signalSubscriptionTask;
                    }
                }
            }
            finally
            {
                this._subscribeLocker.Release();
            }

            if (unsubscribeTask is not null)
            {
                if (unsubscribeTask.IsCompletedSuccessfully == false)
                {
                    // TODO : managed subscription un finished
                    return;
                }

                //// Attention to dead lock
                var subscritionIdTask = this._signalLocalGrainServiceRelayClient.Unsubscribe(unsubscribeTask.Result);
                subscritionIdTask.ConfigureAwait(false);
            }
        }

        #endregion

        #endregion
    }
}
