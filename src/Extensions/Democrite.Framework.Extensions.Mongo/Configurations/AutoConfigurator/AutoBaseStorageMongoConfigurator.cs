// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Extensions.Mongo.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers.MongoDB.Configuration;

    using System;

    /// <summary>
    /// Auto configure the VGrain state storage
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public abstract class AutoBaseStorageMongoConfigurator
    {
        /// <summary>
        /// Configures the mongo as storage.
        /// </summary>
        protected void ConfigureMongoStorage<TOption>(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                      IConfiguration configuration,
                                                      IServiceCollection serviceCollection,
                                                      ILogger _,
                                                      string? connectionString,
                                                      MongoDBOptions? option,
                                                      string key,
                                                      string configurationStringKey,
                                                      string configurationConnectionStringStringKey,
                                                      bool buildRepository = false)

            where TOption : MongoDBOptions, new()
        {
#pragma warning disable IDE0019 // Use pattern matching
            var siloBuilder = democriteBuilderWizard.SourceOrleanBuilder as ISiloBuilder;
#pragma warning restore IDE0019 // Use pattern matching

            if (democriteBuilderWizard.IsClient || siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");

            option ??= new TOption();

            var opt = democriteBuilderWizard.AddExtensionOption(configurationStringKey,
                                                                option!);

            MongoConfigurator.GetInstance(serviceCollection)
                             .SetupMongoConnectionInformation(serviceCollection,
                                                              configuration,
                                                              opt,
                                                              key,
                                                              configurationConnectionStringStringKey,
                                                              connectionString);

            RegisterStorage(democriteBuilderWizard,
                            configuration,
                            serviceCollection,
                            key,
                            siloBuilder,
                            opt,
                            buildRepository);
        }

        /// <summary>
        /// Registers the storage.
        /// </summary>
        protected abstract void RegisterStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                IConfiguration configuration,
                                                IServiceCollection serviceCollection,
                                                string key,
                                                ISiloBuilder siloBuilder,
                                                MongoDBOptions opt,
                                                bool buildRepository = false);
    }
}
