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

    using System;

    /// <summary>
    /// Auto configure - In AppDomain Memory - custom state memory
    /// </summary>
    /// <seealso cref="INodeCustomGrainMemoryAutoConfigurator" />
    public sealed class AutoDefaultCustomGrainMemoryConfigurator : INodeCustomGrainMemoryAutoConfigurator
    {
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
            var builder = DemocriteMemoryInLocalConfiguration.GetBuilder(democriteBuilderWizard);
            builder.SetupGrainStateStorage(key, buildReadRepository);
        }

        #endregion
    }
}
