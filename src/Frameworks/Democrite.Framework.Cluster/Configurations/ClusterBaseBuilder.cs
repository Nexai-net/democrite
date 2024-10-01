// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Attributes;
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Cluster.Abstractions.Exceptions;
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Cluster.Services;
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Core.Extensions;
    using Democrite.Framework.Core.Repositories;
    using Democrite.Framework.Core.Services;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Core.Triggers;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Loggers;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    using Orleans;
    using Orleans.Configuration;
    using Orleans.Messaging;
    using Orleans.Serialization;
    using Orleans.Serialization.Cloning;
    using Orleans.Serialization.Serializers;

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

        private readonly DemocriteClusterNodeInfo _clusterInfo;

        private readonly IAssemblyInspector _assemblyInspector;
        private readonly IFileSystemHandler _fileSystemHandler;
        private readonly INetworkInspector _networkInspector;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IHashService _hashService;

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
            this._hashService = clusterBuilderTools.HashService;

            this._context = context;

            this.IsClient = isClient;
            this.IsServerNode = !isClient;

            this._options = new Queue<Action<IServiceCollection>>();
            this._services = new Queue<ServiceDescriptor>();

            this.MemoryLogger = new InMemoryLogger(new LoggerFilterOptions() { MinLevel = LogLevel.Trace }.ToMonitorOption());

            this._clusterInfo = new DemocriteClusterNodeInfo();
            AddService<IDemocriteClusterNodeInfo, DemocriteClusterNodeInfo>();
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
        public TWizard ConfigureServices(Action<IServiceCollection, IConfiguration> config)
        {
            config(GetServiceCollection(), GetConfiguration());
            return GetWizard();
        }

        /// <inheritdoc />
        public virtual IDemocriteWizardStart<TWizard, TWizardConfig> WizardConfig()
        {
            return this;
        }

        /// <inheritdoc />
        public abstract TWizard NoCluster(string? serviceId = "dev", string? clusterId = "dev", bool useLoopback = true);

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

        /// <inheritdoc />
        public IDemocriteClusterBuilder CustomizeClusterId(string serviceId, string clusterId)
        {
            ConfigureServices(s =>
            {
                s.Configure<ClusterOptions>(options =>
                {
                    options.ServiceId = serviceId;
                    options.ClusterId = clusterId;
                });

                s.PostConfigure<ClusterOptions>(options =>
                {
                    if (string.IsNullOrWhiteSpace(options.ClusterId))
                        options.ClusterId = ClusterOptions.DefaultClusterId;

                    if (string.IsNullOrWhiteSpace(options.ServiceId))
                        options.ServiceId = ClusterOptions.DefaultServiceId;
                });
            });

            return this;
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
        public TWizardConfig AddService<TService>(Func<IServiceProvider, TService> factory)
            where TService : class
        {
            ArgumentNullException.ThrowIfNull(factory);

            // Attention Do not register the concret type
            this._services.Enqueue(ServiceDescriptor.Singleton<TService>(factory));

            return GetConfigurationBuilder();
        }

        /// <inheritdoc />
        public TWizardConfig AddService(Type service, Type instanceType)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(instanceType);

            this._services.Enqueue(ServiceDescriptor.Singleton(service, instanceType));

            return GetConfigurationBuilder();
        }

        public TWizardConfig AddService(Type service, object instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

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

        public void AddExtensionOption<TOptions, TKey>(TOptions instance, TKey key) where TOptions : class
        {
            if (instance is not null)
            {
                var services = GetServiceCollection();
                services.AddKeyedSingleton<IOptions<TOptions>>(key, instance.ToOption());
                services.AddKeyedSingleton<IOptionsMonitor<TOptions>>(key, instance.ToMonitorOption());
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
        public void AddExtensionOption<TOption>(Action<TOption> optionsCfg, string name) where TOption : class
        {
            AddOption<TOption>(name, optionsCfg);
        }

        /// <inheritdoc />
        public TOption AddExtensionOption<TOption>(string configurationSection, TOption fallbackOption)
            where TOption : class
        {
            return AddExtensionOption<TOption, NoneType>(configurationSection, fallbackOption, NoneType.Instance);
        }

        /// <inheritdoc />
        public TOption AddExtensionOption<TOption, TKey>(string configurationSection, TOption fallbackOption, TKey key)
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

            if (NoneType.IsEqualTo<TKey>())
                AddExtensionOption(fallbackOption);
            else
                AddExtensionOption(fallbackOption, key);
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
            AddService<IHashService>(this._hashService);

            serviceCollection.SetupCoreServices();

            serviceCollection.TryAddSingleton<IDemocriteSerializer, DemocriteSerializer>();

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

            serviceCollection.AddSingleton<DeferredClientObserver>()
                             .AddSingleton<IDeferredAwaiterHandler>(p => p.GetRequiredService<DeferredClientObserver>());

            var loggerFactory = serviceCollection.BuildServiceProvider()
                                                 .GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;

            var logger = loggerFactory.CreateLogger(GetType().Name);

            if (!this._blockAutoConfig)
                AutoConfigure(context.Configuration, logger);

            OnFinalizeManualBuildConfigure(logger);

            // Get Init and Finalize services
            RegisterNodeService<IInitService>(serviceCollection);
            RegisterNodeService<IFinalizeService>(serviceCollection);

            return OnBuild(logger);
        }

        /// <summary>
        /// Adds the custom serializer must inherite from orleans interfaces <see cref="IGeneralizedCodec"/>, <see cref="IGeneralizedCopier"/>, <see cref="ITypeFilter"/>
        /// </summary>
        public TWizard AddCustomSerializer<TInterfaceSerializer, TSerializerImplementation>()
            where TInterfaceSerializer : class, IGeneralizedCodec, IGeneralizedCopier, ITypeFilter
            where TSerializerImplementation : class, TInterfaceSerializer
        {

            var serviceCollection = GetServiceCollection();

            serviceCollection.AddSingleton<TInterfaceSerializer, TSerializerImplementation>();
            serviceCollection.AddSingleton<IGeneralizedCodec>(s => s.GetRequiredService<TInterfaceSerializer>());
            serviceCollection.AddSingleton<IGeneralizedCopier>(s => s.GetRequiredService<TInterfaceSerializer>());
            serviceCollection.AddSingleton<ITypeFilter>(s => s.GetRequiredService<TInterfaceSerializer>());

            return GetWizard();
        }

        #region Tools

        /// <summary>
        /// Registers in the <paramref name="serviceCollection"/> the related <typeparamref name="TService"/>
        /// </summary>
        private static void RegisterNodeService<TService>(IServiceCollection serviceCollection)
        {
            var serviceTrait = typeof(TService);

            // Get Concret type that inherite from TService
            var servicesToSetup = serviceCollection.Where(t => t.ServiceType.IsAssignableTo(serviceTrait) &&
                                                               (t.Lifetime == ServiceLifetime.Singleton || t.ImplementationInstance != null))
                                               .Distinct()
                                               .GroupBy(k => k.ServiceType)
                                               .ToArray();

#pragma warning disable CS8603 // Possible null reference return.

            foreach (var init in servicesToSetup)
            {
                if (init.Count() == 1)
                {
                    serviceCollection.Add(new ServiceDescriptor(serviceTrait,
                                                                provider => provider.GetService(init.Key),
                                                                ServiceLifetime.Singleton));

                    continue;
                }

                serviceCollection.Add(new ServiceDescriptor(serviceTrait,
                                                            p => new NodeServiceProxy(p, init.Key),
                                                            ServiceLifetime.Singleton));
            }

#pragma warning restore CS8603 // Possible null reference return.

        }

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
                                              .SelectMany(d => this._fileSystemHandler.SearchFiles(Environment.CurrentDirectory, d! + ".dll", false))
                                              .Distinct()
                                              .ToArray();
                
                logger.OptiLog(LogLevel.Information, "[Extensions] - Extensions to load - {extensions}", string.Join(Environment.NewLine, extensions));

                foreach (var extension in extensionsDll)
                {
                    var stream = this._fileSystemHandler.OpenRead(extension);

                    if (stream != null)
                    {
                        using (stream)
                        {
                            var assembly = this._assemblyLoader.Load(stream);
                            this._assemblyInspector.RegisterAssemblyAndDependencies(assembly, true);
                            logger.OptiLog(LogLevel.Information, "[Extensions] - Loaded - {extension}", Path.GetFileName(extension.OriginalString));
                        }
                    }
                }
            }

            var assemblies = this._clusterInfo.CurrentLoadedAssemblies; //this._assemblyInspector.SearchAssembliesWithAttribute<AutoConfiguratorKeyAttribute>();

            var indexedAssemblies = assemblies?.Select(a => (Assembly: a, Attr: this._assemblyInspector.GetAssemblyAttributes<AutoConfiguratorKeyAttribute>(a).FirstOrDefault()))
                                               .Where(a => a.Attr is not null)
                                               .GroupBy(kv => kv.Attr?.Key ?? string.Empty)
                                               .ToDictionary(grp => grp.Key,
                                                             grp => (IReadOnlyDictionary<Type, Type>)grp.Select(g => g.Assembly)
                                                                                                        .Distinct()
                                                                                                        .SelectMany(a => this._assemblyInspector.GetAssemblyAttributes<AutoConfiguratorAttribute>(a))
                                                                                                        .GroupBy(cfg => cfg.AutoConfigService)
                                                                                                        .ToDictionary(k => k.Key, v => v.Single().Implementation),
                                                             StringComparer.OrdinalIgnoreCase) ?? DictionaryHelper<string, IReadOnlyDictionary<Type, Type>>.ReadOnly;

            OnAutoConfigure(configuration, indexedAssemblies, logger);

            AutoConfigImpl<IMembershipsAutoConfigurator, IDemocriteClusterBuilder>(configuration,
                                                                                   "ClusterMembership",
                                                                                   indexedAssemblies,
                                                                                   s => s.Any(d => (d.ServiceType == typeof(IMembershipTable) && d.ImplementationType != null) ||
                                                                                                   (d.ServiceType == typeof(IGatewayListProvider) && d.ImplementationType != null)),
                                                                                   ConfigurationSectionNames.ClusterMembershipAutoConfigKey,
                                                                                   logger);
        }

        /// <summary>
        /// Extend auto configuration <see cref="IAutoConfigurator"/>
        /// </summary>>
        protected virtual void OnAutoConfigure(IConfiguration configuration,
                                               IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies,
                                               ILogger logger)
        {
        }

        /// <summary>
        /// Automatics the configuration
        /// </summary>
        /// <exception cref="MissingRequiredDemocriteConfigurationException">Raised when no auto configuration have been setup for specific key.</exception>
        protected void AutoConfigImpl<TAutoConfig, TAutoWizard>(IConfiguration configuration,
                                                                string logActionName,
                                                                IReadOnlyDictionary<string, IReadOnlyDictionary<Type, Type>> indexedAssemblies,
                                                                Func<IServiceCollection, bool> predicateConfigurationExist,
                                                                string configFullKey,
                                                                ILogger logger,
                                                                Action<TAutoConfig, TAutoWizard, IConfiguration, IServiceCollection, ILogger>? customConfig = null,
                                                                string? defaultAutoKey = ConfigurationSectionNames.DefaultAutoConfigKey,
                                                                string? key = null)

            where TAutoConfig : IAutoConfigurator
            where TAutoWizard : IBuilderDemocriteBaseWizard
        {
            const string AUTO_KEY_CFG_KEY = ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

            var serviceCollection = GetServiceCollection();

            if (predicateConfigurationExist(serviceCollection))
            {
                logger.OptiLog(LogLevel.Information, "[{ConfigAction}][Key:{key}] - Already Configured", logActionName, key);
                return;
            }

            var autoConfigKey = configuration.GetValue<string?>(configFullKey);

            if (string.IsNullOrEmpty(autoConfigKey))
                autoConfigKey = defaultAutoKey;

            if (string.IsNullOrEmpty(autoConfigKey))
                autoConfigKey = ConfigurationSectionNames.DefaultAutoConfigKey;

            if (indexedAssemblies.TryGetValue(autoConfigKey, out var autoConfigurators) &&
                autoConfigurators.TryGetValue(typeof(TAutoConfig), out var autoConfigurator))
            {
                logger.OptiLog(LogLevel.Information, "[{ConfigAction}][Key:{key}] - {autokey} - Auto-Configuration by {configurator}", logActionName, key, autoConfigKey, autoConfigurator.Name);

                var configurator = (TAutoConfig)(Activator.CreateInstance(autoConfigurator) ?? throw new NotSupportedException());

                if (customConfig is null)
                {
                    if (configFullKey.EndsWith(AUTO_KEY_CFG_KEY))
                        configFullKey = configFullKey.Substring(0, configFullKey.Length - AUTO_KEY_CFG_KEY.Length);

                    if (configurator is IAutoConfigurator<TAutoWizard>)
                        customConfig = (c, wizard, cfg, service, logger) => ((IAutoConfigurator<TAutoWizard>)c!).AutoConfigure(wizard, cfg, service, logger);
                    else if (configurator is IAutoKeyConfigurator<TAutoWizard>)
                        customConfig = (c, wizard, cfg, service, logger) => ((IAutoKeyConfigurator<TAutoWizard>)c!).AutoConfigure(wizard, cfg, service, logger, configFullKey, key!);
                }

                if (customConfig is null)
                    throw new InvalidOperationException("Auto config or custom config is not setups");

                logger.OptiLog(LogLevel.Trace, "[{ConfigAction}][Key:{key}] - Configuring ...", logActionName, key);

                customConfig(configurator, (TAutoWizard)(object)this, configuration, serviceCollection, logger);

                logger.OptiLog(LogLevel.Trace, "[{ConfigAction}][Key:{key}] - Configured", logActionName, key);

                // Directly push data to predicateConfigurationExist function to work correctly
                SetupServices();
                return;
            }

            logger.OptiLog(LogLevel.Information, "[{ConfigAction}][Key:{key}] - {autokey} - Missing", logActionName, key, autoConfigKey);
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
        protected void AddOption<TOptions>(Action<TOptions> optionsCfg)
            where TOptions : class
        {
            this._options.Enqueue((IServiceCollection s) => s.Configure(optionsCfg));
        }

        /// <summary>
        /// Setup an option information
        /// </summary>
        protected void AddOption<TOptions>(string name, Action<TOptions> optionsCfg)
            where TOptions : class
        {
            this._options.Enqueue((IServiceCollection s) => s.Configure(name, optionsCfg));
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
