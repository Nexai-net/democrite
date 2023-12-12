// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client.Abstractions.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;

    /// <summary>
    /// Builder used to configure a cluster client
    /// </summary>
    public interface IDemocriteClientBuilder : IDemocriteBuilder<IDemocriteClientBuilderWizard, IDemocriteCoreConfigurationWizard>
    {
    }
}
