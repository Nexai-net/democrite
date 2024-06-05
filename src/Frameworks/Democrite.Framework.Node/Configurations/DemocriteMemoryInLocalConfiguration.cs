// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.Configurations.AutoConfigurator;
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
        /// Configure system storage in memory
        /// </summary>
        public static IDemocriteNodeMemoryBuilder UseInMemory(this IDemocriteNodeMemoryBuilder wizard, StorageTypeEnum storageType = StorageTypeEnum.All)
        {
            if ((storageType & StorageTypeEnum.Default) == StorageTypeEnum.Default)
            {
                AutoDefaultMemoryConfigurator.Default.AutoConfigure(wizard,
                                                                    wizard.GetConfiguration(),
                                                                    wizard.GetServiceCollection(),
                                                                    wizard.Logger);
            }

            if ((storageType & StorageTypeEnum.Reminders) == StorageTypeEnum.Reminders)
            {
                AutoDefaultReminderStateMemoryAutoConfigurator.Default.AutoConfigure(wizard,
                                                                                     wizard.GetConfiguration(),
                                                                                     wizard.GetServiceCollection(),
                                                                                     wizard.Logger);
            }

            if ((storageType & StorageTypeEnum.Democrite) == StorageTypeEnum.Democrite)
            {
                AutoDefaultDemocriteMemoryConfigurator.Default.AutoConfigure(wizard,
                                                                             wizard.GetConfiguration(),
                                                                             wizard.GetServiceCollection(),
                                                                             wizard.Logger);
            }

            if ((storageType & StorageTypeEnum.DynamicDefinition) == StorageTypeEnum.DynamicDefinition)
            {
                AutoDefaultDemocriteDynamicDefinitionsMemoryConfigurator.Default.AutoConfigure(wizard,
                                                                                               wizard.GetConfiguration(),
                                                                                               wizard.GetServiceCollection(),
                                                                                               wizard.Logger);
            }

            if ((storageType & StorageTypeEnum.DemocriteAdmin) == StorageTypeEnum.DemocriteAdmin)
            {
                AutoDefaultDemocriteAdminMemoryConfigurator.Default.AutoConfigure(wizard,
                                                                                   wizard.GetConfiguration(),
                                                                                   wizard.GetServiceCollection(),
                                                                                   wizard.Logger);
            }

            if ((storageType & StorageTypeEnum.Repositories) == StorageTypeEnum.Repositories)
            {
                AutoDefaultCustomRepositoryMemoryConfigurator.Default.AutoConfigure(wizard,
                                                                                    wizard.GetConfiguration(),
                                                                                    wizard.GetServiceCollection(),
                                                                                    wizard.Logger,
                                                                                    new DefaultRepositoryStorageOption(DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey, AllowWrite: true, TargetGrainState: false));
            }

            return wizard;
        }

        /// <summary>
        /// Configure grain system storage in memory
        /// </summary>
        /// <param name="buildReadOnlyRepository">If true create a repository to look up data throught cluster memory.
        ///  Attention : Memory storage on the cluster is efficient to access using _stateName and StorageName keys otherwise we have to get all the information
        ///     to filtered it is not efficient to use with knowledge of performance issue.</param>
        public static IDemocriteNodeMemoryBuilder UseInMemoryGrainStorage(this IDemocriteNodeMemoryBuilder wizard, string? storageName = null, bool buildReadOnlyRepository = false)
        {
            storageName = string.IsNullOrEmpty(storageName) ? DemocriteConstants.DefaultDemocriteStateConfigurationKey : storageName;

            AutoDefaultCustomGrainMemoryConfigurator.Default.AutoConfigureCustomStorage(wizard,
                                                                                        wizard.GetConfiguration(),
                                                                                        wizard.GetServiceCollection(),
                                                                                        wizard.Logger,
                                                                                        storageName,
                                                                                        buildReadOnlyRepository);
            return wizard;
        }

        /// <summary>
        /// Configure repository system storage in memory
        /// </summary>
        /// <param name="buildRepository">If true create a repository to look up data throught cluster memory.
        ///  Attention : Memory storage on the cluster is efficient to access using _stateName and StorageName keys otherwise we have to get all the information
        ///     to filtered it is not efficient to use with knowledge of performance issue.</param>
        public static IDemocriteNodeMemoryBuilder UseInMemoryRepository(this IDemocriteNodeMemoryBuilder wizard, string? storageName = null, bool allowWrite = true)
        {
            storageName = string.IsNullOrEmpty(storageName) ? DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey : storageName;

            AutoDefaultCustomRepositoryMemoryConfigurator.Default.AutoConfigure(wizard,
                                                                                wizard.GetConfiguration(),
                                                                                wizard.GetServiceCollection(),
                                                                                wizard.Logger,
                                                                                new DefaultRepositoryStorageOption(storageName, AllowWrite: allowWrite, TargetGrainState: false));
            return wizard;
        }
    }
}
