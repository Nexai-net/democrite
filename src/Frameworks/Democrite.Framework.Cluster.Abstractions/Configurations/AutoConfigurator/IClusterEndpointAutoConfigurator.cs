// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;

    /// <summary>
    /// Configurator in charge to define how the current cluster part will expose (PORT, GATEWAY ...)
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IDemocriteClusterBuilder}" />
    public interface IClusterEndpointAutoConfigurator : IAutoConfigurator<IDemocriteClusterBuilder>
    {
    }
}
