// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Test.Interfaces
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Elvex.Toolbox;

    /// <summary>
    /// Provide Html for testing purpose
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainUsage<NoneType, string>]
    public interface IHtmlProviderTestTransformer : IVGrain
    {
        /// <summary>
        /// Gets a test HTML
        /// </summary>
        Task<string> GetHtmlAsync(IExecutionContext sequenceContext);
    }
}