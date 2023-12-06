// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;

    using System.Threading.Tasks;

    /// <summary>
    /// Grain in charge to store and treat currency pair value
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{executionContext.configuration}", FirstParameterFallback = "global")]
    public interface ICurrencyPairVGrain : IVGrain
    {
        /// <summary>
        /// Stores historical values
        /// </summary>
        Task StoreAsync(SerieSetValueModel<double?, string> input, IExecutionContext<string> executionContext);

        /// <summary>
        /// Gets the last values.
        /// </summary>
        Task<IReadOnlyCollection<SerieSetValueModel<double?, string>>> GetLastValues(int quantity, IExecutionContext<string> executionContext);
    }
}
