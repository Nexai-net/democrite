// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Models;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Auto configure the VGrain state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public sealed class AutoCustomGrainMemoryMongoConfigurator : AutoBaseMemoryStorageMongoConfigurator, INodeCustomGrainMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoCustomGrainMemoryMongoConfigurator"/> class.
        /// </summary>
        static AutoCustomGrainMemoryMongoConfigurator()
        {
            Default = new AutoCustomGrainMemoryMongoConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoCustomGrainMemoryMongoConfigurator Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  string sourceConfigurationKey,
                                  string targetKey)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(targetKey);

            var option = configuration.GetSection(sourceConfigurationKey).Get<DefaultGrainStorageOption>();
            AutoConfigureCustomStorage(democriteBuilderWizard,
                                       configuration,
                                       serviceCollection,
                                       logger,
                                       targetKey,
                                       option?.BuildReadRepository ?? false);
        }

        /// <inheritdoc />
        public void AutoConfigureCustomStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                               IConfiguration configuration,
                                               IServiceCollection serviceCollection,
                                               ILogger logger,
                                               string key,
                                               bool buildRepository)
        {
            ConfigureMongoStorages(democriteBuilderWizard,
                                   configuration,
                                   serviceCollection,
                                   logger,
                                   null,
                                   null,
                                   buildRepository,
                                   key.AsEnumerable().ToArray());
        }

        /// <summary>
        /// Configures the mongo as reminder storage.
        /// </summary>
        internal void ConfigureMongoStorages(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                             IConfiguration configuration,
                                             IServiceCollection serviceCollection,
                                             ILogger logger,
                                             string? connectionString,
                                             MongoDBOptions? option,
                                             bool buildRepository,
                                             IReadOnlyCollection<string> keys)
        {
            foreach (var key in keys)
            {
               base.ConfigureMongoStorage<MongoDBOptions>(democriteBuilderWizard,
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
                                                          ConfigurationSectionNames.ConnectionString,
                                                          buildRepository);
            }
        }

        #endregion
    }
}
