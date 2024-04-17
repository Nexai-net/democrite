// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Node.Abstractions.Administrations;
    using Democrite.Framework.Node.Abstractions.Services;

    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.Supports;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class GrainRouteSiloRootService : GrainRouteBaseService, IVGrainRouteService, IInitService
    {
        #region Fields

        private readonly SupportInitializationImplementation<IServiceProvider> _initImpl;
        private readonly IVGrainDemocriteSystemProvider _democriteSystemProvider;
        private readonly ILogger<IVGrainRouteService> _vgrainRouteServiceLogger;
        private readonly ReaderWriterLockSlim _cacheRedirectionLocker;

        private IReadOnlyDictionary<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>> _redirectionDefinitions;
        private string? _localRegistryEtag;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainRouteSiloRootService"/> class.
        /// </summary>
        public GrainRouteSiloRootService(IVGrainDemocriteSystemProvider democriteSystemProvider,
                                         ILogger<IVGrainRouteService> vgrainRouteService)
            : base(null) // This service MUST be the ROOT level by silo
        {
            this._initImpl = new SupportInitializationImplementation<IServiceProvider>((_, t) => new ValueTask(UpdateGlobalRedirectionAsync(string.Empty, t)));

            this._cacheRedirectionLocker = new ReaderWriterLockSlim();
            this._democriteSystemProvider = democriteSystemProvider;
            this._vgrainRouteServiceLogger = vgrainRouteService;

            this._redirectionDefinitions = DictionaryHelper<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>>.ReadOnly;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool ExpectOrleanStarted
        {
            get { return true; }
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
        protected override IReadOnlyDictionary<ConcretType, IReadOnlyCollection<Tuple<VGrainRedirectionDefinition, Func<Type, object?, IExecutionContext?, string?, bool>?>>> GetRedirections()
        {
            this._cacheRedirectionLocker.EnterReadLock();
            try
            {
                return this._redirectionDefinitions;
            }
            finally
            {
                this._cacheRedirectionLocker.ExitReadLock();
            }
        }

        /// <summary>
        /// Updates the global redirection asynchronous.
        /// </summary>
        internal async Task UpdateGlobalRedirectionAsync(string etag, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(etag) && this._localRegistryEtag == etag)
                return;

            var registry = await this._democriteSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(null, this._vgrainRouteServiceLogger);

            using (var grainCancellation = cancellationToken.ToGrainCancellationTokenSource())
            {
                var redirections = await registry.GetGlobalRedirection(this._localRegistryEtag ?? string.Empty, grainCancellation.Token);

                if (redirections is not null)
                {
                    this._localRegistryEtag = redirections.Value.Etag;

                    this._cacheRedirectionLocker.EnterWriteLock();
                    try
                    {
                        this._redirectionDefinitions = GrainRouteBaseService.BuildRedirections(redirections.Value.Info);
                        this.ClearLocalRouteCache();
                    }
                    finally
                    {
                        this._cacheRedirectionLocker.ExitWriteLock();
                    }
                }
            }
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(IServiceProvider? initializationState, CancellationToken token = default)
        {
            return this._initImpl.InitializationAsync(initializationState, token);
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return this._initImpl.InitializationAsync(token);
        }

        #endregion
    }
}
