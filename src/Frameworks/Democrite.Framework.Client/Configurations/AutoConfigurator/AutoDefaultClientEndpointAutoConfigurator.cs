// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations.AutoConfigurator
{
    using Democrite.Framework.Client.Abstractions.Models;
    using Democrite.Framework.Client.Configurations;
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Configurations;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Hosting;

    using System;
    using System.Net;

    /// <summary>
    /// Get open endpoint if no configuration have been provided
    /// </summary>
    /// <seealso cref="IClusterEndpointAutoConfigurator" />
    internal sealed class AutoDefaultClientEndpointAutoConfigurator : IClusterEndpointAutoConfigurator
    {
        #region Methods

        /// <summary>
        /// Automatics configure democrite client endpoint if missings.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The auto configurator must only be used by client side</exception>
        public void AutoConfigure(IDemocriteClusterBuilder democriteBuilderWizard, IConfiguration configuration, IServiceCollection serviceCollection, ILogger logger)
        {
            var clientBuilder = democriteBuilderWizard.SourceOrleanBuilder as IClientBuilder;

            if (!democriteBuilderWizard.IsClient || clientBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by client side");

            var clusters = clientBuilder.Services.AddOptionFromInstOrConfig(configuration,
                                                                            ConfigurationClientSectionNames.Cluster,
                                                                            ClusterClientOptions.Default,
                                                                            false);

            if (clusters != null)
            {
                var endPoints = SolveEndpoints(democriteBuilderWizard.Tools.NetworkInspector, clusters.Endpoints, logger);
                if (endPoints.Any())
                {
                    clientBuilder.UseStaticClustering(endPoints.Distinct().ToArray());
                }
            }
        }

        /// <summary>
        /// Solve all the endpoint dns name provide to return only <see cref="IPEndPoint"/>
        /// Try solve endpoints dns host to provide only IP:PORT from HOST:IP
        /// </summary>
        private static List<IPEndPoint> SolveEndpoints(INetworkInspector networkInspector, IReadOnlyCollection<string> endpoints, ILogger logger)
        {
            var endPointsError = new List<string>();
            var endPoints = new List<IPEndPoint>();

            foreach (var endpoint in (endpoints ?? EnumerableHelper<string>.ReadOnly))
            {
                try
                {
                    var solved = networkInspector.SolveHostAddresse(endpoint);
                    foreach (var s in solved)
                    {
                        try
                        {
                            var ipEndpoint = IPEndPoint.Parse(s);
                            if (ipEndpoint != null)
                                endPoints.Add(ipEndpoint);
                        }
                        catch (Exception parseEx)
                        {
                            endPointsError.Add("IPEndpoint " + endpoint + " Parse (" + s + ") - " + parseEx.Message);
                        }
                    }
                }
                catch (Exception solvingEx)
                {
                    endPointsError.Add("IPEndpoint " + endpoint + " - " + solvingEx.Message);
                }
            }

            foreach (var error in endPointsError)
                logger.LogError(error);

            return endPoints;
        }

        #endregion
    }
}
