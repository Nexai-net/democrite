﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.Configurations;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Auto configure the VGrain state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public sealed class AutoDemocriteMongoConfigurator : AutoBaseStorageMongoConfigurator, INodeDemocriteMemoryAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            ConfigureMongoStorage(democriteBuilderWizard,
                                          configuration,
                                          serviceCollection,
                                          logger,
                                          null,
                                          null);
        }

        /// <summary>
        /// Configures the mongo as reminder storage.
        /// </summary>
        internal static void ConfigureMongoStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                   IConfiguration configuration,
                                                   IServiceCollection serviceCollection,
                                                   ILogger logger,
                                                   string? connectionString,
                                                   MongoDBOptions? option)
        {
            AutoBaseStorageMongoConfigurator.ConfigureMongoStorage(democriteBuilderWizard,
                                                                   configuration,
                                                                   serviceCollection,
                                                                   logger,
                                                                   connectionString,
                                                                   option,
                                                                   nameof(Democrite),
                                                                   ConfigurationNodeSectionNames.NodeDemocriteSystemMemory,
                                                                   ConfigurationNodeSectionNames.NodeDemocriteSystemMemoryConnectionString);
        }
    }
}
