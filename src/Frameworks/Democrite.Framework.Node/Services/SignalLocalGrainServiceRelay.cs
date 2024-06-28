// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// Grain service used as by silo local endpoint to register signal emit and relay it to local services
    /// </summary>
    /// <seealso cref="GrainService" />
    /// <seealso cref="ISignalLocalGrainServiceRelay" />
    internal sealed class SignalLocalGrainServiceRelay : GrainService, ISignalLocalGrainServiceRelay, ISignalReceiver, ISignalReceiverReadOnly
    {
        #region Fields
        
        private readonly SignalLocalServiceRelay _client;
        private readonly ISignalService _signalService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalLocalGrainServiceRelay"/> class.
        /// </summary>
        public SignalLocalGrainServiceRelay(GrainId grainId,
                                            Silo silo,
                                            ILoggerFactory loggerFactory,
                                            ISignalService signalService,
                                            SignalLocalServiceRelay client) 
            : base(grainId, silo, loggerFactory)
        {
            this._signalService = signalService;

            // Use client to send receive msg
            this._client = client;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<SubscriptionId> Subscribe(SignalId signalId)
        {
            return this._signalService.SubscribeAsync(signalId, this, default);
        }

        /// <inheritdoc />
        public Task Unsubscribe(SubscriptionId subscriptionId)
        {
            return this._signalService.UnsubscribeAsync(subscriptionId, default);
        }

        /// <inheritdoc />
        public Task ReceiveSignalAsync(SignalMessage signal)
        {
            return this._client.ReceiveSignalAsync(signal);
        }

        #endregion
    }
}
