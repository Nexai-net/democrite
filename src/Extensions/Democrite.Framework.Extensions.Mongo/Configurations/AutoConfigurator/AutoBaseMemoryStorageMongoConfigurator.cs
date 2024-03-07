// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Extensions.Mongo.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Hosting;
    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Runtime;

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
                                                MongoDBOptions opt,
                                                bool buildRepository = false)
        {
            siloBuilder.AddMongoDBGrainStorage(key, o => o.DatabaseName ??= opt.DatabaseName);

            if (buildRepository)
            {
                siloBuilder.Services.RemoveKeyedService<string, IRepositorySpecificFactory>(key);
                //siloBuilder.Services.TryAddSingleton<MongoSpecificRepositoryFactory, MongoSpecificRepositoryFactory>();
                siloBuilder.Services.AddSingletonNamedService<IRepositorySpecificFactory>(key, (p, storageName) => ActivatorUtilities.CreateInstance<MongoStateSpecificRepositoryFactory>(p, storageName));
            }
        }
    }
}
