// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.Storages;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Runtime.Hosting;
    using Orleans.Storage;

    using System;

    /// <summary>
    /// Auto configure - In AppDomain Memory - custom state memory
    /// </summary>
    /// <seealso cref="INodeCustomGrainMemoryAutoConfigurator" />
    public sealed class AutoDefaultCustomGrainMemoryConfigurator : INodeCustomGrainMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoDefaultCustomGrainMemoryConfigurator"/> class.
        /// </summary>
        static AutoDefaultCustomGrainMemoryConfigurator()
        {
            Default = new AutoDefaultCustomGrainMemoryConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoDefaultCustomGrainMemoryConfigurator Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  string sourceConfigurationKey,
                                  string targetKey)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(targetKey);

            var customGrainStorageOption = configuration.GetSection(sourceConfigurationKey).Get<DefaultGrainStorageOption>();

            AutoConfigureCustomStorage(democriteBuilderWizard,
                                       configuration,
                                       serviceCollection,
                                       logger,
                                       targetKey,
                                       customGrainStorageOption?.BuildReadRepository ?? false);
        }

        /// <inheritdoc />
        public void AutoConfigureCustomStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                               IConfiguration configuration,
                                               IServiceCollection serviceCollection,
                                               ILogger logger,
                                               string key,
                                               bool buildReadRepository)
        {
            var siloBuilder = democriteBuilderWizard.GetSiloBuilder();
            siloBuilder.AddMemoryGrainStorage(key, MemoryConfiguratorHelper.OptionConfigurator);

            if (buildReadRepository)
            {
                // Must found the service associate to name key
                // siloBuilder.Services.RemoveKeyedService<string, IGrainStorage>(key);

                // Setup a new MemoryGrain provider with track to registry
                // siloBuilder.Services.AddKeyedSingleton<IGrainStorage>(key, (p, o) => MemoryGrainStorageRepositoryFactory.Create(p, (string)o!));

                siloBuilder.Services.AddGrainStorage(key, MemoryGrainStorageRepositoryFactory.Create);

                AutoDefaultCustomRepositoryMemoryConfigurator.Default.AutoConfigure(democriteBuilderWizard,
                                                                                    configuration,
                                                                                    serviceCollection,
                                                                                    logger,
                                                                                    new DefaultRepositoryStorageOption(key, AllowWrite: false, TargetGrainState: true));
            }
        }

        #endregion
    }
}
