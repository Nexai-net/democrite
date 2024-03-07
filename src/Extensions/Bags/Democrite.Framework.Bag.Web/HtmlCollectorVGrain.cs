// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.VGrains.Web
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.VGrains.Web.Abstractions;
    using Democrite.VGrains.Web.Abstractions.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Placement;

    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain in charge to fetch html page based on URL
    /// </summary>
    /// <seealso cref="VGrainBase" />
    /// <seealso cref="IHtmlCollectorVGrain" />
    [StatelessWorker]
    [RandomPlacement]
    public sealed class HtmlCollectorVGrain : VGrainBase<IHtmlCollectorVGrain>, IHtmlCollectorVGrain
    {
        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlCollectorVGrain"/> class.
        /// </summary>
        public HtmlCollectorVGrain(ILogger<IHtmlCollectorVGrain> logger,
                                  IHttpClientFactory httpClientFactory)
            : base(logger)
        {
            this._httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<HtmlPage> FetchPageAsync(Uri pageUrl, IExecutionContext executionContext)
        {
            using (var client = this._httpClientFactory.CreateClient())
            {
                var pageRawResponse = await client.GetAsync(pageUrl);

                var contentString = await (pageRawResponse.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

                return new HtmlPage(pageUrl, contentString);
            }
        }

        #endregion
    }
}
