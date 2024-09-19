// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// KEEP : Democrite.Framework
namespace Democrite.Framework
{
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    using Orleans.Runtime;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Democrite cluster's node root
    /// </summary>
    public sealed class DemocriteNode : DemocriteBasePart<DemocriteNodeConfigurationDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteNode"/> class.
        /// </summary>
        internal DemocriteNode(IHost host,
                               DemocriteNodeConfigurationDefinition? config,
                               IDemocriteExecutionHandler? democriteExecutionHandler,
                               bool hostOwned)
            : base(host, config, democriteExecutionHandler, hostOwned)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Task OnStartAsync(CancellationToken token = default)
        {
            var option = this.Services.GetService<IOptions<ClusterNodeOptions>>();

            if (option?.Value.AddSiloInformationToConsole ?? false)
            {
                var silo = this.Services.GetRequiredService<ILocalSiloDetails>();
                Console.Title = silo.Name + " Cl. Id: " + silo.ClusterId;
            }

            return base.OnStartAsync(token);
        }

        /// <inheritdoc />
        /// <remarks>
        /// ATTENTION COULD NOT BE Restart, dispose the node and create new
        /// </remarks>
        protected override Task OnStopAsync(CancellationToken token)
        {
            return base.OnStopAsync(token);
        }

        /// <summary>
        /// Get and setup a new democrite cluster node
        /// </summary>
        public static DemocriteNode Create(string[] args,
                                           Action<IDemocriteNodeBuilder>? builder,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(args, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new democrite cluster node
        /// </summary>
        public static DemocriteNode Create(Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                           Action<IDemocriteNodeBuilder>? builder,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create((string[]?)null, setupConfig, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new democrite cluster node
        /// </summary>
        public static DemocriteNode Create(Action<IDemocriteNodeBuilder>? builder = null,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create((string[]?)null, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new democrite cluster node
        /// </summary>
        /// <remarks>
        ///     Code on section node part ClusterClient.Get is very similar.
        ///     Couldn't factorize for know due method like UseOrleans that are extension method.
        /// </remarks>
        public static DemocriteNode Create(string[]? args,
                                           Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                           Action<IDemocriteNodeBuilder>? builder,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            var host = CreateAndConfigureHost(args, setupConfig);

            Create(host, setupConfig, builder, clusterBuilderTools);

            var buildedHost = host.Build();

            return new DemocriteNode(buildedHost,
                                     buildedHost.Services.GetRequiredService<DemocriteNodeConfigurationDefinition>(),
                                     buildedHost.Services.GetRequiredService<IDemocriteExecutionHandler>(),
                                     true);
        }

        /// <summary>
        /// Get and setup a new democrite cluster node
        /// </summary>
        /// <remarks>
        ///     Code on section node part ClusterClient.Get is very similar.
        ///     Couldn't factorize for know due method like UseOrleans that are extension method.
        /// </remarks>
        internal static void Create(IHostBuilder host,
                                    Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                    Action<IDemocriteNodeBuilder>? builder,
                                    ClusterBuilderTools? clusterBuilderTools = null)
        {
            if (builder is null)
            {
                builder = cfg => cfg.WizardConfig()
                                    .SetupClusterFromConfig();
            }

            clusterBuilderTools ??= ClusterBuilderTools.Default;

            host.UseOrleans((ctx, orleanBuilder) =>
            {
                var cfgHost = new DemocriteNodeBuilder(host,
                                                       orleanBuilder,
                                                       ctx,
                                                       clusterBuilderTools);
                builder?.Invoke(cfgHost);

                var democriteNodeConfigurationDefinition = cfgHost.Build(ctx);

                orleanBuilder.Services.AddSingleton(democriteNodeConfigurationDefinition);
            });
        }

        #endregion
    }
}