// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations.Builders
{
    using Democrite.Framework.Configurations;

    /// <summary>
    /// Define a service allowing custom addin configuration to democrite evironement 
    /// </summary>
    /// <seealso cref="IBuilderDemocriteBaseWizard" />
    /// <seealso cref="IDemocriteBaseGenericBuilder" />
    public interface IDemocriteExtensionBuilderTool : IBuilderDemocriteBaseWizard, IDemocriteBaseGenericBuilder
    {
        /// <summary>
        /// Try get specific builder
        /// </summary>
        TBuilder TryGetBuilder<TBuilder>();
    }
}
