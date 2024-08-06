// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Auto configure the democrite high system state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteAdminMemoryAutoConfigurator" />
    public sealed class AutoDemocriteAdminMongoConfigurator : INodeDemocriteAdminMemoryAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            var builder = DemocriteMongoBuilderDemocriteWizardStartExtensions.GetBuilder(serviceCollection, configuration, democriteBuilderWizard);
            builder.SetupDemocriteAdminStorage();
        }
    }
}
