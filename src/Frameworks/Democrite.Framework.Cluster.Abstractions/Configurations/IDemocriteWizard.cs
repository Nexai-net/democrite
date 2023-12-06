// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations
{
    using Microsoft.Extensions.DependencyInjection;

    using System;

    /// <summary>
    ///  Cluster configuration wizard tools
    /// </summary>
    public interface IDemocriteWizard<TWizard, TWizardConfig> : IDemocriteCoreConfigurationWizard<TWizardConfig>
        where TWizard : IDemocriteWizard<TWizard, TWizardConfig>
        where TWizardConfig : IDemocriteCoreConfigurationWizard<TWizardConfig>
    {
        /// <summary>
        /// Setups dependency injection relations
        /// </summary>
        TWizard Configure(Action<TWizardConfig> configureDelegate);

        /// <summary>
        /// Manual configuration of services.
        /// </summary>
        TWizard ConfigureServices(Action<IServiceCollection> config);
    }
}
