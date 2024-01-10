// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Configurations.AutoConfigurator
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
    public sealed class AutoCustomMemoryMongoConfigurator : AutoBaseMemoryStorageMongoConfigurator, INodeCustomMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoCustomMemoryMongoConfigurator"/> class.
        /// </summary>
        static AutoCustomMemoryMongoConfigurator()
        {
            Default = new AutoCustomMemoryMongoConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoCustomMemoryMongoConfigurator Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            throw new NotSupportedException("Call AutoConfigureCustomStorage to pass the key");
        }

        /// <inheritdoc />
        public void AutoConfigureCustomStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                               IConfiguration configuration,
                                               IServiceCollection serviceCollection,
                                               ILogger logger,
                                               string key)
        {
            ConfigureMongoStorage(democriteBuilderWizard,
                                  configuration,
                                  serviceCollection,
                                  logger,
                                  null,
                                  null,
                                  key);
        }

        /// <summary>
        /// Configures the mongo as reminder storage.
        /// </summary>
        internal static void ConfigureMongoStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                   IConfiguration configuration,
                                                   IServiceCollection serviceCollection,
                                                   ILogger logger,
                                                   string? connectionString,
                                                   MongoDBOptions? option,
                                                   params string[] keys)
        {
            foreach (var key in keys)
            {
                ConfigureMongoStorage(democriteBuilderWizard,
                                      configuration,
                                      serviceCollection,
                                      logger,
                                      connectionString,
                                      option,
                                      key,
                                      
                                      ConfigurationNodeSectionNames.NodeCustomMemory + 
                                      ConfigurationSectionNames.SectionSeparator + 
                                      key,
                                      
                                      ConfigurationNodeSectionNames.NodeCustomMemory + 
                                      ConfigurationSectionNames.SectionSeparator + 
                                      key + 
                                      ConfigurationSectionNames.SectionSeparator + 
                                      ConfigurationSectionNames.ConnectionString);
            }
        }

        #endregion
    }
}
