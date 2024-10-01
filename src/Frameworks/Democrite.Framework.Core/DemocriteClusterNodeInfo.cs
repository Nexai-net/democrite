// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;

    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Supports;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using Orleans.Configuration;
    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provide all information related to current node (SILO) in the democrite cluster
    /// </summary>
    /// <seealso cref="IDemocriteClusterNodeInfo" />
    internal sealed class DemocriteClusterNodeInfo : IDemocriteClusterNodeInfo, ISupportInitialization<IServiceProvider>
    {
        #region Fields

        private readonly SupportInitializationImplementation<IServiceProvider> _initImpl;
        private static ImmutableHashSet<Assembly> s_assemblies;
        private static readonly ReaderWriterLockSlim s_locker;

        private IClusterManifestProvider? _manifestCluster;
        private IOptions<ClusterOptions>? _clusterOptions;
        private IOptions<SiloOptions>? _siloOptions;
        private Silo? _currentSilo;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DemocriteClusterNodeInfo"/> class.
        /// </summary>
        static DemocriteClusterNodeInfo()
        {
            s_locker = new ReaderWriterLockSlim();
            s_assemblies = Assembly.GetExecutingAssembly().AsEnumerable()
                                   .Append(Assembly.GetEntryAssembly())
                                   .Append(Assembly.GetCallingAssembly())
                                   .Append(Assembly.GetExecutingAssembly())
                                   .NotNull()
                                   .SelectMany(a => a.GetReferencedAssemblies()
                                                     .Select(r => Assembly.Load(r))
                                                     .Append(a))
                                   .Distinct()
                                   .ToImmutableHashSet();

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteClusterNodeInfo"/> class.
        /// </summary>
        public DemocriteClusterNodeInfo()
        {
            this._initImpl = new SupportInitializationImplementation<IServiceProvider>(OnInitializationAsync);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public SiloAddress CurrentAddress
        {
            get { return this._currentSilo?.SiloAddress ?? throw new InvalidDataException("NEED initialization first"); }
        }

        /// <inheritdoc />
        public string ClusterName
        {
            get { return this._clusterOptions?.Value?.ClusterId ?? throw new InvalidDataException("NEED initialization first"); }
        }

        /// <inheritdoc />
        public string NodeName
        {
            get { return this._siloOptions?.Value?.SiloName ?? throw new InvalidDataException("NEED initialization first"); }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Assembly> CurrentLoadedAssemblies
        {
            get
            {
                s_locker.EnterReadLock();
                try
                {
                    return s_assemblies;
                }
                finally
                {
                    s_locker.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return this._initImpl.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return this._initImpl.IsInitialized; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask InitializationAsync(IServiceProvider? initializationState, CancellationToken token = default)
        {
            return this._initImpl.InitializationAsync(initializationState, token);
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            throw new NotSupportedException("Required IServiceProvider");
        }

        #region Tools

        /// <inheritdoc  cref="ISupportInitialization{IServiceProvider}.InitializationAsync(IServiceProvider?, CancellationToken)" />
        private ValueTask OnInitializationAsync(IServiceProvider? provider, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(provider);

            this._manifestCluster = provider.GetRequiredService<IClusterManifestProvider>();
            this._currentSilo = provider.GetRequiredService<Silo>();
            this._clusterOptions = provider.GetRequiredService<IOptions<ClusterOptions>>();
            this._siloOptions = provider.GetRequiredService<IOptions<SiloOptions>>();

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Currents the domain assembly load.
        /// </summary>
        private static void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
            s_locker.EnterWriteLock();
            try
            {
                if (args.LoadedAssembly is not null)
                    s_assemblies = s_assemblies.Add(args.LoadedAssembly);
            }
            finally
            {
                s_locker.ExitWriteLock();
            }
        }

        #endregion

        #endregion
    }
}
