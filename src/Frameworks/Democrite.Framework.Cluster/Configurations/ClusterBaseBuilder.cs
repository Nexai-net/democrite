// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Attributes;
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Cluster.Abstractions.Exceptions;
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Core.Extensions;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Core.Triggers;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Loggers;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    using Orleans.Configuration;
    using Orleans.Messaging;
    using Orleans.Runtime;

    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// base builder class 
    /// </summary>
    /// <typeparam name="TWizard">The type of the wizard.</typeparam>
    /// <seealso cref="Abstractions.Configurations.Builders.IDemocriteBuilder&lt;TWizard&gt;" />
    public abstract class ClusterBaseBuilder<TWizard, TWizardConfig, TResultConfig> : IDemocriteBuilder<TWizard, TWizardConfig>,
                                                                                      IDemocriteCoreConfigurationWizard<TWizardConfig>,
                                                                                      IDemocriteWizard<TWizard, TWizardConfig>,
                                                                                      IDemocriteWizardStart<TWizard, TWizardConfig>,
                                                                                      IDemocriteClusterBuilder,
                                                                                      IClusterOptionBuilder

        where TWizard : IDemocriteWizard<TWizard, TWizardConfig>
        where TWizardConfig : IDemocriteCoreConfigurationWizard<TWizardConfig>
    {
        #region Fields

        private readonly IAssemblyInspector _assemblyInspector;
        private readonly IFileSystemHandler _fileSystemHandler;
        private readonly INetworkInspector _networkInspector;
        private readonly IAssemblyLoader _assemblyLoader;

        private readonly HostBuilderContext _context;

        private readonly Queue<Action<IServiceCollection>> _options;
        private readonly Queue<ServiceDescriptor> _services;
        private readonly IHostBuilder _host;
        private bool _blockAutoConfig;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterBaseBuilder{TWizard}"/> class.
        /// </summary>
        protected ClusterBaseBuilder(IHostBuilder host,
                                     bool isClient,
                                     HostBuilderContext context,
                                     ClusterBuilderTools clusterBuilderTools)
        {
            ArgumentNullException.ThrowIfNull(host);
            ArgumentNullException.ThrowIfNull(clusterBuilderTools);
            ArgumentNullException.ThrowIfNull(clusterBuilderTools.AssemblyInspector);
            ArgumentNullException.ThrowIfNull(clusterBuilderTools.AssemblyLoader);
            ArgumentNullException.ThrowIfNull(clusterBuilderTools.FileSystemHandler);

            this._host = host;

            this.Tools = clusterBuilderTools;

            this._assemblyInspector = clusterBuilderTools.AssemblyInspector;
            this._fileSystemHandler = clusterBuilderTools.FileSystemHandler;
            this._assemblyLoader = clusterBuilderTools.AssemblyLoader;
            this._networkInspector = clusterBuilderTools.NetworkInspector;

            this._context = context;

            this.IsClient = isClient;
            this.IsServerNode = !isClient;

            this._options = new Queue<Action<IServiceCollection>>();
            this._services = new Queue<ServiceDescriptor>();

            this.MemoryLogger = new InMemoryLogger(new LoggerFilterOptions() { MinLevel = LogLevel.Trace }.ToMonitorOption());
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsClient { get; }

        /// <inheritdoc />
        public bool IsServerNode { get; }

        /// <inheritdoc />
        public abstract object SourceOrleanBuilder { get; }

        /// <summary>
        /// Gets the build tools.
        /// </summary>
        public IClusterBuilderTools Tools { get; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        protected IConfiguration Configuration
        {
            get { return this._context.Configuration; }
        }

        /// <summary>
        /// Gets the build logger.
        /// </summary>
        public ILogger Logger
        {
            get { return this.MemoryLogger; }
        }

        /// <summary>
        /// Gets the build logger.
        /// </summary>
        protected InMemoryLogger MemoryLogger { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TWizard Configure(Action<TWizardConfig> configureDelegate)
        {
            ArgumentNullException.ThrowIfNull(configureDelegate);

            var cfg = GetConfigurationBuilder();

            configureDelegate(cfg);
            return GetWizard();
        }

        /// <inheritdoc />
        public TWizard ConfigureServices(Action<IServiceCollection> config)
        {
            config(GetServiceCollection());
            return GetWizard();
        }

        /// <inheritdoc />
        public virtual IDemocriteWizardStart<TWizard, TWizardConfig> WizardConfig()
        {
            return this;
        }

        /// <inheritdoc />
        public abstract TWizard NoCluster();

        /// <inheritdoc />
        public TWizard SetupCluster(Action<IDemocriteClusterBuilder> clusterWizard)
        {
            SetupCluster((wizard, _) => clusterWizard(wizard));
            return GetWizard();
        }

        /// <inheritdoc />
        public TWizard SetupCluster(Action<IDemocriteClusterBuilder, IConfiguration> clusterWizard)
        {
            clusterWizard(this, this._context.Configuration);
            return GetWizard();
        }

        /// <inheritdoc />
        public TWizard SetupClusterFromConfig(string? configurationKey = null)
        {
            var section = this._context.Configuration.GetSection(ConfigurationSectionNames.ClusterMembership);

            if (section == null || section.Exists() == false)
                throw new MissingRequiredDemocriteConfigurationException(ConfigurationSectionNames.ClusterMembership, "How to join the cluster. It need a data source to provide the information about other silo nodes");

            var autoConfigSection = this._context.Configuration.GetSection(ConfigurationSectionNames.ClusterMembershipAutoConfigKey);

            if (!autoConfigSection.Exists() && !string.IsNullOrEmpty(configurationKey))
            {
                // Force auto configuration value
                autoConfigSection.Value = configurationKey;
            }

            return GetWizard();
        }

        #region IClusterBuilderDemocriteConfigurationWizard

        /// <inheritdoc />
        public TWizardConfig Add(ServiceDescriptor serviceDescriptor)
        {
            this._services.Enqueue(serviceDescriptor);
            return GetConfigurationBuilder();
        }

        /// <inheritdoc />
        public TWizardConfig AddService<TService>(TService instance)
            where TService : class
        {
            return AddService(typeof(TService), instance);
        }

        /// <inheritdoc />
        public TWizardConfig AddService(Type service, Type instanceType)
        {
            this._services.Enqueue(ServiceDescriptor.Singleton(service, instanceType));
            return GetConfigurationBuilder();
        }

        public TWizardConfig AddService(Type service, object instance)
        {
            this._services.Enqueue(ServiceDescriptor.Singleton(service, instance));
            return GetConfigurationBuilder();
        }

        /// <inheritdoc />
        public TWizardConfig AddService<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            this._services.Enqueue(ServiceDescriptor.Singleton<TService, TImplementation>());
            return GetConfigurationBuilder();
        }

        /// <inheritdoc />
        public TWizardConfig AddOptionMapping<TOptions>(string section)
            where TOptions : class
        {
            var services = GetServiceCollection();
            services.Configure<TOptions>(this._context.Configuration.GetSection(section));
            return GetConfigurationBuilder();
        }

        /// <inheritdoc />
        public void AddExtensionOption<TOptions>(TOptions instance) where TOptions : class
        {
            if (instance is not null)
            {
                var services = GetServiceCollection();
                services.AddSingleton<IOptions<TOptions>>(instance.ToOption());
                services.AddSingleton<IOptionsMonitor<TOptions>>(instance.ToMonitorOption());
            }
        }

        /// <inheritdoc />
        public abstract TWizard ConfigureLogging(Action<ILoggingBuilder> configureLogging);

        #endregion

        /// <inheritdoc />
        public void AddExtensionOption<TOption>(Action<TOption> optionsCfg) where TOption : class
        {
            AddOption<TOption>(optionsCfg);
        }

        /// <inheritdoc />
        public TOption AddExtensionOption<TOption>(string configurationSection, TOption fallbackOption)
            where TOption : class
        {
            var services = GetServiceCollection();

            var config = this._context.Configuration.GetSection(configurationSection);

            if (config != null && config.Exists())
            {
                var opt = config.Get<TOption>();

                if (opt != null)
                {
                    services.Configure<TOption>(config);
                    return opt;
                }
            }

            ArgumentNullException.ThrowIfNull(fallbackOption);
            AddExtensionOption(fallbackOption);
            return fallbackOption;
        }

        /// <inheritdoc />
        public IDemocriteClusterBuilder AddGatewayListProvider<TListProvider>()
            where TListProvider : class, IGatewayListProvider
        {
            AddService<IGatewayListProvider, TListProvider>();
            return this;
        }

        /// <inheritdoc />
        public IDemocriteClusterBuilder AddConfigurationValidator<TValidator>()
            where TValidator : class, IConfigurationValidator
        {
            AddService<IConfigurationValidator, TValidator>();
            return this;
        }

        /// <inheritdoc />
        public IDemocriteClusterBuilder AddMembershipTable<TMembership>()
            where TMembership : class, IMembershipTable
        {
            AddService<IMembershipTable, TMembership>();
            return this;
        }

        /// <inheritdoc />
        public abstract IServiceCollection GetServiceCollection();

        /// <inheritdoc />
        public IConfiguration GetConfiguration()
        {
            return this.Configuration;
        }

        #region IClusterOptionBuilder

        /// <inheritdoc />
        public IClusterOptionBuilder BlockAutoConfig()
        {
            this._blockAutoConfig = true;
            return this;
        }

        #endregion

        /// <summary>
        /// Setups the cluster option.
        /// </summary>
        public IDemocriteBuilder<TWizard, TWizardConfig> SetupClusterOption(Action<IClusterOptionBuilder> configCallback)
        {
            configCallback?.Invoke(this);

            return this;
        }

        /// <summary>
        /// Builds democrite clsuter part
        /// </summary>
        public TResultConfig Build(HostBuilderContext context)
        {
            var serviceCollection = GetServiceCollection();

            AddService<IFileSystemHandler>(this._fileSystemHandler);
            AddService<IAssemblyInspector>(this._assemblyInspector);
            AddService<INetworkInspector>(this._networkInspector);
            AddService<IAssemblyLoader>(this._assemblyLoader);

            serviceCollection.SetupCoreServices();

            OnManualBuildConfigure();

            if (!CheckIsExistSetupInServices<ITriggerDefinitionProvider>(serviceCollection))
                AddService<ITriggerDefinitionProvider, TriggerDefinitionProvider>();

            if (!CheckIsExistSetupInServices<ISignalDefinitionProvider>(serviceCollection))
                AddService<ISignalDefinitionProvider, SignalDefinitionProvider>();

            SetupServices();

            if (!CheckIsExistSetupInServices<ILoggerProvider>(serviceCollection))
                AddService<ILoggerProvider>(NullLoggerProvider.Instance);

            if (!CheckIsExistSetupInServices<ILoggerFactory>(serviceCollection))
                AddService<ILoggerFactory>(NullLoggerFactory.Instance);

            var loggerFactory = serviceCollection.BuildServiceProvider()
                                                 .GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;

            var logger = loggerFactory.CreateLogger(GetType().Name);

            if (!this._blockAutoConfig)
                AutoConfigure(context.Configuration, logger);

            OnFinalizeManualBuildConfigure(logger);

            return OnBuild(logger);
        }

        #region Tools

        /// <summary>
        /// Automatics the configure to section not description programmaticaly.
        /// Use the assembly definition of <see cref="IAutoConfigurator"/> using <see cref="AutoConfiguratorAttribute"/>
        /// </summary>
        private void AutoConfigure(IConfiguration configuration, ILogger logger)
        {
            // Extensions configuration section is mainly used to dynamically load dll that are place side to the executable but not load by it
            var extensions = configuration.GetSection(ConfigurationSectionNames.ExtensionSection);
            if (extensions != null && extensions.GetChildren().Any())
            {
                var extensionsDll = extensions.GetChildren()
                                              .Select(x => x.Value)
                                              .Where(x => !string.IsNullOrWhiteSpace(x))
                                              .SelectMany(d => this._fileSystemHandler.SearchFiles(Environment.CurrentDirectory, "*" + d! + ".dll", false))
                                              .Distinct()
                                              .ToArray();

                foreach (var extension in extensionsDll)
                {
                    var stream = this._fileSystemHandler.OpenRead(extension);

                    if (stream != null)
                    {
                        using (stream)
                        {
                            var assembly = this._assemblyLoader.Load(stream);
                            this._assemblyInspector.RegisterAssemblyAndDependencies(assembly, true);
                        }
                    }
                }
            }

            var assemblies = this._assemblyInspector.SearchAssembliesWithAttribute<AutoConfiguratorKeyAttribute>();

            var indexedAssemblies = assemblies?.GroupBy(kv => kv.attribute?.Key ?? string.Empty)
                                               .ToDictionary(grp => grp.Key,
                                                             grp => (IReadOnlyDictionary<Type, Type>)grp.Select(g => g.assembly)
                                                                                                        .Distinct()
                                                                                                        .SelectMany(a => this._assemblyInspector.GetAssemblyAttributes<AutoConfiguratorAttribute>(a))
                                                                                                        .GroupBy(cfg => cfg.AutoConfigService)
                                                                                                        .ToDictionary(k => k.Key, v => v.Single().Implementation),
                                                             StringComparer.OrdinalIgnoreCase) ?? DictionaryHelper<string, IReadOnlyDictionary<Type, Type>>.ReadOnly;

            OnAutoConfigure(configuration, indexedAssemblies, logger);

            AutoConfigImpl<IMembershipsAutoConfigurator, IDemocriteClusterBuilder>(configuration,
                                                                                   indexedAssemblies,
                                                                                   s => s.Any(d => (d.ServiceType == typeof(IMembershipTable) && d.ImplementationType != null) ||
                                                                                                   (d.ServiceType == typeof(IGatewayListProvider) && d.ImplementationType != null)),
                                                                                   ConfigurationSectionNames.ClusterMembershipAutoConfigKey,
                                                                                   logger);

            AutoConfigImpl<IClusterEndpointAutoConfigurator, IDemocriteClusterBuilder>(configuration,
                                                                                       indexedAssemblies,
                                                                                       s => s.Any(d => (d.ServiceType == typeof(EndpointOptions) && d.ImplementationType != null)),
                                                                                       ConfigurationSectionNames.Endpoints,
                                                                                       logger);
        }

        /// <summary>
        /// Extend auto configuration <see cref="IAutoConfigurator"/>
        /// </summary>>
        protected virtual void OnAutoConfigure(IConfiguration configuration, IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies, ILogger logger)
        {
        }

        /// <summary>
        /// Automatics the configuration
        /// </summary>
        /// <exception cref="MissingRequiredDemocriteConfigurationException">Raised when no auto configuration have been setup for specific key.</exception>
        protected void AutoConfigImpl<TAutoConfig, TAutoWizard>(IConfiguration configuration,
                                                                IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies,
                                                                Func<IServiceCollection, bool> predicateConfigurationExist,
                                                                string configKey,
                                                                ILogger logger,
                                                                Action<TAutoConfig, TAutoWizard, IConfiguration, IServiceCollection, ILogger>? customConfig = null,
                                                                string? defaultAutoKey = ConfigurationSectionNames.DefaultAutoConfigKey)

            where TAutoConfig : IAutoConfigurator<TAutoWizard>
            where TAutoWizard : IBuilderDemocriteBaseWizard
        {
            var serviceCollection = GetServiceCollection();

            if (predicateConfigurationExist(serviceCollection))
                return;

            var autoConfigKey = configuration.GetValue<string?>(configKey);

            if (string.IsNullOrEmpty(autoConfigKey))
                autoConfigKey = defaultAutoKey;

            if (string.IsNullOrEmpty(autoConfigKey))
                autoConfigKey = ConfigurationSectionNames.DefaultAutoConfigKey;

            if (indexedAssemblies.TryGetValue(autoConfigKey, out var autoConfigurators) &&
                autoConfigurators.TryGetValue(typeof(TAutoConfig), out var autoConfigurator))
            {
                var configurator = (TAutoConfig)(Activator.CreateInstance(autoConfigurator) ?? throw new NotSupportedException());

                customConfig ??= (c, wizard, cfg, service, logger) => c.AutoConfigure(wizard, cfg, service, logger);

                customConfig(configurator, (TAutoWizard)(object)this, configuration, serviceCollection, logger);

                // Directly push data to predicateConfigurationExist function to work correctly
                SetupServices();
                return;
            }

            throw MissingRequiredAutoDemocriteConfigurationException.Create<TAutoConfig>(autoConfigKey);
        }

        /// <summary>
        /// Called to build democrite
        /// </summary>
        protected abstract TResultConfig OnBuild(ILogger logger);

        /// <summary>
        /// Called when as first step of the build process to allow implementation to add custom configuration before final build
        /// </summary>
        protected virtual void OnManualBuildConfigure()
        {

        }

        /// <summary>
        /// Called when at step of the build process to allow implementation to add custom configuration after auto build
        /// </summary>
        protected virtual void OnFinalizeManualBuildConfigure(ILogger logger)
        {

        }

        /// <summary>
        /// Gets the <typeparamref name="TWizard"/>.
        /// </summary>
        protected abstract TWizard GetWizard();

        /// <summary>
        /// Setups register services.
        /// </summary>
        private void SetupServices()
        {
            var serviceCollection = GetServiceCollection();
            if (this._options.Count > 0)
            {
                foreach (var opt in this._options)
                    opt.Invoke(serviceCollection);
            }

            foreach (var service in this._services)
                serviceCollection.Add(service);

            this._services.Clear();
            this._options.Clear();
        }

        /// <summary>
        /// Setup an option information
        /// </summary>
        protected void AddOption<TOptions>(Action<TOptions> optionsCfg) where TOptions : class
        {
            this._options.Enqueue((IServiceCollection s) => s.Configure(optionsCfg));
        }

        /// <summary>
        /// Checks the service <typeparamref name="TService"/> is in pending services.
        /// </summary>
        protected bool CheckIsExistInPendingServices<TService>()
        {
            return this._services.Any(s => s.ServiceType == typeof(TService));
        }

        /// <summary>
        /// Checks if the service  <typeparamref name="TService"/> have already been setup 
        /// in <paramref name="serviceCollection"/> of in the local pending services.
        /// </summary>
        protected bool CheckIsExistSetupInServices<TService>(IServiceCollection serviceCollection)
        {
            return CheckIsExistInPendingServices<TService>() || serviceCollection.Any(s => s.ServiceType == typeof(TService));
        }

        /// <summary>
        /// Gets the configuration builder.
        /// </summary>
        protected virtual TWizardConfig GetConfigurationBuilder()
        {
            if (this is TWizardConfig wizard)
                return wizard;

            throw new NotImplementedException("Plz override the method " + nameof(GetConfigurationBuilder) + " for type " + GetType());
        }

        #endregion

        #endregion
    }
}
