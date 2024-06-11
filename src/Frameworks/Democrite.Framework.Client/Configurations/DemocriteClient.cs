// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework
{
    using Democrite.Framework.Client.Configurations;
    using Democrite.Framework.Client.Model;
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Democrite cluster's client root
    /// </summary>
    public sealed class DemocriteClient : DemocriteBasePart<DemocriteClientConfigurationDefinition>
    {
        private bool _clientDeco;
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteClient"/> class.
        /// </summary>
        internal DemocriteClient(IHost host,
                                 DemocriteClientConfigurationDefinition? config,
                                 IDemocriteExecutionHandler democriteExecutionHandler,
                                 bool hostOwned)
            : base(host, config, democriteExecutionHandler, hostOwned)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <remarks>
        ///     Override to managed connect/deconnect
        /// </remarks>
        protected override async Task TryStartHostAsync(CancellationToken token)
        {
            if (this._clientDeco)
            {
                var connectionService = this.Services.GetServices<IClusterClient>() as IHostedService;

                await (connectionService?.StartAsync(token) ?? Task.CompletedTask);
                return;
            }

            await base.TryStartHostAsync(token);
        }

        /// <summary>
        /// Prevent HOST to be cleanup to allow multiple restart
        /// </summary>
        protected override async Task TryStopHostAsync(CancellationToken token)
        {
            var connectionService = this.Services.GetRequiredService<IClusterClient>() as IHostedService;

            await (connectionService?.StopAsync(token) ?? Task.CompletedTask);
            this._clientDeco = true;
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            await base.DisposeBeginAsync();
            // Force host to stop on dispose
            await base.TryStopHostAsync(default);
        }

        /// <summary>
        /// Get and setup a new democrite cluster client
        /// </summary>
        public static DemocriteClient Create(string[] args,
                                             Action<IDemocriteClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(args, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new democrite cluster client
        /// </summary>
        public static DemocriteClient Create(Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                             Action<IDemocriteClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(null, setupConfig, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new democrite cluster client
        /// </summary>
        public static DemocriteClient Create(Action<IDemocriteClientBuilder>? builder = null,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(null, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new democrite cluster client
        /// </summary>
        public static DemocriteClient Create(string[]? args,
                                             Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                             Action<IDemocriteClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools)
        {
            var host = CreateAndConfigureHost(args, setupConfig);

            Create(host, setupConfig, builder, clusterBuilderTools);

            var buildedHost = host.Build();

            return new DemocriteClient(buildedHost,
                                       buildedHost.Services.GetRequiredService<DemocriteClientConfigurationDefinition>(),
                                       buildedHost.Services.GetRequiredService<IDemocriteExecutionHandler>(),
                                       true);
        }

        /// <summary>
        /// Get and setup a new democrite cluster client
        /// </summary>
        internal static void Create<THostBuilder>(THostBuilder hostBuilder,
                                                  Action<HostBuilderContext, IConfigurationBuilder>? setupConfig = null,
                                                  Action<IDemocriteClientBuilder>? builder = null,
                                                  ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            if (builder is null)
            {
                builder = cfg => cfg.WizardConfig()
                                    .SetupClusterFromConfig();
            }

            clusterBuilderTools ??= ClusterBuilderTools.Default;

            hostBuilder.UseOrleansClient((ctx, orleanClientBuilder) =>
            {
                var cfgHost = new DemocriteClientBuilder(hostBuilder,
                                                         orleanClientBuilder,
                                                         ctx,
                                                         clusterBuilderTools);

                builder?.Invoke(cfgHost);

                var democriteClientConfigurationDefinition = cfgHost.Build(ctx);

                orleanClientBuilder.Services.AddSingleton(democriteClientConfigurationDefinition);
            });
        }

        #endregion
    }
}
