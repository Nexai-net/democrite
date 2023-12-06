// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.VGrains.Web.Abstractions.Models;

    /// <summary>
    /// VGrain in charge to search price on html page
    /// </summary>
    /// <seealso cref="IVGrain" />
    public interface IPriceInspectorVGrain : IVGrain
    {
        /// <summary>
        /// Searches in configure HTML page the price the currency pair.
        /// </summary>
        [ExecutionConfigurationInfo("Expect currency pair like 'eur-usd'")]
        [ExecutionConfigurationRegexValidator("[a-zA-Z]{3}[-]{1}[a-zA-Z]{3}")]
        Task<SerieSetValueModel<double?, string>> SearchValueAsync(HtmlPage page, IExecutionContext<string> executionContext);
    }
}