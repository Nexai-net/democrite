// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using Nexai.Sample.Forex.VGrain.Abstractions;

    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ISignalDemoTargetVGrain" />
    public sealed class SignalDemoTargetVGrain : VGrainBase<ISignalDemoTargetVGrain>, ISignalDemoTargetVGrain
    {
        #region Fields

        private readonly ISignalService _signalService;
        private SubscriptionId _subscriptionId;

        #endregion

        #region Ctor

        public SignalDemoTargetVGrain(ILogger<SignalDemoTargetVGrain> logger,
                                           ISignalService signalService)
            : base(logger)
        {
            this._signalService = signalService;
        }

        #endregion

        #region 

        /// <inheritdoc />
        public async Task SubscribeToAsync(SignalDefinition signalDefinition)
        {
            this._subscriptionId = await this._signalService.SubscribeAsync(signalDefinition.SignalId, this, default);
        }

        public async Task SubscribeToAsync(DoorDefinition doorDefinition)
        {
            this._subscriptionId = await this._signalService.SubscribeAsync(doorDefinition.DoorId, this, default);
        }

        /// <inheritdoc />
        public Task ReceiveSignalAsync(SignalMessage signal)
        {
            this.Logger.OptiLog(LogLevel.Information, "{currentGrainId} received signal : {signal}", GetGrainId(), JsonConvert.SerializeObject(signal, Formatting.Indented));
            return Task.CompletedTask;
        }

        #endregion
    }
}
