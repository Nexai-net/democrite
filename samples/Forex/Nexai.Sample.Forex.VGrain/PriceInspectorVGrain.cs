// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.VGrains.Web.Abstractions.Models;

    using HtmlAgilityPack;

    using Microsoft.Extensions.Logging;

    using Nexai.Sample.Forex.VGrain.Abstractions;

    using Orleans.Concurrency;
    using Orleans.Placement;

    using System.Globalization;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain in charge to read html page and try to extract a price for the currency pair
    /// </summary>
    /// <seealso cref="VGrainBase" />
    /// <seealso cref="IPriceInspectorVGrain" />
    [StatelessWorker]
    [PreferLocalPlacement]
    public sealed class PriceInspectorVGrain : VGrainBase<IPriceInspectorVGrain>, IPriceInspectorVGrain
    {
        #region Ctor

        /// <summary>
        /// Initialize a new instance of the class <see cref="PriceInspectorVGrain"/>
        /// </summary>
        public PriceInspectorVGrain(ILogger<IPriceInspectorVGrain> logger)
            : base(logger)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Search price value from html page
        /// </summary>
        public Task<SerieSetValueModel<double?, string>> SearchValueAsync(HtmlPage page, IExecutionContext<string> executionContext)
        {
            double? value = null;

            if (page != null && !string.IsNullOrWhiteSpace(page.RawHtml))
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(page.RawHtml);

                var priceSpanNode = htmlDocument.DocumentNode.SelectNodes("//span [@data-test='instrument-price-last']")?.FirstOrDefault()
                                                ?? htmlDocument.DocumentNode.SelectNodes("//span [@class='c-instrument c-instrument--last']")?.FirstOrDefault()
                                                ?? htmlDocument.DocumentNode.SelectNodes("//span [@class='price']")?.FirstOrDefault();

                if (priceSpanNode is not null)
                {
                    var text = priceSpanNode.InnerText?.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                    if (double.TryParse(text, out var doubleValue))
                        value = doubleValue;
                }
                else
                {

                }
            }

            var result = new SerieSetValueModel<double?, string>(value,
                                                                 string.Empty,
                                                                 page?.Url.ToString() ?? string.Empty,
                                                                 executionContext.Configuration ?? string.Empty,
                                                                 DateTime.UtcNow);

            return Task.FromResult(result);
        }

        #endregion
    }
}
