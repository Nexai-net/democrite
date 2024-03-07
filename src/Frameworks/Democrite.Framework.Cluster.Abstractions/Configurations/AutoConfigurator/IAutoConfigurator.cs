// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Root definition of a auto configurator
    /// </summary>
    /// <remarks>
    ///     You must inherit only from verion <see cref="IAutoConfigurator{TWizard}"/>
    /// </remarks>
    public interface IAutoConfigurator
    {

    }

    /// <summary>
    /// Root definition of a auto configurator
    /// </summary>
    public interface IAutoConfigurator<TWizard> : IAutoConfigurator
             where TWizard : IBuilderDemocriteBaseWizard
    {
        /// <summary>
        /// Automatics configure democrite section.
        /// </summary>
        void AutoConfigure(TWizard democriteBuilderWizard,
                           IConfiguration configuration,
                           IServiceCollection serviceCollection,
                           ILogger logger);
    }

    /// <summary>
    /// Root definition of a auto configurator
    /// </summary>
    public interface IAutoKeyConfigurator<TWizard> : IAutoConfigurator
             where TWizard : IBuilderDemocriteBaseWizard
    {
        /// <summary>
        /// Automatics configure democrite section.
        /// </summary>
        void AutoConfigure(TWizard democriteBuilderWizard,
                           IConfiguration configuration,
                           IServiceCollection serviceCollection,
                           ILogger logger,
                           string sourceConfigurationKey,
                           string targetKey);
    }
}
