// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;

    /// <summary>
    /// Define a service able to automatically configure the membership table for client/node 
    /// </summary>
    /// <seealso cref="IAutoConfigurator" />
    public interface IMembershipsAutoConfigurator : IAutoConfigurator<IDemocriteClusterBuilder>
    {
    }
}
