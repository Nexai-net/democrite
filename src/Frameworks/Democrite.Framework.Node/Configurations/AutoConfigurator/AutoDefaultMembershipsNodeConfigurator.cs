// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Configurator in charge to setup the default services
    /// </summary>
    /// <seealso cref="IMembershipsAutoConfigurator" />
    public sealed class AutoDefaultMembershipsNodeConfigurator : IMembershipsAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteClusterBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceDescriptors,
                                  ILogger logger)
        {
            var siloBuilder = democriteBuilderWizard.GetSiloBuilder();
            siloBuilder.UseLocalhostClustering();
        }
    }
}
