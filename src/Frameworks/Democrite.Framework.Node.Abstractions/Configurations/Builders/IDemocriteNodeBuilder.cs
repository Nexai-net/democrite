// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Orleans.Hosting;

    /// <summary>
    /// Builder used to configure a cluster node
    /// </summary>
    public interface IDemocriteNodeBuilder : IDemocriteBuilder<IDemocriteNodeWizard, IDemocriteNodeConfigurationWizard>
    {
        /// <summary>
        /// Call for a manual advance configuration
        /// </summary>
        /// <remarks>
        ///     Could create inappropriate configuration
        /// </remarks>
        ISiloBuilder ManualyAdvancedConfig();
    }
}
