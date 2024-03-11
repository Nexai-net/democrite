// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Node.Abstractions.Models;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Hosting;

    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Get open endpoint if no configuration have been provided
    /// </summary>
    /// <seealso cref="IClusterEndpointAutoConfigurator" />
    internal sealed class AutoDefaultNodeEndpointAutoConfigurator : IClusterEndpointAutoConfigurator
    {
        /// <summary>
        /// Automatics configure democrite node endpoint if missings.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The auto configurator must only be used by Node/Server side</exception>
        public void AutoConfigure(IDemocriteClusterBuilder democriteBuilderWizard, IConfiguration configuration, IServiceCollection serviceCollection, ILogger logger)
        {
            var siloBuilder = democriteBuilderWizard.SourceOrleanBuilder as ISiloBuilder;

            if (democriteBuilderWizard.IsClient || siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");

            var endpoint = siloBuilder.Services.AddOptionFromInstOrConfig(configuration,
                                                                          ConfigurationSectionNames.Endpoints,
                                                                          ClusterNodeEndPointOptions.Default,
                                                                          false);

            if (endpoint != null)
            {
                var networkInspector = democriteBuilderWizard.Tools.NetworkInspector;

                var bindAddr = IPAddress.Loopback;

                if (!endpoint.Loopback)
                {
                    var dnsName = networkInspector.GetHostName();
                    var ips = networkInspector.GetHostAddresses(dnsName);

                    if (ips is not null && ips.Count > 1)
                        bindAddr = ips.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);
                    else if (ips is not null && ips.Count == 1)
                        bindAddr = ips.First();

                    if (ips == null || ips.Count == 0)
                        logger.OptiLog(LogLevel.Error, "Could find a correct IP to bind with dns {dnsName}", dnsName);
                }

                logger.OptiLog(LogLevel.Information, "Cluster bind ip {bindAddr}", bindAddr);

                using (var secureContainer = new SafeStructDisposableContainer())
                {
                    var siloPort = (int)endpoint.SiloPort;
                    if (siloPort == 0)
                    {
                        var siloPortSecureToken = networkInspector.GetAndReservedNextUnusedPort(5000, 65536);
                        secureContainer.PushToken(siloPortSecureToken);
                        siloPort = siloPortSecureToken.Token;
                    }

                    var gatewayPort = endpoint.GatewayPort ?? 0;
                    if (gatewayPort == 0 && endpoint.AutoGatewayPort)
                    {
                        var gatewayPortSecureToken = networkInspector.GetAndReservedNextUnusedPort(5000, 65536);
                        secureContainer.PushToken(gatewayPortSecureToken);
                        gatewayPort = gatewayPortSecureToken.Token;
                    }

                    siloBuilder.ConfigureEndpoints(bindAddr,
                                                   siloPort,
                                                   gatewayPort,
                                                   listenOnAnyHostAddress: endpoint.Loopback == false);
                }
            }
        }
    }
}
