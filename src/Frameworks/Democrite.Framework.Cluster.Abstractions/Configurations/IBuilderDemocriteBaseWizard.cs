// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Base wizard contract root to every other wizard
    /// </summary>
    public interface IBuilderDemocriteBaseWizard
    {
        /// <summary>
        /// Gets the DPI service descriptors.
        /// </summary>
        IServiceCollection GetServiceCollection();
    }
}
