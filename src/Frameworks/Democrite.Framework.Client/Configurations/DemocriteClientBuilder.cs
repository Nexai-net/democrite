﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client.Configurations
{
    using Democrite.Framework.Client.Abstractions.Configurations;
    using Democrite.Framework.Client.Abstractions.Models;
    using Democrite.Framework.Client.Model;
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Net;

    /// <see cref="IClusterClientBuilder" /> implementation
    internal sealed class DemocriteClientBuilder : ClusterBaseBuilder<IClusterClientBuilderDemocriteWizard, IDemocriteCoreConfigurationWizard, DemocriteClientConfigurationDefinition>,
                                                   IClusterClientBuilder,
                                                   IDemocriteWizardStart<IClusterClientBuilderDemocriteWizard, IDemocriteCoreConfigurationWizard>,
                                                   IClusterClientBuilderDemocriteWizard,
                                                   IDemocriteCoreConfigurationWizard
    {
        #region Fields

        private readonly INetworkInspector _networkInspector;
        private readonly IClientBuilder _orleanClientBuilder;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteClientBuilder"/> class.
        /// </summary>
        public DemocriteClientBuilder(IHostBuilder host,
                                    IClientBuilder orleanClientBuilder,
                                    HostBuilderContext builderContext,
                                    ClusterBuilderTools clusterBuilderTools)

            : base(host, true, builderContext, clusterBuilderTools)
        {
            ArgumentNullException.ThrowIfNull(clusterBuilderTools.NetworkInspector);

            this._networkInspector = clusterBuilderTools.NetworkInspector;

            this._orleanClientBuilder = orleanClientBuilder;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public sealed override object SourceOrleanBuilder
        {
            get { return this._orleanClientBuilder; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override IDemocriteCoreConfigurationWizard ConfigureLogging(Action<ILoggingBuilder> configureLogging)
        {
            this._orleanClientBuilder.ConfigureServices(collection => collection.AddLogging(configureLogging));
            return this;
        }

        /// <inheritdoc />
        public override IClusterClientBuilderDemocriteWizard NoCluster()
        {
            this._orleanClientBuilder.UseLocalhostClustering();
            return this;
        }

        /// <inheritdoc />
        public override IServiceCollection GetServiceCollection()
        {
            return this._orleanClientBuilder.Services;
        }

        #region Tools

        /// <inheritdoc />
        protected override IClusterClientBuilderDemocriteWizard GetWizard()
        {
            return this;
        }

        /// <inheritdoc />
        protected override void OnFinalizeManualBuildConfigure(ILogger logger)
        {
            var clusters = AddOptionFromInstOrConfig(this._orleanClientBuilder.Services,
                                                     ConfigurationClientSectionNames.Cluster,
                                                     ClusterClientOptions.Default,
                                                     false);

            if (clusters != null)
            {
                var endPoints = SolvedEndpoints(clusters.Endpoints, logger);
                if (endPoints.Any())
                {
                    this._orleanClientBuilder.UseStaticClustering(endPoints.Distinct()
                                                                           .ToArray());
                }
            }

            base.OnFinalizeManualBuildConfigure(logger);
        }

        /// <summary>
        /// Try solve endpoints dns host to provide only IP:PORT from HOST:IP
        /// </summary>
        private List<IPEndPoint> SolvedEndpoints(IReadOnlyCollection<string> endpoints, ILogger logger)
        {
            var endPointsError = new List<string>();
            var endPoints = new List<IPEndPoint>();

            foreach (var endpoint in (endpoints ?? EnumerableHelper<string>.ReadOnly))
            {
                try
                {
                    var solved = this._networkInspector.SolveHostAddresse(endpoint);
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

        /// <inheritdoc />
        protected override DemocriteClientConfigurationDefinition OnBuild(ILogger _)
        {
            return new DemocriteClientConfigurationDefinition();
        }

        #endregion

        #endregion
    }
}
