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
    /// Auto configure the VGrain state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public sealed class AutoDemocriteDynamicDefinitionsMongoConfigurator : AutoBaseMemoryStorageMongoConfigurator, INodeDemocriteDynamicDefinitionsMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoDemocriteDynamicDefinitionsMongoConfigurator"/> class.
        /// </summary>
        static AutoDemocriteDynamicDefinitionsMongoConfigurator()
        {
            Default = new AutoDemocriteDynamicDefinitionsMongoConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoDemocriteDynamicDefinitionsMongoConfigurator Default { get; }

        #endregion

        #region Methods

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
        internal void ConfigureMongoStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
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
                                                              DemocriteConstants.DefaultDemocriteDynamicDefinitionsConfigurationKey,
                                                              ConfigurationNodeSectionNames.NodeDemocriteDynamicDefinitionsMemory,
                                                              ConfigurationNodeSectionNames.NodeDemocriteDynamicDefinitionsMemoryConnectionString);

            AutoCustomRepositoryMongoConfigurator.Default.ConfigureMongoStorages(democriteBuilderWizard,
                                                                                 configuration,
                                                                                 serviceCollection,
                                                                                 logger,
                                                                                 connectionString,
                                                                                 option,
                                                                                 DemocriteConstants.DefaultDemocriteDynamicDefinitionsRepositoryConfigurationKey.AsEnumerable().ToReadOnly());
        }

        #endregion
    }
}
