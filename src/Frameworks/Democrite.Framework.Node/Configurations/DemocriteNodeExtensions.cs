// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Maintains Microsoft.Extensions.Hosting to easy fluent setups
// KEEP : Microsoft.Extensions.Hosting
namespace Microsoft.Extensions.Hosting
{
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework;
    using Democrite.Framework.Node.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using System;

    /// <summary>
    /// Expose method to activate democrite node on existing host
    /// </summary>
    public static class DemocriteNodeExtensions
    {
        /// <summary>
        /// Configure Democrite client part in the <paramref name="hostBuilder"/> environment
        /// </summary>
        public static THostBuilder UseDemocriteNode<THostBuilder>(this THostBuilder hostBuilder,
                                                                  Action<IDemocriteNodeBuilder>? builder = null,
                                                                  ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            return UseDemocriteNode<THostBuilder>(hostBuilder, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Configure Democrite client part in the <paramref name="hostBuilder"/> environment
        /// </summary>
        public static THostBuilder UseDemocriteNode<THostBuilder>(this THostBuilder hostBuilder,
                                                                  Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                                                  Action<IDemocriteNodeBuilder>? builder,
                                                                  ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            DemocriteNode.Create(hostBuilder, setupConfig, builder, clusterBuilderTools);

            hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService((s) =>
                {
                    return new DemocriteNode(s.GetRequiredService<IHost>(),
                                             s.GetRequiredService<DemocriteNodeConfigurationDefinition>(),
                                             s.GetRequiredService<IDemocriteExecutionHandler>(),
                                             false);
                });
            });

            return hostBuilder;
        }
    }
}
