// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.Configurations;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers;
    using Orleans.Providers.MongoDB.Configuration;

    using System;

    /// <summary>
    /// Auto configure the VGrain state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public sealed class AutoDefaultMemoryMongoConfigurator : AutoBaseMemoryStorageMongoConfigurator, INodeDefaultMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoDefaultMemoryMongoConfigurator"/> class.
        /// </summary>
        static AutoDefaultMemoryMongoConfigurator()
        {
            Default = new AutoDefaultMemoryMongoConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoDefaultMemoryMongoConfigurator Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            ConfigureDefaultMongoStorage(democriteBuilderWizard,
                                         configuration,
                                         serviceCollection,
                                         logger,
                                         null,
                                         null);
        }

        /// <summary>
        /// Configures the mongo as reminder storage.
        /// </summary>
        internal void ConfigureDefaultMongoStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                   IConfiguration configuration,
                                                   IServiceCollection serviceCollection,
                                                   ILogger logger,
                                                   string? connectionString,
                                                   MongoDBOptions? option)
        {
            ConfigureMongoStorage<MongoDBGrainStorageOptions>(democriteBuilderWizard,
                                                              configuration,
                                                              serviceCollection,
                                                              logger,
                                                              connectionString,
                                                              option,
                                                              ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME,
                                                              ConfigurationNodeSectionNames.NodeDefaultMemoryAutoConfigKey,
                                                              ConfigurationNodeSectionNames.NodeDefaultMemoryConnectionString);
        }

        #endregion
    }
}
