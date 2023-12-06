// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.VGrains.Web.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.VGrains.Web.Abstractions.Models;

    /// <summary>
    /// VGrain is charge to extract html from web page address
    /// </summary>
    /// <seealso cref="IVGrain" />
    public interface IHtmlCollectorVGrain : IVGrain
    {
        /// <summary>
        /// Fetches an html page through it's web URL
        /// </summary>
        Task<HtmlPage> FetchPageAsync(Uri pageUrl, IExecutionContext executionContext);
    }
}