// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Test.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Elvex.Toolbox.Helpers;
    using Democrite.Test.Interfaces;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;

    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    [StatelessWorker]
    public sealed class TagExtractorTestTransformer : VGrainBase<ITagExtractorTestTransformer>, ITagExtractorTestTransformer
    {
        #region Fields

        private static readonly Regex s_simpleTag = new Regex("<(?<tag>[a-zA-Z]+)(?<attribute>.*)(/>|>(?<Content>.*)</(\\k<tag>)>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex s_attribute = new Regex("([a-zA-Z]+=\"(.*[^\"])\")");

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TagExtractorTestTransformer"/> class.
        /// </summary>
        public TagExtractorTestTransformer(ILogger<TagExtractorTestTransformer> logger)
            : base(logger)
        {
        }

        /// <summary>
        /// Extracts tags from HTML asynchronous.
        /// </summary>
        public Task<IReadOnlyCollection<Tag>> ExtractTagFromHtmlAsync(string html, IExecutionContext sequenceContext)
        {
            var result = EnumerableHelper<Tag>.ReadOnly;

            if (!string.IsNullOrEmpty(html))
            {
                result = s_simpleTag.Matches(html)
                                    .Where(x => x.Success)
                                    .Select(t => new Tag(t.Value, s_attribute.Matches(t.Groups["attribute"].Value).Select(s => s.Value).ToArray(), null))
                                    .ToArray();
            }

            return Task.FromResult(result);
        }
    }
}
