﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto configure the VGrain state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public sealed class AutoCustomGrainMemoryMongoConfigurator : INodeCustomGrainMemoryAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  string sourceConfigurationKey,
                                  string targetKey)
        {
            AutoConfigureCustomStorage(democriteBuilderWizard, configuration, serviceCollection, logger, targetKey, false);
        }

        /// <inheritdoc />
        public void AutoConfigureCustomStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                               IConfiguration configuration,
                                               IServiceCollection serviceCollection,
                                               ILogger logger,
                                               string key,
                                               bool buildReadRepository = false)
        {
            var builder = DemocriteMongoBuilderDemocriteWizardStartExtensions.GetBuilder(serviceCollection, configuration, democriteBuilderWizard);
            builder.SetupGrainStateStorage(key, buildRepository: buildReadRepository);
        }
    }
}
