// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Node.Models;

    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Configuration;

    public static class DemocriteMemoryInLocalConfiguration
    {
        /// <summary>
        /// Configures the memory storage options
        /// </summary>
        public static IDemocriteNodeMemoryBuilder ConfigureMemoryStorage(this IDemocriteNodeMemoryBuilder wizard, uint numStorageGrains = 10, uint initStage = MemoryGrainStorageOptions.DEFAULT_INIT_STAGE)
        {
            // MemoryGrainStorageOptions
            wizard.GetServiceCollection().AddSingleton(new DefaultMemoryGrainStorageOptions(numStorageGrains, initStage));
            return wizard;
        }

        /// <summary>
        /// Gets the in memory storage builder
        /// </summary>
        internal static IDemocriteMemoryInLocalBuilder GetBuilder(IDemocriteNodeMemoryBuilder memoryBuilder)
        {
            var services = memoryBuilder.GetServiceCollection();

            var inst = services.FirstOrDefault(s => s.IsKeyedService == false &&
                                                    s.ServiceType == typeof(IDemocriteMemoryInLocalBuilder) &&
                                                    s.ImplementationInstance is IDemocriteMemoryInLocalBuilder)?.ImplementationInstance as IDemocriteMemoryInLocalBuilder;

            if (inst is null)
            {
                inst = new DemocriteMemoryInLocalBuilder(memoryBuilder);
                services.AddSingleton<IDemocriteMemoryInLocalBuilder>(inst);
            }

            return inst;
        }

        /// <summary>
        /// Configure system storage in memory
        /// </summary>
        public static IDemocriteNodeMemoryBuilder UseInMemory(this IDemocriteNodeMemoryBuilder wizard, StorageTypeEnum storageType = StorageTypeEnum.All)
        {
            var builder = GetBuilder(wizard);
            builder.Store(storageType);

            return wizard;
        }

        /// <summary>
        /// Configure grain system storage in memory
        /// </summary>
        /// <param name="buildReadOnlyRepository">If true create a repository to look up data throught cluster memory.
        ///  Attention : Memory storage on the cluster is efficient to access using _stateName and StorageName keys otherwise we have to get all the information
        ///     to filtered it is not efficient to use with knowledge of performance issue.</param>
        public static IDemocriteNodeMemoryBuilder UseInMemoryGrainStorage(this IDemocriteNodeMemoryBuilder wizard, string? configurationKey = null, bool buildReadOnlyRepository = false)
        {
            var builder = GetBuilder(wizard);

            configurationKey = string.IsNullOrEmpty(configurationKey) ? DemocriteConstants.DefaultDemocriteStateConfigurationKey : configurationKey;
            builder.SetupGrainStateStorage(configurationKey, buildReadOnlyRepository);

            return wizard;
        }

        /// <summary>
        /// Configure repository system storage in memory
        /// </summary>
        /// <param name="buildRepository">If true create a repository to look up data throught cluster memory.
        ///  Attention : Memory storage on the cluster is efficient to access using _stateName and StorageName keys otherwise we have to get all the information
        ///     to filtered it is not efficient to use with knowledge of performance issue.</param>
        public static IDemocriteNodeMemoryBuilder UseInMemoryRepository(this IDemocriteNodeMemoryBuilder wizard, string? configurationKey = null)//, bool allowWrite = true)
        {
            var builder = GetBuilder(wizard);

            configurationKey = string.IsNullOrEmpty(configurationKey) ? DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey : configurationKey;
            builder.SetupRepositoryStorage(configurationKey);

            return wizard;
        }
    }
}
