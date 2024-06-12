// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client.Configurations
{
    using Democrite.Framework.Client.Model;
    using Democrite.Framework.Client.Services;
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Core.Services;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Diagnostics;

    /// <see cref="IDemocriteClientBuilder" /> implementation
    internal sealed class DemocriteClientBuilder : ClusterBaseBuilder<IDemocriteClientBuilderWizard, IDemocriteCoreConfigurationWizard, DemocriteClientConfigurationDefinition>,
                                                   IDemocriteClientBuilder,
                                                   IDemocriteWizardStart<IDemocriteClientBuilderWizard, IDemocriteCoreConfigurationWizard>,
                                                   IDemocriteClientBuilderWizard,
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
        public sealed override IDemocriteClientBuilderWizard ConfigureLogging(Action<ILoggingBuilder> configureLogging)
        {
            this._orleanClientBuilder.ConfigureServices(collection => collection.AddLogging(configureLogging));
            return this;
        }

        /// <inheritdoc />
        public override IDemocriteClientBuilderWizard NoCluster(string? serviceId = null, string? clusterId = null, bool useLoopback = true)
        {
            this._orleanClientBuilder.UseLocalhostClustering(serviceId: serviceId ?? "dev", clusterId: clusterId ?? "dev");
            return this;
        }

        /// <inheritdoc />
        public override IServiceCollection GetServiceCollection()
        {
            return this._orleanClientBuilder.Services;
        }

        #region Tools

        /// <inheritdoc />
        protected override void OnFinalizeManualBuildConfigure(ILogger logger)
        {
            var serviceDescriptors = this._orleanClientBuilder.Services;

            serviceDescriptors.AddSingleton<GrainOrleanFactory>(p =>
            {
                var orleanGrainFactory = typeof(MembershipEntry).Assembly.GetTypes().FirstOrDefault(d => d.Name == "GrainFactory" &&
                                                                                                        (d.Attributes & System.Reflection.TypeAttributes.NotPublic) == System.Reflection.TypeAttributes.NotPublic &&
                                                                                                         d.IsAssignableTo(typeof(IGrainFactory)));
                Debug.Assert(orleanGrainFactory != null);
                return new GrainOrleanFactory((IGrainFactory)p.GetRequiredService(orleanGrainFactory));
            });

            serviceDescriptors.AddSingleton<IGrainOrleanFactory>(p => p.GetRequiredService<GrainOrleanFactory>());

            serviceDescriptors.AddSingleton<IVGrainDemocriteSystemProvider, VGrainClientDemocriteSystemProvider>();

            base.OnFinalizeManualBuildConfigure(logger);
        }

        /// <inheritdoc />
        protected override IDemocriteClientBuilderWizard GetWizard()
        {
            return this;
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
