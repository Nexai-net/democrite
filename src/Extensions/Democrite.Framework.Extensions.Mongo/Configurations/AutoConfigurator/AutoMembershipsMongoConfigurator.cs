// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Extensions.Mongo.Models;
    using Democrite.Framework.Extensions.Mongo.Services;

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
            {
                cl.AddMembershipTable<MongoMembershipTable>();
                cl.AddConfigurationValidator<MongoDBOptionsValidator<MongoDBMembershipTableOptions>>();
            }
            else
            {
                cl.AddGatewayListProvider<MongoGatewayListProvider>();
                cl.AddConfigurationValidator<MongoDBOptionsValidator<MongoDBGatewayListProviderOptions>>();
            }

            MongoDBOptions? genericOptions = option;

            if (option is null)
            {
                if (cl.IsServerNode)
                    genericOptions = cl.AddExtensionOption(ConfigurationSectionNames.ClusterMembership, new MongoDBMembershipTableOptions());
                else
                    genericOptions = cl.AddExtensionOption(ConfigurationSectionNames.ClusterMembership, new MongoDBGatewayListProviderOptions());
            }
            else
            {
                if (cl.IsServerNode)
                {
                    cl.AddExtensionOption(option);
                }
                else
                {
                    var clientCopy = new MongoDBGatewayListProviderOptions()
                    {
                        ClientName = option.ClientName,
                        CollectionConfigurator = option.CollectionConfigurator, 
                        CollectionPrefix = option.CollectionPrefix,
                        CreateShardKeyForCosmos = option.CreateShardKeyForCosmos,
                        DatabaseName = option.DatabaseName,
                        Strategy = option.Strategy
                    };

                    cl.AddExtensionOption(clientCopy);
                }
            }

            serviceCollection.PostConfigure<MongoDBMembershipTableOptions>(m => m.DatabaseName ??= nameof(Democrite).ToLower());
            serviceCollection.PostConfigure<MongoDBGatewayListProviderOptions>(m => m.DatabaseName ??= nameof(Democrite).ToLower());

            MongoConfigurator.GetInstance(serviceCollection)
                             .SetupMongoConnectionInformation(serviceCollection,
                                                              configuration,
                                                              genericOptions!,
                                                              MongoDBConnectionOptions.DEMOCRITE_CLUSTER,
                                                              ConfigurationSectionNames.ClusterMembershipConnectionStringKey,
                                                              connectionString);
        }
    }
}
