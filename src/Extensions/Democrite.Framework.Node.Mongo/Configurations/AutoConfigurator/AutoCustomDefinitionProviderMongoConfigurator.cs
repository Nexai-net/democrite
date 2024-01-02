﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.Configurations;
    using Democrite.Framework.Node.Mongo.Models;
    using Democrite.Framework.Node.Mongo.Services;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using MongoDB.Bson.Serialization;

    using Orleans.Hosting;
    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.StorageProviders;
    using Orleans.Providers.MongoDB.Utils;
    using Orleans.Runtime;

    using System.Xml.Linq;

    /// <summary>
    /// Auto configure the VGrain state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public sealed class AutoCustomDefinitionProviderMongoConfigurator : AutoBaseStorageMongoConfigurator, INodeCustomDefinitionProviderAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoCustomDefinitionProviderMongoConfigurator"/> class.
        /// </summary>
        static AutoCustomDefinitionProviderMongoConfigurator()
        {
            Default = new AutoCustomDefinitionProviderMongoConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoCustomDefinitionProviderMongoConfigurator Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            throw new NotSupportedException("Call ConfigureMongoStorage to pass the key");
        }

        /// <inheritdoc />
        public void AutoConfigureCustomProvider(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                IConfiguration configuration,
                                                IServiceCollection serviceCollection,
                                                ILogger logger,
                                                string key)
        {
            ConfigureMongoProviderStorage(democriteBuilderWizard,
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
        internal void ConfigureMongoProviderStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                    IConfiguration configuration,
                                                    IServiceCollection serviceCollection,
                                                    ILogger logger,
                                                    string? connectionString,
                                                    MongoDBOptions? option,
                                                    params string[] keys)
        {
            foreach (var key in keys)
            {
                ConfigureMongoStorage<MongoDBOptions>(democriteBuilderWizard,
                                                      configuration,
                                                      serviceCollection,
                                                      logger,
                                                      connectionString,
                                                      option,
                                                      
                                                      // Key add prefix to prevent conflict
                                                      ConfigurationNodeSectionNames.NodeDefinitionProvider +
                                                      ConfigurationSectionNames.SectionSeparator + 
                                                      key,
                                                      
                                                      ConfigurationNodeSectionNames.NodeDefinitionProvider +
                                                      ConfigurationSectionNames.SectionSeparator +
                                                      key,
                                                      
                                                      ConfigurationNodeSectionNames.NodeDefinitionProvider +
                                                      ConfigurationSectionNames.SectionSeparator +
                                                      key +
                                                      ConfigurationSectionNames.SectionSeparator +
                                                      ConfigurationSectionNames.ConnectionString);
            }
        }

        /// <inheritdoc />
        protected override void RegisterStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                IConfiguration configuration,
                                                IServiceCollection services,
                                                string key,
                                                ISiloBuilder siloBuilder,
                                                MongoDBOptions opt)
        {
            AddDefinitionProviderSource<SequenceMongoDefinitionProviderSource, ISequenceDefinitionSourceProvider>(services, key);
            AddDefinitionProviderSource<TriggerMongoDefinitionProviderSource, ITriggerDefinitionProviderSource>(services, key);
            AddDefinitionProviderSource<SignalMongoDefinitionProviderSource, ISignalDefinitionProviderSource>(services, key);
            AddDefinitionProviderSource<DoorMongoDefinitionProviderSource, IDoorDefinitionProviderSource>(services, key);
        }

        /// <summary>
        /// Adds the definition provider source.
        /// </summary>
        private static void AddDefinitionProviderSource<TType, TInterface>(IServiceCollection services, string key)
            where TType : class, TInterface
            where TInterface : class
        {
            services.AddSingletonNamedService(key, (p, k) => (TInterface)Activator.CreateInstance(typeof(TType),
                                                                                                  new object[]
                                                                                                  {
                                                                                                      p.GetRequiredService<IMongoClientFactory>(),
                                                                                                      p.GetServiceByName<MongoDBConnectionOptions>(key),
                                                                                                      key
                                                                                                  })!);

            services.AddSingleton<TInterface>(p => p.GetServiceByName<TInterface>(key));
        }

        #endregion
    }
}
