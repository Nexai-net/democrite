// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Services;
    using Democrite.Framework.Node.Services;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Orleans.Hosting;
    using Orleans.TestingHost;
    using Orleans.Timers.Internal;

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
            services.AddSingleton<IVGrainProvider, VGrainProvider>()
                    .AddSingleton<IDiagnosticLogger, Democrite.Framework.Core.Diagnostics.DiagnosticLogger>()
                    .AddSingleton<IVGrainIdFactory, VGrainIdFactory>()
                    .AddSingleton<ITimeManager, TimeManager>();
        }

        #endregion
    }
}
