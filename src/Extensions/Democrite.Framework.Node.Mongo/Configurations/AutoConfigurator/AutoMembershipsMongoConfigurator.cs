﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Mongo.Models;
    using Democrite.Framework.Node.Mongo.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers.MongoDB.Configuration;

    using Orleans.Providers.MongoDB.Membership;

    /// <summary>
    /// Configurator used to auto inject mongo configuration when <see cref="AutoConfiguratorKeyAttribute"/> 'mongo' is used
    /// </summary>
    public sealed class AutoMembershipsMongoConfigurator : IMembershipsAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteClusterBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger _)
        {
            MongoMemberShipsConfiguration(democriteBuilderWizard, serviceCollection, configuration);
        }

        /// <summary>
        /// Centralize configuration to use mongo as rendez vous point on the cluster
        /// </summary>
        internal static void MongoMemberShipsConfiguration(IDemocriteClusterBuilder cl,
                                                           IServiceCollection serviceCollection,
                                                           IConfiguration configuration,
                                                           string? connectionString = null,
                                                           MongoDBMembershipTableOptions? option = null)
        {
            if (cl.IsServerNode)
                cl.AddMembershipTable<MongoMembershipTable>();
            else
                cl.AddGatewayListProvider<MongoGatewayListProvider>();

            cl.AddConfigurationValidator<MongoDBOptionsValidator<MongoDBMembershipTableOptions>>();

            if (option == null)
                option = cl.AddExtensionOption(ConfigurationSectionNames.ClusterMembership, new MongoDBMembershipTableOptions());
            else
                cl.AddExtensionOption(option);

            serviceCollection.PostConfigure<MongoDBMembershipTableOptions>(m => m.DatabaseName ??= nameof(Democrite).ToLower());
            serviceCollection.PostConfigure<MongoDBGatewayListProviderOptions>(m => m.DatabaseName ??= nameof(Democrite).ToLower());

            MongoConfigurator.GetInstance(serviceCollection)
                             .SetupMongoConnectionInformation(serviceCollection,
                                                              configuration,
                                                              option,
                                                              MongoDBConnectionOptions.DEMOCRITE_CLUSTER,
                                                              ConfigurationSectionNames.ClusterMembershipConnectionStringKey,
                                                              connectionString);
        }
    }
}