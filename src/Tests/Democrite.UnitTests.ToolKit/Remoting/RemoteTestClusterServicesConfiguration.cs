// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Remoting
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Orleans.TestingHost;

    using System;
    using System.Linq;

    public sealed class RemoteTestClusterServicesConfiguration : IHostConfigurator
    {
        public void Configure(IHostBuilder hostBuilder)
        {
            var cfg = hostBuilder.GetConfiguration();

            var remoteKeys = cfg.AsEnumerable()
                                .Where(kv => kv.Key.StartsWith(TestRemotingServiceBuilder.PrefixConfigKeys, StringComparison.OrdinalIgnoreCase))
                                .ToArray();

            hostBuilder.ConfigureServices(s =>
            {
                foreach (var kv in remoteKeys)
                {
                    if (string.IsNullOrEmpty(kv.Value) || string.IsNullOrEmpty(kv.Key))
                        continue;

                    var serviceType = Type.GetType(kv.Value);
                    ArgumentNullException.ThrowIfNull(serviceType);

                    s.Add(new ServiceDescriptor(serviceType, serviceType.CreateRemoteServiceFrom(kv.Key)!.Object));
                }
            });
        }
    }
}
