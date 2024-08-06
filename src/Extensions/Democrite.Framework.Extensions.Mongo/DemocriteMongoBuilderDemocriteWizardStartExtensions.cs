// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Configurations; to easy configuration use
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Cluster.Configurations.Builders;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Extensions.Mongo.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Providers;
    using Orleans.Providers.MongoDB.Configuration;

    using System;

    /// <summary>
    /// Extensions use to setup AdoNet memory cluster from <see cref="IClusterNodeBuilderDemocriteWizardStart"/>
    /// </summary>

    public static class DemocriteMongoBuilderDemocriteWizardStartExtensions
    {
        #region Fields

        private static readonly Type s_builderTraits = typeof(IDemocriteMongoStorageBuilder);

        #endregion

        #region Methods

        /// <summary>
        /// Setup Mongo db cluster synchronication
        /// </summary>
        public static TWizard UseMongoCluster<TWizard, TWizardConfig>(this IDemocriteWizardStart<TWizard, TWizardConfig> wizard,
                                                                      Action<IDemocriteClusterExternalBuilder<MongoDBMembershipTableOptions>>? buildOption = null,
                                                                      string? serviceId = null,
                                                                      string? clusterId = null)
            where TWizard : IDemocriteWizard<TWizard, TWizardConfig>
            where TWizardConfig : IDemocriteCoreConfigurationWizard<TWizardConfig>
        {
            return wizard.SetupCluster((cl, cfg) =>
            {
                var builder = new DemocriteExternalClusterBuilder<MongoDBMembershipTableOptions>(cfg);

                buildOption?.Invoke(builder);

                var buildResult = builder.Build();

                var mongoBuilder = GetBuilder(cl.GetServiceCollection(), cl.GetConfiguration(), cl);
                mongoBuilder.SetupMongoCluster(cl, buildResult.ConnectionString, buildResult.Option, serviceId, clusterId);
            });
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IDemocriteNodeMemoryBuilder UseMongo(this IDemocriteNodeMemoryBuilder wizard,
                                                           StorageTypeEnum storageType = StorageTypeEnum.All,
                                                           string? connectionString = null,
                                                           string? database = null,
                                                           string? collectionPrefix = null)
        {
            var mongoBuilder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);

            mongoBuilder.Store(storageType, connectionString, database, collectionPrefix);
            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IDemocriteNodeMemoryBuilder UseMongo(this IDemocriteNodeMemoryBuilder wizard,
                                                           Action<IDemocriteMongoStorageBuilder> builderAction)
        {
            var mongoBuilder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builderAction(mongoBuilder);
            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IDemocriteNodeMemoryBuilder UseMongoGrainStorage(this IDemocriteNodeMemoryBuilder wizard,
                                                                       string key,
                                                                       string? connectionString = null,
                                                                       string? database = null,
                                                                       string? collectionPrefix = null,
                                                                       bool buildRepository = false)
        {
            var builder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builder.SetupGrainStateStorage(key,
                                           connectionString,
                                           database,
                                           collectionPrefix,
                                           buildRepository);

            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IDemocriteNodeMemoryBuilder UseMongoGrainStorage(this IDemocriteNodeMemoryBuilder wizard,
                                                                       IReadOnlyCollection<string> keys,
                                                                       string? connectionString = null,
                                                                       string? database = null,
                                                                       string? collectionPrefix = null,
                                                                       bool buildRepository = false)
        {
            var builder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builder.SetupGrainStateStorage(keys,
                                           connectionString,
                                           database,
                                           collectionPrefix,
                                           buildRepository);

            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage as repository
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IDemocriteNodeMemoryBuilder UseMongoRepository(this IDemocriteNodeMemoryBuilder wizard,
                                                                     string key,
                                                                     string? connectionString = null,
                                                                     string? database = null,
                                                                     string? collectionPrefix = null)
        {
            var builder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builder.SetupRepositoryStorage(key,
                                           connectionString,
                                           database,
                                           collectionPrefix);

            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage as repository
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IDemocriteNodeMemoryBuilder UseMongoRepository(this IDemocriteNodeMemoryBuilder wizard,
                                                                 IReadOnlyCollection<string> keys,
                                                                 string? connectionString = null,
                                                                 string? database = null,
                                                                 string? collectionPrefix = null)
        {
            var builder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builder.SetupRepositoryStorage(keys,
                                           connectionString,
                                           database,
                                           collectionPrefix);

            return wizard;
        }

        /// <summary>
        /// Adds the mongo as a definition provider.
        /// </summary>
        public static IDemocriteNodeWizard AddMongoDefinitionProvider(this IDemocriteNodeWizard wizard,
                                                                      Action<IDemocriteClusterExternalBuilder<MongoDBOptions>> buildOption,
                                                                      string uniqueKeyName = ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)
        {
            //wizard.SetupNodeMemories(memory =>
            //{
            //    var builder = new DemocriteExternalClusterBuilder<MongoDBOptions>(memory.GetConfiguration());
            //    buildOption?.Invoke(builder);

            //    var buildResult = builder.Build();

            //    AutoCustomDefinitionProviderMongoConfigurator.Default
            //                                                 .ConfigureMongoProviderStorage(memory,
            //                                                                                memory.GetConfiguration(),
            //                                                                                memory.GetServiceCollection(),
            //                                                                                memory.Logger,
            //                                                                                buildResult.ConnectionString,
            //                                                                                buildResult.Option,
            //                                                                                uniqueKeyName);
            //});

            //return wizard;

            throw new NotImplementedException();
        }

        #region Tools

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public static IDemocriteMongoStorageBuilder GetBuilder(IServiceCollection services, IConfiguration configuration, IDemocriteBaseGenericBuilder democriteBaseGenericBuilder)
        {
            var builder = services.FirstOrDefault(s => s.IsKeyedService == false &&
                                                       s.ServiceType == s_builderTraits &&
                                                       s.ImplementationInstance is not null)?.ImplementationInstance as IDemocriteMongoStorageBuilder;

            if (builder == null)
            {
                builder = new DemocriteMongoStorageBuilder(services, configuration, democriteBaseGenericBuilder);
                services.AddSingleton<IDemocriteMongoStorageBuilder>(builder);
            }

            return builder;
        }

        #endregion

        #endregion
    }
}
