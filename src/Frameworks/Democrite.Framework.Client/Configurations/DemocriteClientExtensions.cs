// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Maintains Microsoft.Extensions.Hosting to easy fluent setups
namespace Microsoft.Extensions.Hosting
{
    using Democrite.Framework;
    using Democrite.Framework.Client.Model;
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using System;

    /// <summary>
    /// Expose method to activate democrite client on existing host
    /// </summary>
    public static class DemocriteClientExtensions
    {
        /// <summary>
        /// Configure Democrite client part in the <paramref name="hostBuilder"/> environment
        /// </summary>
        public static THostBuilder UseDemocriteClient<THostBuilder>(this THostBuilder hostBuilder,
                                                                    Action<IDemocriteClientBuilder>? builder = null,
                                                                    ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            return UseDemocriteClient<THostBuilder>(hostBuilder, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Configure Democrite client part in the <paramref name="hostBuilder"/> environment
        /// </summary>
        public static THostBuilder UseDemocriteClient<THostBuilder>(this THostBuilder hostBuilder,
                                                                    Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                                                    Action<IDemocriteClientBuilder>? builder,
                                                                    ClusterBuilderTools? clusterBuilderTools)
            where THostBuilder : IHostBuilder
        {
            DemocriteClient.Create(hostBuilder, setupConfig, builder, clusterBuilderTools);

            hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService((s) =>
                {
                    return new DemocriteClient(s.GetRequiredService<IHost>(),
                                               s.GetRequiredService<DemocriteClientConfigurationDefinition>(),
                                               s.GetRequiredService<IDemocriteExecutionHandler>(),
                                               false);
                });
            });

            return hostBuilder;
        }
    }
}
