// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Microsoft.Extensions.Logging;

    using Nexai.Sample.Forex.VGrain.Abstractions;
    using Nexai.Sample.Forex.VGrain.Abstractions.Models;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Grain vgrain used to store information about currency pair in the forex
    /// </summary>
    /// <seealso cref="VGrainBase" />
    /// <seealso cref="ICurrencyPairVGrain" />
    public sealed class CurrencyPairVGrain : VGrainBase<ForexCurrencyState, ForexCurrencyStateSurrogate, ForexCurrencyStateSurrogateConverter, ICurrencyPairVGrain>,
                                                   ICurrencyPairVGrain,
                                                   IGrainWithStringKey
    {
        #region Fields

        private readonly ISignalService _signalService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyPairVGrain"/> class.
        /// </summary>
        public CurrencyPairVGrain(ILogger<ICurrencyPairVGrain> logger,
                                        [PersistentState("forex")] IPersistentState<ForexCurrencyStateSurrogate> persistentState,
                                        ISignalService signalService)
            : base(logger, persistentState)
        {
            this._signalService = signalService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task StoreAsync(SerieSetValueModel<double?, string> value, IExecutionContext<string> context)
        {
            if (value is not null)
            {
                this.State!.PushNewValue(value!);

                if (this.State!.Count % 10 == 0)
                    await PushStateAsync(context.CancellationToken);

                await this._signalService.Fire("CurrencyPair_" + this.State!.Configuration + "_Stored_Above_Average", context.CancellationToken, this);
            }
        }

        /// <inheritdoc />
        [ReadOnly()]
        public Task<IReadOnlyCollection<SerieSetValueModel<double?, string>>> GetLastValues(int quantity, IExecutionContext<string> executionContext)
        {
            var lastValues = this.State!.LastValues
                                        .Reverse()
                                        .Take(quantity)
                                        .ToArray();

            if (lastValues.Length < quantity)
            {
                var fromHistory = this.State!.HistoryValues
                                             .Reverse()
                                             .Take(quantity - lastValues.Length)
                                             .ToArray();

                lastValues = lastValues.Concat(fromHistory)
                                       .ToArray();
            }

            return Task.FromResult(lastValues.OrderByDescending(l => l.TickUtc).ToReadOnly());
        }

        /// <inheritdoc />
        protected override Task OnActivationSetupState(ForexCurrencyState? state, CancellationToken ct)
        {
            var pair = this.GetPrimaryKeyString();

            state ??= new ForexCurrencyState(pair);

            if (string.IsNullOrEmpty(state.Configuration))
                state.SetupConfiguration(pair);

            return base.OnActivationSetupState(state, ct);
        }

        #endregion
    }
}