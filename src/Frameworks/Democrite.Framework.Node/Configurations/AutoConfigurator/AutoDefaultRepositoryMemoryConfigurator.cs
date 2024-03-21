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
    /// Auto configure - In AppDomain Memory - default repository memory
    /// </summary>
    /// <seealso cref="INodeCustomRepositoryMemoryAutoConfigurator" />
    public sealed class AutoDefaultRepositoryMemoryConfigurator : INodeDefaultRepositoryMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoDefaultCustomRepositoryMemoryConfigurator"/> class.
        /// </summary>
        static AutoDefaultRepositoryMemoryConfigurator()
        {
            Default = new AutoDefaultRepositoryMemoryConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoDefaultRepositoryMemoryConfigurator Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            AutoDefaultCustomRepositoryMemoryConfigurator.Default.AutoConfigure(democriteBuilderWizard,
                                                                                configuration,
                                                                                serviceCollection,
                                                                                logger,
                                                                                new DefaultRepositoryStorageOption(DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey, true, false));
        }

        #endregion
    }
}
