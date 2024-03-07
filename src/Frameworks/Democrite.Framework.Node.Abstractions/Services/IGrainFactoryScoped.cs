// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Services
{
    using Democrite.Framework.Core.Abstractions.Customizations;

    /// <summary>
    /// Grain factory used to apply redirection between service implementations during a specific controlled scope
    /// </summary>
    public interface IGrainFactoryScoped : IGrainFactory, IDisposable
    {
        /// <summary>
        /// Get a new <see cref="IGrainFactoryScoped"/> with redirection applyed
        /// </summary>
        IGrainFactoryScoped ApplyRedirections(params VGrainRedirectionDefinition[] redirections);
    }
}
