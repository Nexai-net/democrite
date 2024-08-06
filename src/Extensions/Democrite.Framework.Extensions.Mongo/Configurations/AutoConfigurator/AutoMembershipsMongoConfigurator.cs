// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Configurator used to auto inject mongo configuration when <see cref="AutoConfiguratorKeyAttribute"/> 'mongo' is used
    /// </summary>
    public sealed class AutoMembershipsMongoConfigurator : IMembershipsAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteClusterBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger _)
        {
            var builder = DemocriteMongoBuilderDemocriteWizardStartExtensions.GetBuilder(serviceCollection, configuration, democriteBuilderWizard);
            builder.SetupMongoCluster(democriteBuilderWizard);
        }
    }
}
