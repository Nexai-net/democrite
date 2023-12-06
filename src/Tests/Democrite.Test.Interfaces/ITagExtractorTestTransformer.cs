// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Test.Interfaces
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Test VGrain that extract html tag in a html code
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainUsage<string, IReadOnlyCollection<Tag>>]
    public interface ITagExtractorTestTransformer : IVGrain
    {
        /// <summary>
        /// Extracts tags from HTML asynchronous.
        /// </summary>
        Task<IReadOnlyCollection<Tag>> ExtractTagFromHtmlAsync(string html, IExecutionContext sequenceContext);
    }
}
