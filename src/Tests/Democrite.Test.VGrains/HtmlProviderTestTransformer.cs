// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Test.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Test.Interfaces;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;

    using System;
    using System.Threading.Tasks;

    [StatelessWorker]
    public sealed class HtmlProviderTestTransformer : VGrainBase<IHtmlProviderTestTransformer>, IHtmlProviderTestTransformer
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="HtmlProviderTestTransformer"/> class.
        /// </summary>
        static HtmlProviderTestTransformer()
        {
            var assembly = typeof(HtmlProviderTestTransformer).Assembly;
            var res = assembly.GetManifestResourceNames().First(s => s.Contains("sample.html", StringComparison.OrdinalIgnoreCase));
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(res)!))
            {
                SampleHtml = reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlProviderTestTransformer"/> class.
        /// </summary>
        public HtmlProviderTestTransformer(ILogger<HtmlProviderTestTransformer> logger)
            : base(logger)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the sample HTML.
        /// </summary>
        public static string SampleHtml { get; }

        #endregion

        #region methods

        /// <inheritdoc />
        public Task<string> GetHtmlAsync(IExecutionContext sequenceContext)
        {
            return Task.FromResult(SampleHtml);
        }

        #endregion
    }
}
