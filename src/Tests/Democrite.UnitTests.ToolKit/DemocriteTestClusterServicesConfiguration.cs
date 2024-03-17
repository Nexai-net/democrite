// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Extensions;
    using Democrite.Framework.Core.Repositories;
    using Democrite.Framework.Node.Configurations;
    using Democrite.Framework.Node.Extensions;
    using Democrite.Framework.Node.Services;

    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Orleans.Hosting;
    using Orleans.TestingHost;

    /// <summary>
    /// Service configuration use to 
    /// </summary>
    public sealed class DemocriteTestClusterServicesConfiguration : ISiloConfigurator,
                                                                    IHostConfigurator,
                                                                    IClientBuilderConfigurator
    {
        #region Methods

        /// <inheritdoc />
        public void Configure(ISiloBuilder siloBuilder)
        {
            ConfigTestDemocriteServices(siloBuilder.Services);

            siloBuilder.AddMemoryGrainStorage(nameof(Democrite))
                       .AddMemoryGrainStorageAsDefault()
                       .AddIncomingGrainCallFilter<IncomingGrainCallTracer>();
        }

        /// <inheritdoc />
        public void Configure(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(ConfigTestDemocriteServices);
        }

        /// <inheritdoc />
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            clientBuilder.ConfigureServices(ConfigTestDemocriteServices);
        }

        /// <inheritdoc />
        private void ConfigTestDemocriteServices(IServiceCollection services)
        {
            DemocriteCoreServicesExtensions.SetupCoreServices(services);

            services.AddSingleton<IVGrainProvider, VGrainProvider>()
                    .AddSingleton<IDiagnosticLogger, Democrite.Framework.Core.Diagnostics.DiagnosticLogger>()
                    .AddSingleton<ITimeManager, TimeManager>()
                    .AddSingleton<IDemocriteSerializer, DemocriteSerializer>()
                    .AddSingleton<IObjectConverter, ObjectConverter>();

            services.SetupGrainRoutingServices();

            // Call
            services.SetupSequenceExecutorThreadStageProvider();

        }

        #endregion
    }
}
