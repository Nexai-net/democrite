// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Node.Mongo.Models;
    using Democrite.Framework.Node.Mongo.Services;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers.MongoDB.Configuration;

    using System;

    /// <summary>
    /// Setup mongodb as storage for all reminder
    /// </summary>
    /// <seealso cref="INodeReminderStateMemoryAutoConfigurator" />
    public sealed class AutoReminderMongoConfigurator : INodeReminderStateMemoryAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            ConfigureMongoReminderStorage(democriteBuilderWizard,
                                          configuration,
                                          serviceCollection,
                                          logger,
                                          null,
                                          null);
        }

        /// <summary>
        /// Configures the mongo as reminder storage.
        /// </summary>
        internal static void ConfigureMongoReminderStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                           IConfiguration configuration,
                                                           IServiceCollection serviceCollection,
                                                           ILogger _,
                                                           string? connectionString,
                                                           MongoDBRemindersOptions? option)
        {
            var siloBuilder = democriteBuilderWizard.SourceOrleanBuilder as ISiloBuilder;

            if (democriteBuilderWizard.IsClient || siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");

            option ??= new MongoDBRemindersOptions();

            var opt = democriteBuilderWizard.AddExtensionOption(ConfigurationNodeSectionNames.NodeReminderStateMemory, option);

            MongoConfigurator.GetInstance(serviceCollection)
                             .SetupMongoConnectionInformation(serviceCollection,
                                                              configuration,
                                                              opt,
                                                              MongoDBConnectionOptions.DEMOCRITE_REMINDER,
                                                              ConfigurationNodeSectionNames.NodeReminderStateMemoryConnectionString,
                                                              connectionString);

            siloBuilder.UseMongoDBReminders(o => 
            {
                o.DatabaseName ??= o.DatabaseName;
                o.ClientName ??= o.ClientName;
                o.CollectionPrefix ??= o.CollectionPrefix;
            });
        }
    }
}
