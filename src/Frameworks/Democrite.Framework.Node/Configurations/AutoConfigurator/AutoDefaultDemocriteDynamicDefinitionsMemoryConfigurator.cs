// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto configure - In AppDomain Memory - dynamic definitions
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public sealed class AutoDefaultDemocriteDynamicDefinitionsMemoryConfigurator : INodeDemocriteDynamicDefinitionsMemoryAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoDefaultDemocriteDynamicDefinitionsMemoryConfigurator"/> class.
        /// </summary>
        static AutoDefaultDemocriteDynamicDefinitionsMemoryConfigurator()
        {
            Default = new AutoDefaultDemocriteDynamicDefinitionsMemoryConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoDefaultDemocriteDynamicDefinitionsMemoryConfigurator Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            AutoDefaultCustomGrainMemoryConfigurator.Default.AutoConfigureCustomStorage(democriteBuilderWizard,
                                                                                        configuration,
                                                                                        serviceCollection,
                                                                                        logger,
                                                                                        DemocriteConstants.DefaultDemocriteDynamicDefinitionsConfigurationKey,
                                                                                        false);

            AutoDefaultCustomRepositoryMemoryConfigurator.Default.AutoConfigure(democriteBuilderWizard,
                                                                                configuration,
                                                                                serviceCollection,
                                                                                logger,
                                                                                new DefaultRepositoryStorageOption(DemocriteConstants.DefaultDemocriteDynamicDefinitionsRepositoryConfigurationKey,
                                                                                                                   AllowWrite: true,
                                                                                                                   TargetGrainState: false));
        }

        #endregion
    }
}
