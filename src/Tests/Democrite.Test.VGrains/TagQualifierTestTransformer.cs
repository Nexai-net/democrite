// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Test.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Test.Interfaces;

    using Microsoft.Extensions.Logging;

    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public sealed class TagQualifierTestTransformer : VGrainBase<ITagQualifierTestTransformer>, ITagQualifierTestTransformer
    {
        #region Fields

        private static readonly Regex s_tagName = new Regex("<(?<tag>[a-zA-Z]+)", RegexOptions.Compiled);

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TagQualifierTestTransformer"/> class.
        /// </summary>
        public TagQualifierTestTransformer(ILogger<TagQualifierTestTransformer> logger)
            : base(logger)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<TagQualify> QualifyTagAsync(Tag tag, IExecutionContext sequenceContext)
        {
            var match = s_tagName.Match(tag.OriginHtml);

            var tagName = "";
            if (match.Success)
                tagName = match.Groups["tag"].Value;

            return Task.FromResult(new TagQualify(tag, tagName, null));
        }

        #endregion
    }
}
