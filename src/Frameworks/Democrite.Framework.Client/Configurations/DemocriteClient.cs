// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client.Configurations
{
    using Democrite.Framework.Client.Abstractions.Configurations;
    using Democrite.Framework.Client.Model;
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using System;

    /// <summary>
    /// Democrite cluster's client root
    /// </summary>
    public sealed class DemocriteClient : DemocriteBasePart<DemocriteClientConfigurationDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteClient"/> class.
        /// </summary>
        private DemocriteClient(IHost host,
                                DemocriteClientConfigurationDefinition? config,
                                IDemocriteExecutionHandler democriteExecutionHandler)
            : base(host, config, democriteExecutionHandler)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create and setup a new democrite cluster client
        /// </summary>
        public static DemocriteClient Create(string[] args,
                                             Action<IClusterClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(args, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Create and setup a new democrite cluster client
        /// </summary>
        public static DemocriteClient Create(Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                             Action<IClusterClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(null, setupConfig, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Create and setup a new democrite cluster client
        /// </summary>
        public static DemocriteClient Create(Action<IClusterClientBuilder>? builder = null,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(null, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Create and setup a new democrite cluster client
        /// </summary>
        public static DemocriteClient Create(string[]? args,
                                             Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                             Action<IClusterClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools)
        {
            var host = CreateAndConfigureHost(args, setupConfig);

            Create(host, setupConfig, builder, clusterBuilderTools);

            var buildedHost = host.Build();

            return new DemocriteClient(buildedHost,
                                       buildedHost.Services.GetRequiredService<DemocriteClientConfigurationDefinition>(),
                                       buildedHost.Services.GetRequiredService<IDemocriteExecutionHandler>());
        }

        /// <summary>
        /// Create and setup a new democrite cluster client
        /// </summary>
        internal static void Create<THostBuilder>(THostBuilder hostBuilder,
                                                  Action<HostBuilderContext, IConfigurationBuilder>? setupConfig = null,
                                                  Action<IClusterClientBuilder>? builder = null,
                                                  ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            if (builder is null)
            {
                builder = cfg => cfg.WizardConfig()
                                    .ClusterFromConfig();
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
