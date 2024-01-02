// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Node.Configurations; to easy configuration use
namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Cluster.Configurations.Builders;
    using Democrite.Framework.Node.Mongo.Configurations;
    using Democrite.Framework.Node.Mongo.Configurations.AutoConfigurator;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Node.Abstractions.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;

    using Orleans.Providers.MongoDB.Configuration;

    using System;
    using Orleans.Providers;

    /// <summary>
    /// Extensions use to setup AdoNet memory cluster from <see cref="IClusterNodeBuilderDemocriteWizardStart"/>
    /// </summary>

    public static class DemocriteMongoBuilderDemocriteWizardStartExtensions
    {
        #region Methods

        /// <summary>
        /// Setup Mongo db cluster synchronication
        /// </summary>
        public static TWizard UseMongoCluster<TWizard, TWizardConfig>(this IDemocriteWizardStart<TWizard, TWizardConfig> wizard,
                                                                      Action<IDemocriteClusterExternalBuilder<MongoDBMembershipTableOptions>>? buildOption = null)
            where TWizard : IDemocriteWizard<TWizard, TWizardConfig>
            where TWizardConfig : IDemocriteCoreConfigurationWizard<TWizardConfig>
        {
            return wizard.SetupCluster((cl, cfg) =>
            {
                var builder = new DemocriteExternalClusterBuilder<MongoDBMembershipTableOptions>(cfg);

                buildOption?.Invoke(builder);

                var buildResult = builder.Build();

                AutoMembershipsMongoConfigurator.MongoMemberShipsConfiguration(cl,
                                                                               cl.GetServiceCollection(),
                                                                               cfg,
                                                                               buildResult.ConnectionString,
                                                                               buildResult.Option);
            });
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        public static IDemocriteNodeMemoryBuilder UseMongoStorage(this IDemocriteNodeMemoryBuilder wizard,
                                                                  Action<IDemocriteMongoStorageConfiguration> configuration)
        {
            var cfg = new DemocriteMongoStorageConfiguration(wizard,
                                                             wizard.GetServiceCollection(),
                                                             wizard.GetConfiguration());

            configuration(cfg);
            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IDemocriteNodeMemoryBuilder UseMongoStorage(this IDemocriteNodeMemoryBuilder wizard,
                                                                  StorageTypeEnum storageType = StorageTypeEnum.All,
                                                                  string? connectionString = null,
                                                                  string? database = null,
                                                                  string? collectionPrefix = null)
        {
            return UseMongoStorage(wizard, b =>
            {
                if (!string.IsNullOrEmpty(connectionString))
                    b.ConnectionString(connectionString, database ?? nameof(Democrite).ToLower(), collectionPrefix);

                b.Store(storageType, database, collectionPrefix);
            });
        }

        /// <summary>
        /// Adds the mongo as a definition provider.
        /// </summary>
        public static IDemocriteNodeWizard AddMongoDefinitionProvider(this IDemocriteNodeWizard wizard,
                                                                      Action<IDemocriteClusterExternalBuilder<MongoDBOptions>> buildOption,
                                                                      string uniqueKeyName = ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)
        {
            wizard.SetupNodeMemories(memory =>
            {
                var builder = new DemocriteExternalClusterBuilder<MongoDBOptions>(memory.GetConfiguration());
                buildOption?.Invoke(builder);

                var buildResult = builder.Build();

                AutoCustomDefinitionProviderMongoConfigurator.Default
                                                             .ConfigureMongoProviderStorage(memory,
                                                                                            memory.GetConfiguration(),
                                                                                            memory.GetServiceCollection(),
                                                                                            memory.Logger,
                                                                                            buildResult.ConnectionString,
                                                                                            buildResult.Option,
                                                                                            uniqueKeyName);
            });

            return wizard;
        }

        #endregion
    }
}
