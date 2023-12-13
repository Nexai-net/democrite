// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Democrite cluster's node root
    /// </summary>
    public sealed class DemocriteNode : DemocriteBasePart<DemocriteNodeConfigurationDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteNode"/> class.
        /// </summary>
        private DemocriteNode(IHost host,
                              DemocriteNodeConfigurationDefinition? config,
                              IDemocriteExecutionHandler? democriteExecutionHandler)
            : base(host, config, democriteExecutionHandler)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create and setup a new democrite cluster node
        /// </summary>
        public static DemocriteNode Create(string[] args,
                                           Action<IDemocriteNodeBuilder>? builder,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(args, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Create and setup a new democrite cluster node
        /// </summary>
        public static DemocriteNode Create(Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                           Action<IDemocriteNodeBuilder>? builder,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create((string[]?)null, setupConfig, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Create and setup a new democrite cluster node
        /// </summary>
        public static DemocriteNode Create(Action<IDemocriteNodeBuilder>? builder = null,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create((string[]?)null, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Create and setup a new democrite cluster node
        /// </summary>
        /// <remarks>
        ///     Code on section node part ClusterClient.Create is very similar.
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
                                     buildedHost.Services.GetRequiredService<IDemocriteExecutionHandler>());
        }

        /// <summary>
        /// Create and setup a new democrite cluster node
        /// </summary>
        /// <remarks>
        ///     Code on section node part ClusterClient.Create is very similar.
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
                                    .ClusterFromConfig();
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