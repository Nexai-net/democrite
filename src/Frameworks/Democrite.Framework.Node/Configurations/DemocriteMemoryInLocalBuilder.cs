// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Core.Repositories;
    using Democrite.Framework.Node.Abstractions.Repositories;
    using Democrite.Framework.Node.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Storages;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using Orleans.Runtime.Hosting;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Memory storage builder
    /// </summary>
    internal sealed class DemocriteMemoryInLocalBuilder : IDemocriteMemoryInLocalBuilder
    {
        #region Fields

        private readonly IDemocriteNodeMemoryBuilder _memoryBuilder;
        private readonly ISiloBuilder _siloBuilder;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteMemoryInLocalBuilder"/> class.
        /// </summary>
        public DemocriteMemoryInLocalBuilder(IDemocriteNodeMemoryBuilder memoryBuilder)
        {
            this._memoryBuilder = memoryBuilder;
            this._siloBuilder = memoryBuilder.GetSiloBuilder();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupDefaultRepositoryStorage()
        {
            SetupRepositoryStorageImpl(DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupDefaultStorage()
        {
            this._siloBuilder.AddMemoryGrainStorageAsDefault(MemoryConfiguratorHelper.OptionConfigurator);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupDemocriteAdminStorage()
        {
            SetupGrainStateStorageImpl(DemocriteConstants.DefaultDemocriteAdminStateConfigurationKey, false);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupDemocriteStorage()
        {
            SetupGrainStateStorageImpl(DemocriteConstants.DefaultDemocriteStateConfigurationKey, false);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupDynamicDefinitionStorage()
        {
            SetupGrainStateStorageImpl(DemocriteConstants.DefaultDemocriteDynamicDefinitionsConfigurationKey, false);
            SetupRepositoryStorageImpl(DemocriteConstants.DefaultDemocriteDynamicDefinitionsRepositoryConfigurationKey);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupGrainStateStorage(string key, bool buildRepository = false)
        {
            SetupGrainStateStorageImpl(key, buildRepository);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupGrainStateStorage(IReadOnlyCollection<string> keys, bool buildRepository = false)
        {
            foreach (var key in keys)
                SetupGrainStateStorageImpl(key, buildRepository);

            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupReminderStorage()
        {
            this._siloBuilder.UseInMemoryReminderService();
            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupRepositoryStorage(string key)
        {
            SetupRepositoryStorageImpl(key);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder SetupRepositoryStorage(IReadOnlyCollection<string> keys)
        {
            foreach (var key in keys)
                SetupRepositoryStorageImpl(key);

            return this;
        }

        /// <inheritdoc />
        public IDemocriteMemoryInLocalBuilder Store(StorageTypeEnum storageType)
        {
            if ((storageType & StorageTypeEnum.Default) == StorageTypeEnum.Default)
            {
                SetupDefaultStorage();
                storageType -= StorageTypeEnum.Default;
            }

            if ((storageType & StorageTypeEnum.Reminders) == StorageTypeEnum.Reminders)
            {
                SetupReminderStorage();
                storageType -= StorageTypeEnum.Reminders;
            }

            if ((storageType & StorageTypeEnum.Democrite) == StorageTypeEnum.Democrite)
            {
                SetupDemocriteStorage();
                storageType -= StorageTypeEnum.Democrite;
            }

            if ((storageType & StorageTypeEnum.DynamicDefinition) == StorageTypeEnum.DynamicDefinition)
            {
                SetupDynamicDefinitionStorage();
                storageType -= StorageTypeEnum.DynamicDefinition;
            }

            if ((storageType & StorageTypeEnum.DemocriteAdmin) == StorageTypeEnum.DemocriteAdmin)
            {
                SetupDemocriteAdminStorage();
                storageType -= StorageTypeEnum.DemocriteAdmin;
            }

            if ((storageType & StorageTypeEnum.Repositories) == StorageTypeEnum.Repositories)
            {
                SetupDefaultRepositoryStorage();
                storageType -= StorageTypeEnum.Repositories;
            }

            if (storageType != 0)
                throw new InvalidOperationException("Storage configuration not managed : " + storageType);

            return this;
        }

        #region Tools

        /// <summary>
        /// Setups the grain state storage implementation.
        /// </summary>
        private void SetupGrainStateStorageImpl(string key, bool buildReadRepository)
        {
            this._siloBuilder.AddMemoryGrainStorage(key, MemoryConfiguratorHelper.OptionConfigurator);

            if (buildReadRepository)
            {
                this._siloBuilder.Services.AddGrainStorage(key, MemoryGrainStorageRepositoryFactory.Create);
                this._siloBuilder.Services.TryAddSingleton<CacheProxyRepositorySpecificFactory<MemorySpecificStateContainerFactory>>();
                this._siloBuilder.Services.AddKeyedSingleton<IRepositorySpecificFactory>(key, (p, k) => p.GetRequiredService<CacheProxyRepositorySpecificFactory<MemorySpecificStateContainerFactory>>());
            }
        }

        /// <summary>
        /// Setups the repository storage implementation.
        /// </summary>
        private void SetupRepositoryStorageImpl(string key)
        {
            this._siloBuilder.Services.RemoveKeyedService<string, IRepositorySpecificFactory>(key);
            this._siloBuilder.Services.TryAddSingleton<CacheProxyRepositorySpecificFactory<MemorySpecificRepositoryFactory>>();
            this._siloBuilder.Services.AddKeyedSingleton<IRepositorySpecificFactory>(key, (p, k) => p.GetRequiredService<CacheProxyRepositorySpecificFactory<MemorySpecificRepositoryFactory>>());
        }

        #endregion

        #endregion
    }
}
