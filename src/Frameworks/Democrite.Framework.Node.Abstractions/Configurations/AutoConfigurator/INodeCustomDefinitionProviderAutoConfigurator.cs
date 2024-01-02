﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto setup default definition provider
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IClusterNodeBuilderDemocriteMemoryBuilder}" />
    public interface INodeCustomDefinitionProviderAutoConfigurator : IAutoConfigurator<IDemocriteNodeMemoryBuilder>
    {
        /// <summary>
        /// Automatics configure democrite section.
        /// </summary>
        void AutoConfigureCustomProvider(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                         IConfiguration configuration,
                                         IServiceCollection serviceCollection,
                                         ILogger logger,
                                         string key);
    }
}
