// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Models;

    using Orleans;

    using System;
    using System.Linq;

    /// <summary>
    /// Surrogate class of <see cref="ForexCurrencyState"/>
    /// </summary>
    [GenerateSerializer]
    public struct ForexCurrencyStateSurrogate
    {

        /// <inheritdoc cref="ForexCurrencyState.LastValues"/>
        [Id(0)]
        public SerieSetValueModel<double?, string>[] LastValues { get; set; }

        /// <inheritdoc cref="ForexCurrencyState.HistoryValues"/>
        [Id(1)]
        public SerieSetValueModel<double?, string>[] HistoryValues { get; set; }

        /// <inheritdoc cref="ForexCurrencyState.LastValuesSize"/>
        [Id(2)]
        public int LastValuesSize { get; set; }

        /// <inheritdoc cref="ForexCurrencyState.Configuration"/>
        [Id(3)]
        public string Configuration { get; set; }
    }

    [RegisterConverter]
    public sealed class ForexCurrencyStateSurrogateConverter : IConverter<ForexCurrencyState, ForexCurrencyStateSurrogate>
    {
        public ForexCurrencyState ConvertFromSurrogate(in ForexCurrencyStateSurrogate surrogate)
        {
            return new ForexCurrencyState(surrogate.LastValues,
                                          surrogate.HistoryValues,
                                          Math.Max(surrogate.LastValuesSize, 60 * 60 * 24),
                                          surrogate.Configuration);
        }

        public ForexCurrencyStateSurrogate ConvertToSurrogate(in ForexCurrencyState value)
        {
            return new ForexCurrencyStateSurrogate()
            {
                LastValuesSize = value.LastValuesSize,
                LastValues = value.LastValues.ToArray(),
                HistoryValues = value.HistoryValues.ToArray(),
                Configuration = value.Configuration
            };
        }
    }
}
