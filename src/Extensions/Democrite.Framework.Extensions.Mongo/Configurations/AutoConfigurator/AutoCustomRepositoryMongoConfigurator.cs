// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Extensions.Mongo.Services;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Models;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Hosting;
    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Runtime;

    /// <summary>
    /// Auto configure the repository storage
    /// </summary>
    /// <seealso cref="INodeCustomRepositoryMemoryAutoConfigurator" />
    public sealed class AutoCustomRepositoryMongoConfigurator : AutoBaseMemoryStorageMongoConfigurator, INodeCustomRepositoryMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoCustomGrainMemoryMongoConfigurator"/> class.
        /// </summary>
        static AutoCustomRepositoryMongoConfigurator()
        {
            Default = new AutoCustomRepositoryMongoConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoCustomRepositoryMongoConfigurator Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void RegisterStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                IConfiguration configuration,
                                                IServiceCollection serviceCollection,
                                                string key,
                                                ISiloBuilder siloBuilder,
                                                MongoDBOptions opt,
                                                bool buildRepository = false)
        {
            siloBuilder.Services.AddSingletonNamedService<IRepositorySpecificFactory>(key, (p, k) => ActivatorUtilities.CreateInstance<MongoStorageSpecificRepositoryFactory>(p, k));
        }

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  string sourceConfigurationKey,
                                  string targetKey)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(targetKey);

            //var option = configuration.GetSection(sourceConfigurationKey).Get<DefaultGrainStorageOption>();
            AutoConfigureCustomStorage(democriteBuilderWizard,
                                       configuration,
                                       serviceCollection,
                                       logger,
                                       targetKey);
        }

        /// <inheritdoc />
        public void AutoConfigureCustomStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                               IConfiguration configuration,
                                               IServiceCollection serviceCollection,
                                               ILogger logger,
                                               string key)
        {
            ConfigureMongoStorages(democriteBuilderWizard,
                                   configuration,
                                   serviceCollection,
                                   logger,
                                   null,
                                   null,
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

                                                           ConfigurationNodeSectionNames.NodeRepositoryStorages +
                                                           ConfigurationSectionNames.SectionSeparator +
                                                           key,

                                                           ConfigurationNodeSectionNames.NodeRepositoryStorages +
                                                           ConfigurationSectionNames.SectionSeparator +
                                                           key +
                                                           ConfigurationSectionNames.SectionSeparator +
                                                           ConfigurationSectionNames.ConnectionString,
                                                           true);
            }
        }

        #endregion
    }
}
