// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Cluster.Configurations;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.References;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Core.Abstractions.Streams;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Core.Diagnostics;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Core.Streams;
    using Democrite.Framework.Node;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.Abstractions.Inputs;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.Abstractions.Services;
    using Democrite.Framework.Node.Administrations;
    using Democrite.Framework.Node.Artifacts;
    using Democrite.Framework.Node.Components;
    using Democrite.Framework.Node.Configurations;
    using Democrite.Framework.Node.Extensions;
    using Democrite.Framework.Node.Inputs;
    using Democrite.Framework.Node.Models;
    using Democrite.Framework.Node.References;
    using Democrite.Framework.Node.Services;
    using Democrite.Framework.Node.Signals;
    using Democrite.Framework.Node.Streams;
    using Democrite.Framework.Node.Triggers;

    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Loggers;
    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.Models.Converters;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Orleans.Configuration;
    using Orleans.Hosting;
    using Orleans.Providers;
    using Orleans.Serialization.Configuration;
    using Orleans.Services;
    using Orleans.Storage;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <see cref="IDemocriteNodeBuilder" /> implementation
    internal sealed class DemocriteNodeBuilder : ClusterBaseBuilder<IDemocriteNodeWizard, IDemocriteNodeConfigurationWizard, DemocriteNodeConfigurationDefinition>,
                                                 IDemocriteNodeBuilder,
                                                 IDemocriteWizardStart<IDemocriteNodeWizard, IDemocriteNodeConfigurationWizard>,
                                                 IDemocriteNodeWizard,
                                                 IDemocriteExtensionBuilderTool,
                                                 IDemocriteNodeConfigurationWizard,
                                                 IDemocriteNodeSequenceWizard,
                                                 IDemocriteNodeArtifactsWizard,
                                                 IDemocriteNodeMemoryBuilder,
                                                 IDemocriteNodeTriggersWizard,
                                                 IDemocriteNodeSignalsWizard,
                                                 IDemocriteNodeDoorsWizard,
                                                 IDemocriteNodeStreamQueueWizard,
                                                 IDemocriteNodeLocalDefinitionsBuilder

    {
        #region Fields

        private readonly InMemoryTriggerDefinitionProviderSource _triggerDefinitionProviderSource;
        private readonly InMemoryDoorDefinitionProviderSource _doorDefinitionProviderSource;
        private readonly InMemoryArtifactProviderSource _artefactInMemoryProviderSource;
        private readonly InMemorySignalDefinitionProviderSource _signalDefinitionProviderSource;
        private readonly InMemorySequenceDefinitionProvider _inMemorySequenceDefinitionProviderSource;
        private readonly InMemoryStreamQueueDefinitionProviderSource _streamQueueDefinitionProviderSource;

        private readonly INetworkInspector _networkInspector;
        private readonly ISiloBuilder _orleanSiloBuilder;

        private readonly DemocriteNodeClusterOptionWizard _clusterOptionWizard;

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

            this._clusterOptionWizard = new DemocriteNodeClusterOptionWizard(siloBuilder.Services);

            this._inMemorySequenceDefinitionProviderSource = new InMemorySequenceDefinitionProvider(null!);
            this._artefactInMemoryProviderSource = new InMemoryArtifactProviderSource(null!);
            this._triggerDefinitionProviderSource = new InMemoryTriggerDefinitionProviderSource(null!);
            this._signalDefinitionProviderSource = new InMemorySignalDefinitionProviderSource(null!);
            this._doorDefinitionProviderSource = new InMemoryDoorDefinitionProviderSource(null!);
            this._streamQueueDefinitionProviderSource = new InMemoryStreamQueueDefinitionProviderSource(null!);

            this._networkInspector = clusterBuilderTools.NetworkInspector;

            this._orleanSiloBuilder = siloBuilder;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public sealed override object SourceOrleanBuilder
        {
            get { return this._orleanSiloBuilder; }
        }

        /// <inheritdoc />
        public IDemocriteExtensionBuilderTool ConfigurationTools
        {
            get { return this; }
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
            var services = this._orleanSiloBuilder.Services;

            services.AddSingleton(option);
            services.AddSingleton(option.ToOption());
            services.AddSingleton(option.ToMonitorOption());

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
        public IDemocriteNodeWizard AddInMemoryDefinitionProvider(Action<IDemocriteNodeLocalDefinitionsBuilder> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeWizard SetupClusterOptions(Action<IDemocriteNodeClusterOptionWizard> action)
        {
            action?.Invoke(this._clusterOptionWizard);
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
        public IDemocriteNodeWizard SetupNodeMemories(string defaultAutokey, Action<IDemocriteNodeMemoryBuilder>? memoryBuilder = null)
        {
            if (memoryBuilder != null)
                SetupNodeMemories(memoryBuilder);

            var configSection = this.Configuration.GetSection(ConfigurationNodeSectionNames.NodeMemoryDefaultAutoConfigKey);

            if (!configSection.Exists())
            {
                ArgumentNullException.ThrowIfNullOrEmpty(defaultAutokey);

                configSection.Value = defaultAutokey;
            }

            return this;
        }

        /// <inheritdoc />
        public sealed override IDemocriteNodeWizard NoCluster(string? serviceId = null, string? clusterId = null, bool useLoopback = true)
        {
            this._orleanSiloBuilder.Services.AddOptionFromInstOrConfig(this.Configuration,
                                                                       ConfigurationSectionNames.Endpoints,
                                                                       new ClusterNodeEndPointOptions(useLoopback,
                                                                                                      siloPort: EndpointOptions.DEFAULT_SILO_PORT,
                                                                                                      gatewayPort: EndpointOptions.DEFAULT_GATEWAY_PORT));

            this._orleanSiloBuilder.UseLocalhostClustering(serviceId: serviceId ?? "dev", clusterId: clusterId ?? "dev");
            return this;
        }

        #endregion

        #region IDemocriteNodeLocalDefinitionsBuilder

        /// <summary>
        /// Setup callable definitions
        /// </summary>
        public IDemocriteNodeLocalDefinitionsBuilder Setup<TDefinition>(params TDefinition[] definitions)
            where TDefinition : IDefinition, IRefDefinition
        {
            foreach (var def in definitions ?? EnumerableHelper<TDefinition>.ReadOnlyArray)
            {
                _ = def switch
                {
                    SequenceDefinition sequence => SetupSequences(sequence),
                    TriggerDefinition trigger => SetupTriggers(trigger),
                    ArtifactDefinition artifact => SetupArtifacts(artifact),
                    SignalDefinition signal => SetupSignals(signal),
                    DoorDefinition door => SetupDoors(door),
                    StreamQueueDefinition queue => SetupStreamQueues(queue),
                    _ => throw new InvalidOperationException("Definition unknown, may came from extension (if so call the dedicated method) : " + def)
                };
            }
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupSequences(Action<IDemocriteNodeSequenceWizard> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupSequences(params SequenceDefinition[] sequences)
        {
            ArgumentNullException.ThrowIfNull(sequences);
            SetupSequences(s => s.Register(sequences));
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
        public IDemocriteNodeLocalDefinitionsBuilder SetupTriggers(params TriggerDefinition[] triggers)
        {
            ArgumentNullException.ThrowIfNull(triggers);
            SetupTriggers(s => s.Register(triggers));
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
        public IDemocriteNodeLocalDefinitionsBuilder SetupSignals(params SignalDefinition[] signals)
        {
            ArgumentNullException.ThrowIfNull(signals);
            SetupSignals(s => s.Register(signals));
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
        public IDemocriteNodeLocalDefinitionsBuilder SetupDoors(params DoorDefinition[] doors)
        {
            ArgumentNullException.ThrowIfNull(doors);
            SetupDoors(s => s.Register(doors));
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupArtifacts(Action<IDemocriteNodeArtifactsWizard> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupArtifacts(params ArtifactDefinition[] artifacts)
        {
            ArgumentNullException.ThrowIfNull(artifacts);
            SetupArtifacts(a => a.Register(artifacts));
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupStreamQueues(Action<IDemocriteNodeStreamQueueWizard> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            config(this);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeLocalDefinitionsBuilder SetupStreamQueues(params StreamQueueDefinition[] streamQueueDefinitions)
        {
            ArgumentNullException.ThrowIfNull(streamQueueDefinitions);
            SetupStreamQueues(a => a.Register(streamQueueDefinitions));
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
                // TODO : ValidateDefinition(sequence);
                this._inMemorySequenceDefinitionProviderSource.AddOrUpdate(sequenceDefinition);
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

        #region IClusterBuilderDemocriteArtifactBuilder

        ///// <inheritdoc />
        //public IDemocriteNodeArtifactsWizard AddResourceSelector()
        //{
        //    throw new NotImplementedException();
        //}

        /// <inheritdoc />
        public IDemocriteNodeArtifactsWizard AddSourceProvider<TSource>(TSource singletonInstances)
            where TSource : class, IArtifactDefinitionProviderSource
        {
            AddService<IArtifactDefinitionProviderSource>(singletonInstances);
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeArtifactsWizard AddSourceProvider<TSource>()
            where TSource : class, IArtifactDefinitionProviderSource
        {
            AddService<IArtifactDefinitionProviderSource, TSource>();
            return this;
        }

        /// <inheritdoc />
        public IDemocriteNodeArtifactsWizard Register(params ArtifactDefinition[] artifactResource)
        {
            foreach (var artifactDefinition in artifactResource)
                this._artefactInMemoryProviderSource.AddOrUpdate(artifactDefinition);
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

        #region IDemocriteNodeStreamQueueWizard

        /// <summary>
        /// Adds an streamQueue in definition execution.
        /// </summary>
        public IDemocriteNodeStreamQueueWizard Register(params StreamQueueDefinition[] streamQueueDefinitions)
        {
            foreach (var def in streamQueueDefinitions)
            {
                ValidateDefinition(def);
                this._streamQueueDefinitionProviderSource.AddOrUpdate(def);
            }
            return this;
        }

        #endregion

        /// <inheritdoc />
        public TBuilder TryGetBuilder<TBuilder>()
        {
            if (this is TBuilder builder)
                return builder;

            if (this._clusterOptionWizard is TBuilder clusterBuilder)
                return clusterBuilder;

            throw new InvalidCastException(typeof(TBuilder) + " not founded");
        }

        #region Tools

        /// <inheritdoc />
        protected override void OnAutoConfigure(IConfiguration configuration,
                                                IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies,
                                                ILogger logger)
        {
            this._clusterOptionWizard.Build();

            var defaultMemoryAutoKey = configuration.GetSection(ConfigurationNodeSectionNames.NodeMemoryDefaultAutoConfigKey).Get<string>();

            AutoConfigImpl<INodeDemocriteMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(configuration,
                                                                                              "Democrite-System-Storage",
                                                                                              indexedAssemblies,
                                                                                              s => s.GetServiceByKey<string, IGrainStorage>(DemocriteConstants.DefaultDemocriteStateConfigurationKey) != null,
                                                                                              ConfigurationNodeSectionNames.NodeDemocriteMemoryAutoConfigKey,
                                                                                              logger,
                                                                                              defaultAutoKey: defaultMemoryAutoKey);

            AutoConfigImpl<INodeDemocriteAdminMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(configuration,
                                                                                                   "Democrite-Admin-Storage",
                                                                                                   indexedAssemblies,
                                                                                                   s => s.GetServiceByKey<string, IGrainStorage>(DemocriteConstants.DefaultDemocriteAdminStateConfigurationKey) != null,
                                                                                                   ConfigurationNodeSectionNames.NodeDemocriteAdminMemoryAutoConfigKey,
                                                                                                   logger,
                                                                                                   defaultAutoKey: defaultMemoryAutoKey);

            AutoConfigImpl<INodeReminderStateMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(configuration,
                                                                                                  "Orlean-Reminder-Storage",
                                                                                                  indexedAssemblies,
                                                                                                  s => s.Any(d => (d.ServiceType == typeof(IReminderTable))),
                                                                                                  ConfigurationNodeSectionNames.NodeReminderStateMemoryAutoConfigKey,
                                                                                                  logger,
                                                                                                  defaultAutoKey: defaultMemoryAutoKey);

            base.OnAutoConfigure(configuration, indexedAssemblies, logger);

            AutoConfigBasedOnKeys<INodeCustomGrainMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(ConfigurationNodeSectionNames.NodeCustomMemory,
                                                                                                       "Custom-Grain-Storage",
                                                                                                       configuration,
                                                                                                       indexedAssemblies,
                                                                                                       (s, key) => s.GetServiceByKey<string, IGrainStorage>(key) != null,
                                                                                                       logger,
                                                                                                       (c, wizard, key, cfg, service, logger) => c.AutoConfigureCustomStorage(wizard,
                                                                                                                                                                              cfg,
                                                                                                                                                                              service,
                                                                                                                                                                              logger,
                                                                                                                                                                              key,
                                                                                                                                                                              GetConfigurationValue<bool>(cfg, ConfigurationNodeSectionNames.NodeCustomMemory +
                                                                                                                                                                                                               ConfigurationSectionNames.SectionSeparator +
                                                                                                                                                                                                               key +
                                                                                                                                                                                                               ConfigurationSectionNames.SectionSeparator +
                                                                                                                                                                                                               ConfigurationSectionNames.BuildRepository)),
                                                                                                       defaultAutoKey: defaultMemoryAutoKey);

            AutoConfigBasedOnKeys<INodeCustomDefinitionProviderAutoConfigurator, IDemocriteNodeMemoryBuilder>(ConfigurationNodeSectionNames.NodeDefinitionProvider,
                                                                                                              "Custom-Definition-Provider",
                                                                                                              configuration,
                                                                                                              indexedAssemblies,
                                                                                                              (s, key) => s.GetServiceByKey<string, ISequenceDefinitionSourceProvider>(key) != null,
                                                                                                              logger,
                                                                                                              null, //(c, wizard, key, cfg, service, logger) => c.AutoConfigureCustomProvider(wizard, cfg, service, logger, key),
                                                                                                              defaultAutoKey: defaultMemoryAutoKey);

            AutoConfigImpl<INodeDefaultMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(configuration,
                                                                                            "Default-Grain-Storage",
                                                                                            indexedAssemblies,
                                                                                            s => s.GetServiceByKey<string, IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME) != null,
                                                                                            ConfigurationNodeSectionNames.NodeDefaultMemoryAutoConfigKey,
                                                                                            logger,
                                                                                            defaultAutoKey: defaultMemoryAutoKey);

            AutoConfigImpl<IClusterEndpointAutoConfigurator, IDemocriteClusterBuilder>(configuration,
                                                                                       "Cluster-Endpoint",
                                                                                       indexedAssemblies,
                                                                                       s => s.Any(d => (d.ServiceType == typeof(EndpointOptions) && d.ImplementationType != null)),
                                                                                       ConfigurationSectionNames.Endpoints,
                                                                                       logger);

            // Repository
            AutoConfigBasedOnKeys<INodeCustomRepositoryMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(ConfigurationNodeSectionNames.NodeRepositoryStorages,
                                                                                                            "Custom-Repository-Storage",
                                                                                                            configuration,
                                                                                                            indexedAssemblies,
                                                                                                            (s, key) => s.GetServiceByKey<string, IRepositorySpecificFactory>(key) != null,
                                                                                                            logger,
                                                                                                            defaultAutoKey: defaultMemoryAutoKey);

            AutoConfigImpl<INodeDemocriteDynamicDefinitionsMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(configuration,
                                                                                                                "Dynamic-Definitions-Storage",
                                                                                                                indexedAssemblies,
                                                                                                                (s) => s.GetServiceByKey<string, IRepositorySpecificFactory>(DemocriteConstants.DefaultDemocriteDynamicDefinitionsConfigurationKey) != null,
                                                                                                                ConfigurationNodeSectionNames.NodeDemocriteDynamicDefinitionsMemoryAutoConfigKey,
                                                                                                                logger);

            AutoConfigImpl<INodeDefaultRepositoryMemoryAutoConfigurator, IDemocriteNodeMemoryBuilder>(configuration,
                                                                                                      "Default-Repository-Storage",
                                                                                                      indexedAssemblies,
                                                                                                      (s) => s.GetServiceByKey<string, IRepositorySpecificFactory>(DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey) != null,
                                                                                                      ConfigurationNodeSectionNames.NodeRepositoryStoragesDefaultAutoConfigKey,
                                                                                                      logger);

            ConfigDefaultOptions(this.GetServiceCollection(), configuration);
        }

        /// <summary>
        /// Gets the configuration value if exist
        /// </summary>
        private T? GetConfigurationValue<T>(IConfiguration cfg, string key)
        {
            return cfg.GetValue<T>(key) ?? default;
        }

        /// <summary>
        /// Configurations the default options.
        /// </summary>
        private void ConfigDefaultOptions(IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            var existRuntimeOption = serviceDescriptors.FirstOrDefault(s => s.IsKeyedService == false &&
                                                                            (s.ServiceType == typeof(ClusterNodeRuntimeOptions) ||
                                                                             s.ServiceType == typeof(IOptions<ClusterNodeRuntimeOptions>) ||
                                                                             s.ServiceType == typeof(IOptionsMonitor<ClusterNodeRuntimeOptions>)));

            if (existRuntimeOption != null)
                return;

            serviceDescriptors.Configure<ClusterNodeRuntimeOptions>(configuration);
        }

        /// <summary>
        /// Helper use to configured services on each on specific key in the configuration
        /// </summary>
        private void AutoConfigBasedOnKeys<TAutoConfig, TAutoWizard>(string rootConfig,
                                                                     string logActionName,
                                                                     IConfiguration configuration,
                                                                     IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies,
                                                                     Func<IServiceCollection, string, bool> predicateConfigurationExist,
                                                                     ILogger logger,
                                                                     Action<TAutoConfig, TAutoWizard, string, IConfiguration, IServiceCollection, ILogger>? customConfig = null,
                                                                     string? defaultAutoKey = ConfigurationSectionNames.DefaultAutoConfigKey)
            where TAutoConfig : IAutoKeyConfigurator<TAutoWizard>
            where TAutoWizard : IBuilderDemocriteBaseWizard

        {
            var rootSection = configuration.GetSection(rootConfig);
            if (rootSection == null || rootSection.Exists() == false)
                return;

            foreach (var child in rootSection.GetChildren())
            {
                AutoConfigImpl(configuration,
                               logActionName,
                               indexedAssemblies,
                               s => predicateConfigurationExist?.Invoke(s, child.Key) ?? false,

                               rootConfig +
                               ConfigurationSectionNames.SectionSeparator +
                               child.Key +
                               ConfigurationSectionNames.SectionSeparator +
                               ConfigurationSectionNames.AutoConfigKey,

                               logger,
                               customConfig == null
                                    ? (Action<TAutoConfig, TAutoWizard, IConfiguration, IServiceCollection, ILogger>?)null
                                    : (c, wizard, cfg, service, logger) => customConfig?.Invoke(c, wizard, child.Key, cfg, service, logger),
                               defaultAutoKey,
                               child.Key);
            }
        }

        /// <inheritdoc />
        protected override void OnManualBuildConfigure()
        {
            this._orleanSiloBuilder.AddGrainService<ClusterNodeComponentIdentitCardProvider>();

            // Reference
            this._orleanSiloBuilder.AddGrainService<DemocriteTypeReferenceGrainService>();
            this._orleanSiloBuilder.Services.AddSingleton<DemocriteTypeReferenceGrainServiceClient>()
                                            .AddSingleton<IDemocriteTypeReferenceGrainServiceClient>(p => p.GetRequiredService<DemocriteTypeReferenceGrainServiceClient>());

            // Reference Solver
            this._orleanSiloBuilder.Services.AddSingleton<DemocriteReferenceSolverService>()
                                            .AddSingleton<IDemocriteReferenceSolverService>(p => p.GetRequiredService<DemocriteReferenceSolverService>());

            // Grain Service signal relay
            this._orleanSiloBuilder.AddGrainService<SignalLocalGrainServiceRelay>();
            this._orleanSiloBuilder.Services.AddSingleton<SignalLocalGrainServiceRelayClient>()
                                            .AddSingleton<ISignalLocalGrainServiceRelayClient>(p => p.GetRequiredService<SignalLocalGrainServiceRelayClient>())
                                            .AddSingleton<SignalLocalServiceRelay>()
                                            .AddSingleton<ISignalLocalServiceRelay>(p => p.GetRequiredService<SignalLocalServiceRelay>());

            AddService<IComponentIdentitCardProviderClient, ClusterNodeComponentIdentitCardProviderClient>();

            var serviceCollection = this._orleanSiloBuilder.Services;

            if (!CheckIsExistSetupInServices<IObjectConverter>(serviceCollection))
                AddService<IObjectConverter, ObjectConverter>();

            // Check external definitions provider

            serviceCollection.AddSingleton<IDedicatedObjectConverter, SignalMessageDedicatedObjectConverter>();
            serviceCollection.AddSingleton<IDedicatedObjectConverter, ScalarDedicatedConverter>();
            serviceCollection.AddSingleton<IDedicatedObjectConverter, GuidDedicatedConverter>();

            //serviceCollection.TryAddSingleton<IDemocriteSerializer, DemocriteSerializer>();

            serviceCollection.SetupSequenceExecutorThreadStageProvider();

            AddService<ISequenceDefinitionSourceProvider>(this._inMemorySequenceDefinitionProviderSource);
            AddService<IArtifactDefinitionProviderSource>(this._artefactInMemoryProviderSource);
            AddService<ITriggerDefinitionProviderSource>(this._triggerDefinitionProviderSource);
            AddService<ISignalDefinitionProviderSource>(this._signalDefinitionProviderSource);
            AddService<IDoorDefinitionProviderSource>(this._doorDefinitionProviderSource);
            AddService<IStreamQueueDefinitionProviderSource>(this._streamQueueDefinitionProviderSource);

            this._orleanSiloBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IDefinitionSourceProvider<>), typeof(DynamicDefinitionSourceProvider<>)));

            if (!CheckIsExistSetupInServices<IDataSourceProviderFactory>(serviceCollection))
                AddService<IDataSourceProviderFactory, DataSourceProviderFactory>();

            if (!CheckIsExistSetupInServices<IArtifactExecutorFactory>(serviceCollection))
                AddService<IArtifactExecutorFactory, ArtifactExecutorFactory>();

            AddService<IArtifactExecutorDedicatedFactory, ArtifactExecutorCLIDedicatedFactory>();

            if (!CheckIsExistSetupInServices<ITriggerDefinitionProvider>(serviceCollection))
                AddService<IDoorDefinitionProvider, DoorDefinitionProvider>();

            if (!CheckIsExistSetupInServices<IArtifactDefinitionProvider>(serviceCollection))
                AddService<IArtifactDefinitionProvider, ArtifactDefinitionProvider>();

            if (!CheckIsExistSetupInServices<IStreamQueueDefinitionProvider>(serviceCollection))
                AddService<IStreamQueueDefinitionProvider, StreamQueueDefinitionProvider>();

            this._orleanSiloBuilder.AddIncomingGrainCallFilter<IncomingGrainCallTracer>()
                                   .AddIncomingGrainCallFilter<GrainPopulateCancellationTokenCallFilter>();

            this._orleanSiloBuilder.AddGrainService<SignalTriggerVGrainService>();
            this._orleanSiloBuilder.AddGrainService<AdministrationGrainService>();

            base.OnManualBuildConfigure();
        }

        /// <inheritdoc />
        protected sealed override void OnFinalizeManualBuildConfigure(ILogger logger)
        {
            this._orleanSiloBuilder.Services.AddOptionFromInstOrConfig(this.Configuration,
                                                                       ConfigurationNodeSectionNames.Diagnostics,
                                                                       ClusterNodeDiagnosticOptions.Default,
                                                                       false);

            base.OnFinalizeManualBuildConfigure(logger);

            // Route & redirection services
            this._orleanSiloBuilder.Services.SetupGrainRoutingServices();

            AddDemocriteSystemDefinitions(this._orleanSiloBuilder.Services);

            var speSourceProviderTrait = typeof(ISpecificDefinitionSourceProvider);
            var serviceCollection = this._orleanSiloBuilder.Services;
            foreach (var desc in serviceCollection.ToArray())
            {
                var serviceType = desc.ServiceType;
                var implServiceType = desc.IsKeyedService ? desc.KeyedImplementationType : desc.ImplementationType;
                var impl = desc.IsKeyedService ? desc.KeyedImplementationInstance : desc.ImplementationInstance;

                var implType = impl?.GetType();

                if ((serviceType != speSourceProviderTrait && serviceType.IsAssignableTo(speSourceProviderTrait)) ||
                    (implServiceType is not null && implServiceType != speSourceProviderTrait && implServiceType.IsAssignableTo(speSourceProviderTrait)) ||
                    (implType is not null && implType != speSourceProviderTrait && implType.IsAssignableTo(speSourceProviderTrait)))
                {
                    serviceCollection.Add(new ServiceDescriptor(speSourceProviderTrait,
                                                                p => desc.IsKeyedService ? p.GetRequiredKeyedService(desc.ServiceType, desc.ServiceKey) : p.GetRequiredService(desc.ServiceType),
                                                                ServiceLifetime.Singleton));
                }
            }
        }

        /// <summary>
        /// Adds the democrite system definitions.
        /// </summary>
        private void AddDemocriteSystemDefinitions(IServiceCollection serviceDescriptors)
        {
            var inMemoryStorage = serviceDescriptors.Where(s => s.IsKeyedService == false &&
                                                                 s.ImplementationInstance is IDefinitionInMemoryFillSourceProvider)
                                                    .Select(s => (IDefinitionInMemoryFillSourceProvider)s.ImplementationInstance!)
                                                    .Distinct()
                                                    .ToArray();

            var preferences = new Dictionary<Type, IDefinitionInMemoryFillSourceProvider>();

            foreach (var definition in DemocriteSystemDefinitions.GetAllSystemDefinitions())
            {
                var defType = definition.GetType();

                if (preferences.TryGetValue(defType, out var preferedStoreDef) && preferedStoreDef.CanStore(definition))
                {
                    preferedStoreDef.TryStore(definition);
                    continue;
                }

                bool stored = false;
                foreach (var storeDef in inMemoryStorage)
                {
                    if (storeDef.CanStore(definition) && storeDef.TryStore(definition))
                    {
                        if (!preferences.ContainsKey(defType))
                            preferences.Add(defType, storeDef);

                        stored = true;
                        break;
                    }
                }

                if (stored == false)
                    throw new Exception("Couldn't store definition " + definition);
            }
        }

        /// <inheritdoc />
        protected sealed override DemocriteNodeConfigurationDefinition OnBuild(ILogger logger)
        {
            var logs = this.MemoryLogger.GetLogsCopy();

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
        /// Get a local logger used to trace build informations
        /// </summary>
        private RelayLogger CreateBuilderLogger(string category)
        {
            return new RelayLogger(this.MemoryLogger, category, true);
        }

        #endregion

        #endregion
    }
}
