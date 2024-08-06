// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations.AutoConfigurator
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto configure - In AppDomain Memory - the vgrain state
    /// </summary>
    /// <seealso cref="INodeDemocriteMemoryAutoConfigurator" />
    public sealed class AutoDefaultDemocriteMemoryConfigurator : INodeDemocriteMemoryAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IDemocriteNodeMemoryBuilder democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            var builder = DemocriteMemoryInLocalConfiguration.GetBuilder(democriteBuilderWizard);
            builder.SetupDemocriteStorage();
        }
    }
}
