// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using Nexai.Sample.Forex.VGrain.Abstractions;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ISignalDemoTargetVGrain" />
    public sealed class SignalDemoTargetVGrain : VGrainBase<ISignalDemoTargetVGrain>, ISignalDemoTargetVGrain
    {
        #region Fields

        private readonly ISignalService _signalService;
        private Guid _subscriptionId;

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
            this._subscriptionId = await this._signalService.SubscribeAsync(signalDefinition.SignalId, this);
        }

        public async Task SubscribeToAsync(DoorDefinition doorDefinition)
        {
            this._subscriptionId = await this._signalService.SubscribeAsync(doorDefinition.DoorId, this);
        }

        /// <inheritdoc />
        public Task ReceiveSignalAsync(SignalMessage signal)
        {
            this.Logger.OptiLog<GrainId, string>(LogLevel.Information, "{currentGrainId} received signal : {signal}", GetGrainId(), JsonConvert.SerializeObject(signal, Formatting.Indented));
            return Task.CompletedTask;
        }

        #endregion
    }
}
