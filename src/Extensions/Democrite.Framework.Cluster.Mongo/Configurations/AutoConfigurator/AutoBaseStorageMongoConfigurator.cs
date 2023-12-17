// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Mongo
{
    using Democrite.Framework.Cluster.Mongo.Services;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers.MongoDB.Configuration;

    using System;

    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Orleans.Runtime;
    using Orleans.Storage;
    using Orleans.Providers;

    /// <summary>
    /// Auto configure the VGrain state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public abstract class AutoBaseStorageMongoConfigurator
    {
        /// <summary>
        /// Configures the mongo as storage.
        /// </summary>
        protected static void ConfigureMongoStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                    IConfiguration configuration,
                                                    IServiceCollection serviceCollection,
                                                    ILogger _,
                                                    string? connectionString,
                                                    MongoDBOptions? option,
                                                    string key,
                                                    string configurationStringKey,
                                                    string configurationConnectionStringStringKey)
        {
            var siloBuilder = democriteBuilderWizard.SourceOrleanBuilder as ISiloBuilder;

            if (democriteBuilderWizard.IsClient || siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");

            option ??= new MongoDBGrainStorageOptions();

            var opt = democriteBuilderWizard.AddExtensionOption(configurationStringKey,
                                                                option!);

            MongoConfigurator.GetInstance(serviceCollection)
                             .SetupMongoConnectionInformation(serviceCollection,
                                                              configuration,
                                                              opt,
                                                              key,
                                                              configurationConnectionStringStringKey,
                                                              connectionString);

            //if (!string.Equals(key, ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, StringComparison.OrdinalIgnoreCase))
            //    serviceCollection.TryAddSingleton(sp => sp.GetServiceByName<IGrainStorage>(key));

            siloBuilder.AddMongoDBGrainStorage(key, o => o.DatabaseName ??= opt.DatabaseName);
        }
    }
}
