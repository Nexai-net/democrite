// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.VGrains.Web.Abstractions.Models
{
    using System.ComponentModel;

    /// <summary>
    /// Html page information
    /// </summary>
    [Serializable]
    [Immutable]
    [ImmutableObject(true)]
    [GenerateSerializer]
    public sealed class HtmlPage
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlPage"/> class.
        /// </summary>
        public HtmlPage(Uri url, string rawHtml)
        {
            this.Url = url;
            this.RawHtml = rawHtml;
        }

        #endregion

        #region Property

        /// <summary>
        /// Gets the source URL.
        /// </summary>
        [Id(0)]
        public Uri Url { get; }

        /// <summary>
        /// Gets the raw HTML.
        /// </summary>
        [Id(1)]
        public string RawHtml { get; }

        #endregion
    }
}
