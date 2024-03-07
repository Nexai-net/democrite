// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;

    /// <summary>
    /// Auto setup default definition provider
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IClusterNodeBuilderDemocriteMemoryBuilder}" />
    public interface INodeCustomDefinitionProviderAutoConfigurator : IAutoKeyConfigurator<IDemocriteNodeMemoryBuilder>
    {
        ///// <summary>
        ///// Automatics configure democrite section.
        ///// </summary>
        //void AutoConfigureCustomProvider(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
        //                                 IConfiguration configuration,
        //                                 IServiceCollection serviceCollection,
        //                                 ILogger logger,
        //                                 string key);
    }
}
