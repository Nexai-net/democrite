// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto setup default state memory container
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IClusterNodeBuilderDemocriteMemoryBuilder}" />
    public interface INodeCustomGrainMemoryAutoConfigurator : IAutoKeyConfigurator<IDemocriteNodeMemoryBuilder>
    {
        /// <summary>
        /// Automatics configure democrite section.
        /// </summary>
        void AutoConfigureCustomStorage(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                        IConfiguration configuration,
                                        IServiceCollection serviceCollection,
                                        ILogger logger,
                                        string key,
                                        bool buildReadRepository = false);
    }
}
