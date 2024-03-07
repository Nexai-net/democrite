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
    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    /// <summary>
    /// Auto configure - In AppDomain Memory - custom repository memory
    /// </summary>
    /// <seealso cref="INodeCustomRepositoryMemoryAutoConfigurator" />
    public sealed class AutoDefaultCustomRepositoryMemoryConfigurator : INodeCustomRepositoryMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoDefaultCustomRepositoryMemoryConfigurator"/> class.
        /// </summary>
        static AutoDefaultCustomRepositoryMemoryConfigurator()
        {
            Default = new AutoDefaultCustomRepositoryMemoryConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoDefaultCustomRepositoryMemoryConfigurator Default { get; }

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

            var opt = configuration.GetSection(sourceConfigurationKey).Get<DefaultRepositoryStorageOption>();
            AutoConfigure(democriteBuilderWizard,
                          configuration,
                          serviceCollection,
                          logger,
                          new DefaultRepositoryStorageOption(targetKey, opt?.AllowWrite ?? true, opt?.TargetGrainState ?? false));
        }

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  DefaultRepositoryStorageOption option)
        {
            var siloBuilder = democriteBuilderWizard.GetSiloBuilder();
            siloBuilder.Services.RemoveKeyedService<string, IRepositorySpecificFactory>(option.Key);

            if (option.TargetGrainState)
                siloBuilder.Services.AddSingletonNamedService<IRepositorySpecificFactory>(option.Key, (p, k) => ActivatorUtilities.CreateInstance<MemoryGrainStateSpecificRepositoryFactory>(p, k));
            else
                siloBuilder.Services.AddSingletonNamedService<IRepositorySpecificFactory>(option.Key, (p, k) => ActivatorUtilities.CreateInstance<MemorySpecificRepositoryFactory>(p, k));
        }

        #endregion
    }
}
