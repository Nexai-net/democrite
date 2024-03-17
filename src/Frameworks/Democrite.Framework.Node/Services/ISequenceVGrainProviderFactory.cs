// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions.Customizations;

    using Elvex.Toolbox.Abstractions.Disposables;

    /// <summary>
    /// Service used to provide a dedicated <see cref="ISequenceVGrainProvider"/>
    /// </summary>
    internal interface ISequenceVGrainProviderFactory
    {
        /// <summary>
        /// Gets the grain provider dedicated based on
        /// </summary>
        ISafeDisposable<ISequenceVGrainProvider> GetProvider(in ExecutionCustomizationDescriptions? customization);
    }
}
