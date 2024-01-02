// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Hosting;
    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="AutoBaseStorageMongoConfigurator" />
    public abstract class AutoBaseMemoryStorageMongoConfigurator : AutoBaseStorageMongoConfigurator
    {
        /// <inheritdoc />
        protected override void RegisterStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                                IConfiguration configuration,
                                                IServiceCollection serviceCollection,
                                                string key,
                                                ISiloBuilder siloBuilder,
                                                MongoDBOptions opt)
        {
            siloBuilder.AddMongoDBGrainStorage(key, o => o.DatabaseName ??= opt.DatabaseName);
        }
    }
}
