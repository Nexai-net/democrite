// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client
{
    using Democrite.Framework.Client.Abstractions.Configurations;
    using Democrite.Framework.Client.Configurations;
    using Democrite.Framework.Cluster.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

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
                                                                    Action<IClusterClientBuilder>? builder = null,
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
                                                                    Action<IClusterClientBuilder>? builder,
                                                                    ClusterBuilderTools? clusterBuilderTools)
            where THostBuilder : IHostBuilder
        {
            DemocriteClient.Create(hostBuilder, setupConfig, builder, clusterBuilderTools);

            return hostBuilder;
        }
    }
}
