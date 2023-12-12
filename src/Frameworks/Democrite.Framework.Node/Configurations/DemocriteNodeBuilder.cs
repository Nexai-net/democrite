// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Core.Diagnostics;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Node.Abstractions.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.ArtifactResources;
    using Democrite.Framework.Node.ArtifactResources.ExecCodePreparationSteps;
    using Democrite.Framework.Node.Components;
    using Democrite.Framework.Node.Inputs;
    using Democrite.Framework.Node.Models;
    using Democrite.Framework.Node.Services;
    using Democrite.Framework.Node.Signals;
    using Democrite.Framework.Node.Triggers;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Loggers;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Orleans.Hosting;
    using Orleans.Runtime;
    using Orleans.Serialization.Configuration;
    using Orleans.Storage;

    using System;
    using System.Collections.Generic;
    using System.Net;

    /// <see cref="IDemocriteNodeBuilder" /> implementation
    internal sealed class DemocriteNodeBuilder : ClusterBaseBuilder<IDemocriteNodeWizard, IDemocriteNodeConfigurationWizard, DemocriteNodeConfigurationDefinition>,
                                                 IDemocriteNodeBuilder,
                                                 IDemocriteWizardStart<IDemocriteNodeWizard, IDemocriteNodeConfigurationWizard>,
                                                 IDemocriteNodeWizard,
                                                 IDemocriteNodeConfigurationWizard,
                                                 IDemocriteNodeSequenceWizard,
                                                 IDemocriteNodeArtifacResourceBuilder,
                                                 IDemocriteNodeMemoryBuilder,
                                                 IDemocriteNodeTriggersWizard,
                                                 IDemocriteNodeSignalsWizard,
                                                 IDemocriteNodeDoorsWizard,
                                                 IDemocriteNodeLocalDefinitionsBuilder

    {
        #region Fields

        private readonly InMemoryTriggerDefinitionProviderSource _triggerDefinitionProviderSource;
        private readonly InMemoryDoorDefinitionProviderSource _doorDefinitionProviderSource;
        private readonly InMemoryArtifactResourceProviderSource _artefactInMemoryProviderSource;
        private readonly InMemorySignalDefinitionProviderSource _signalDefinitionProviderSource;
        private readonly InMemorySequenceDefinitionProvider _inMemorySequenceDefinition;
        private readonly InMemoryLogger _globalBuildLogger;
        private readonly INetworkInspector _networkInspector;
        private readonly ISiloBuilder _orleanSiloBuilder;

        private ClusterNodeVGrainBuilder? _vgrainsCfg;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteNodeBuilder"/> class.
        /// </summary>
        /// <param name="host">The global host builder.</param>
        /// <param name="siloBuilder">The orlean client builder.</param>
        internal DemocriteNodeBuilder(IHostBuilder host,
                                      ISiloBuilder siloBuilder,
                                      HostBuilderContext builderContext,
                                      ClusterBuilderTools clusterBuilderTools)

            : base(host, false, builderContext, clusterBuilderTools)
        {
            ArgumentNullException.ThrowIfNull(clusterBuilderTools.NetworkInspector);

            this._inMemorySequenceDefinition = new InMemorySequenceDefinitionProvider();
            this._artefactInMemoryProviderSource = new InMemoryArtifactResourceProviderSource();
            this._triggerDefinitionProviderSource = new InMemoryTriggerDefinitionProviderSource();
            this._signalDefinitionProviderSource = new InMemorySignalDefinitionProviderSource();
            this._doorDefinitionProviderSource = new InMemoryDoorDefinitionProviderSource();

            this._networkInspector = clusterBuilderTools.NetworkInspector;

            this._orleanSiloBuilder = siloBuilder;

            this._globalBuildLogger = new InMemoryLogger(new LoggerFilterOptions() { MinLevel = LogLevel.Trace }.ToMonitorOption());
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public sealed override object SourceOrleanBuilder
        {
            get { return this._orleanSiloBuilder; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ISiloBuilder ManualyAdvancedConfig()
        {
            return this._orleanSiloBuilder;
        }

        /// <inheritdoc />
        public override IServiceCollection GetServiceCollection()
        {
            return this._orleanSiloBuilder.Services;
        }

        #region IClusterNodeBuilderDemocriteWizard

        /// <inheritdoc />
        public IDemocriteNodeWizard AddNodeOption<TOption>(TOption option)
             where TOption : class, INodeOptions
        {
            this._orleanSiloBuilder.Services.ConfigureOptions(option);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeWizard AddNodeOption<TOption>(string configurationSection)
             where TOption : class, INodeOptions
        {
            var cfg = this.Configuration.GetRequiredSection(configurationSection);
            this._orleanSiloBuilder.Configure<TOption>(cfg);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeWizard SetupNodeVGrains(Action<IDemocriteNodeVGrainWizard> config)
        {
            var vgrainsCfg = this._vgrainsCfg;

            if (vgrainsCfg == null)
            {
                vgrainsCfg = new ClusterNodeVGrainBuilder(this);
                this._vgrainsCfg = vgrainsCfg;
            }

            config?.Invoke(vgrainsCfg);

            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeWizard SetupInMemoryDefintions(Action<IDemocriteNodeLocalDefinitionsBuilder> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeWizard SetupNodeMemories(Action<IDemocriteNodeMemoryBuilder> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public sealed override IDemocriteNodeWizard NoCluster()
        {
            this._orleanSiloBuilder.UseLocalhostClustering();
            return this;
        }

        #endregion

        #region IDemocriteNodeLocalDefinitionsBuilder

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupSequences(Action<IDemocriteNodeSequenceWizard> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupTriggers(Action<IDemocriteNodeTriggersWizard> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupSignals(Action<IDemocriteNodeSignalsWizard> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupDoors(Action<IDemocriteNodeDoorsWizard> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupArtifactResources(Action<IDemocriteNodeArtifacResourceBuilder> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        #endregion

        #region IDemocriteNodeMemoryBuilder

        /// <inheritdoc />
        public IDemocriteNodeMemoryBuilder UseInMemoryVGrainStateMemory()
        {
            this._orleanSiloBuilder.AddMemoryGrainStorage(nameof(Democrite));
            this._orleanSiloBuilder.AddMemoryGrainStorageAsDefault();
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeMemoryBuilder UseInMemoryTriggerReminderMemory()
        {
            this._orleanSiloBuilder.UseInMemoryReminderService();
            return this;
        }

        #endregion

        #region IClusterNodeBuilderDemocriteConfigurationWizard

        /// <inheritdoc />
        public sealed override IDemocriteNodeWizard ConfigureLogging(Action<ILoggingBuilder> configureLogging)
        {
            this._orleanSiloBuilder.ConfigureLogging(configureLogging);
            return this;
        }

        #endregion

        #region IClusterNodeBuilderDemocriteSequenceWizard

        /// <inheritdoc />
        public IDemocriteNodeSequenceWizard Register(params SequenceDefinition[] sequenceDefinitions)
        {
            foreach (var sequenceDefinition in sequenceDefinitions)
            {
                // TODO : ValidateDefinition(sequenceDefinition);
                this._inMemorySequenceDefinition.AddOrUpdate(sequenceDefinition);
            }

            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeSequenceWizard Register<TSequenceModule>()
            where TSequenceModule : ISequenceModule, new()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IClusterBuilderDemocriteArtifacResourceBuilder

        /// <inheritdoc />
        public IDemocriteNodeArtifacResourceBuilder AddResourceSelector()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IDemocriteNodeArtifacResourceBuilder AddSourceProvider<TSource>(TSource singletonInstances)
            where TSource : class, IArtifactResourceProviderSource
        {
            AddService<IArtifactResourceProviderSource>(singletonInstances);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeArtifacResourceBuilder AddSourceProvider<TSource>()
            where TSource : class, IArtifactResourceProviderSource
        {
            AddService<IArtifactResourceProviderSource, TSource>();
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeArtifacResourceBuilder AddLocalArtifact(IArtifactResource artifactResource)
        {
            this._artefactInMemoryProviderSource.AddOrUpdate(artifactResource);
            return this;
        }

        #endregion

        #region IDemocriteNodeConfigurationWizard

        /// <inheritdoc />
        public IDemocriteNodeConfigurationWizard AddDiagnostic(ClusterNodeDiagnosticOptions options)
        {
            AddService(options.ToOption());
            AddService(options.ToMonitorOption());
            return this;
        }

        /// <summary>
        /// Enables the diagnostic log to be relay to classic logger.
        /// </summary>
        public IDemocriteNodeConfigurationWizard EnableDiagnosticRelayToLogger()
        {
            AddService<IDiagnosticLogConsumer, DiagnosticLogConsumerToLogger>();
            return this;
        }

        #endregion

        #region IDemocriteNodeTriggersWizard

        /// <inheritdoc />
        public IDemocriteNodeTriggersWizard Register(params TriggerDefinition[] triggerDefinitions)
        {
            foreach (var triggerDefinition in triggerDefinitions)
            {
                // TODO : ValidateDefinition(triggerDefinition);
                this._triggerDefinitionProviderSource.AddOrUpdate(triggerDefinition);
            }

            return this;
        }

        #endregion

        #region IDemocriteNodeSignalsWizard

        /// <inheritdoc />
        public IDemocriteNodeSignalsWizard Register(params SignalDefinition[] signalDefinitions)
        {
            foreach (var signalDefinition in signalDefinitions)
            {
                ValidateDefinition(signalDefinition);
                this._signalDefinitionProviderSource.AddOrUpdate(signalDefinition);
            }
            return this;
        }

        #endregion

        #region IDemocriteNodeDoorsWizard

        /// <inheritdoc />
        public IDemocriteNodeDoorsWizard Register(params DoorDefinition[] signalDefinitions)
        {
            foreach (var signalDefinition in signalDefinitions)
            {
                ValidateDefinition(signalDefinition);
                this._doorDefinitionProviderSource.AddOrUpdate(signalDefinition);
            }
            return this;
        }

        #endregion

        #region Tools

        /// <inheritdoc />
        protected override void OnAutoConfigure(IConfiguration configuration, IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies, ILogger logger)
        {
            AutoConfigVGrainStateMemory(configuration, indexedAssemblies, logger);
            AutoConfigReminderStateMemory(configuration, indexedAssemblies, logger);

            base.OnAutoConfigure(configuration, indexedAssemblies, logger);
        }

        /// <inheritdoc />
        protected override void OnManualBuildConfigure()
        {
            this._orleanSiloBuilder.AddGrainService<ClusterNodeComponentIdentitCardProvider>();
            AddService<IComponentIdentitCardProviderClient, ClusterNodeComponentIdentitCardProviderClient>();

            var serviceCollection = this._orleanSiloBuilder.Services;

            AddService<ISequenceDefinitionSourceProvider>(this._inMemorySequenceDefinition);
            AddService<IArtifactResourceProviderSource>(this._artefactInMemoryProviderSource);
            AddService<ITriggerDefinitionProviderSource>(this._triggerDefinitionProviderSource);
            AddService<ISignalDefinitionProviderSource>(this._signalDefinitionProviderSource);
            AddService<IDoorDefinitionProviderSource>(this._doorDefinitionProviderSource);

            if (!CheckIsExistSetupInServices<IInputSourceProviderFactory>(serviceCollection))
                AddService<IInputSourceProviderFactory, InputSourceProviderFactory>();

            if (!CheckIsExistSetupInServices<IExternalCodePackageFactory>(serviceCollection))
                AddService<IExternalCodePackageFactory, ExternalCodePackageFactory>();

            if (!CheckIsExistSetupInServices<ITriggerDefinitionProvider>(serviceCollection))
                AddService<IDoorDefinitionProvider, DoorDefinitionProvider>();

            if (!CheckIsExistSetupInServices<IArtifactResourceProvider>(serviceCollection))
                AddService<IArtifactResourceProvider, ArtifactResourceProvider>();

            // Artefacts
            serviceCollection.AddSingletonKeyedService<string, IExternalCodeExecutorPreparationStep, PreparationLocalCheckStep>(PreparationLocalCheckStep.KEY);
            serviceCollection.AddSingletonKeyedService<string, IExternalCodeExecutorPreparationStep, PreparationExecutorCheckStep>(PreparationExecutorCheckStep.KEY);

            this._orleanSiloBuilder.AddIncomingGrainCallFilter<IncomingGrainCallTracer>()
                                   .AddIncomingGrainCallFilter<GrainPopulateCancellationTokenCallFilter>();

            this._orleanSiloBuilder.AddGrainService<SignalTriggerVGrainService>();

            base.OnManualBuildConfigure();
        }

        /// <inheritdoc />
        protected sealed override void OnFinalizeManualBuildConfigure(ILogger logger)
        {
            AddOptionFromInstOrConfig(this._orleanSiloBuilder.Services,
                                      ConfigurationNodeSectionNames.Diagnostics,
                                      ClusterNodeDiagnosticOptions.Default,
                                      false);

            var endpoint = AddOptionFromInstOrConfig(this._orleanSiloBuilder.Services,
                                                     ConfigurationNodeSectionNames.Endpoints,
                                                     ClusterNodeEndPointOptions.Default,
                                                     false);

            if (endpoint != null)
            {
                var bindAddr = IPAddress.Loopback;

                if (!endpoint.Loopback)
                {
                    var ips = this._networkInspector.GetHostAddresses(this._networkInspector.GetHostName()); // Dns.GetHostAddresses(Dns.GetHostName());
                    bindAddr = ips.First();
                }

                logger.LogInformation("Bind ip {bindAddr}", bindAddr);

                this._orleanSiloBuilder.ConfigureEndpoints(bindAddr,
                                                           endpoint.SiloPort == 0
                                                                ? this._networkInspector.GetNextUnusedPort(5000, 65536) //NetworkHelper.GetNextUnusedPort(5000, 65536)
                                                                : (int)endpoint.SiloPort,
                                                           endpoint.GatewayPort ?? 0,
                                                           listenOnAnyHostAddress: endpoint.Loopback == false);
            }

            base.OnFinalizeManualBuildConfigure(logger);
        }

        /// <inheritdoc />
        protected sealed override DemocriteNodeConfigurationDefinition OnBuild(ILogger logger)
        {
            var logs = this._globalBuildLogger.GetLogsCopy();

            foreach (var log in logs)
                logger.Log(log.LogLevel, log.Message);

            var manifest = this._vgrainsCfg?.Setup(this._orleanSiloBuilder.Services) ?? new TypeManifestOptions();

            return new DemocriteNodeConfigurationDefinition(manifest);
        }

        /// <inheritdoc />
        protected override IDemocriteNodeWizard GetWizard()
        {
            return this;
        }

        /// <summary>
        /// Automatics the configuration vgrain state memory.
        /// </summary>
        private void AutoConfigVGrainStateMemory(IConfiguration configuration,
                                               IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies,
                                               ILogger logger)
        {
            AutoConfigImpl<INodeVGrainStateMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(configuration,
                                                                                               indexedAssemblies,
                                                                                               s => s.Any(d => (d.ServiceType == typeof(IGrainStorage))),
                                                                                               ConfigurationSectionNames.NodeVGrainStateMemoryMembershipAutoConfigKey,
                                                                                               logger);
        }

        /// <summary>
        /// Automatics the configuration reminder state memory.
        /// </summary>
        private void AutoConfigReminderStateMemory(IConfiguration configuration,
                                       IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies,
                                       ILogger logger)
        {
            AutoConfigImpl<INodeReminderStateMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(configuration,
                                                                                                  indexedAssemblies,
                                                                                                  s => s.Any(d => (d.ServiceType == typeof(IReminderTable))),
                                                                                                  ConfigurationSectionNames.NodeReminderStateMemoryMembershipAutoConfigKey,
                                                                                                  logger);
        }

        /// <summary>
        /// Validates the definition before registry in local memory
        /// </summary>
        private void ValidateDefinition<TDefinition>(TDefinition definition)
            where TDefinition : class, IDefinition
        {
            ArgumentNullException.ThrowIfNull(definition);

            using (var contextualLogger = CreateBuilderLogger(typeof(TDefinition).Name))
            {
                if (!definition.Validate(contextualLogger))
                    throw new InvalidDecmocriteDefinitionException(definition, contextualLogger.GetLogsCopy()!);
            }
        }

        /// <summary>
        /// Create a local logger used to trace build informations
        /// </summary>
        private RelayLogger CreateBuilderLogger(string category)
        {
            return new RelayLogger(this._globalBuildLogger, category, true);
        }
        
        #endregion

        #endregion
    }
}
