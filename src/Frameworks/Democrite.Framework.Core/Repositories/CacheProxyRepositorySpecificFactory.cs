// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Repositories
{
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;

    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Providers;

    using System;

    /// <summary>
    /// Base class of <see cref="IRepositoryFactory"/>
    /// </summary>
    /// <seealso cref="IRepositoryFactory" />
    public sealed class CacheProxyRepositorySpecificFactory<TSpecificFactory> : IRepositorySpecificFactory
        where TSpecificFactory : IRepositorySpecificFactory
    {
        #region Fields

        // StoreName, stateName, expectedType
        private readonly Dictionary<string, Dictionary<Type, object>> _cachedRepo;
        private readonly ReaderWriterLockSlim _cacheLocker;

        private TSpecificFactory? _factory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepositorySpecificFactory"/> class.
        /// </summary>
        public CacheProxyRepositorySpecificFactory(IServiceProvider serviceProvider)
        {
            this._cacheLocker = new ReaderWriterLockSlim();
            this._cachedRepo = new Dictionary<string, Dictionary<Type, object>>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IReadOnlyRepository<TEntity, TEntityId> Get<TTargetRepo, TEntity, TEntityId>(IServiceProvider serviceProvider, RepositoryGetOptions request)
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            var storageName = request.StorageName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;
            var configurationName = request.ConfigurationName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;

            var fullConfigKey = configurationName + "##" + storageName + "##" + (typeof(TTargetRepo).GetTypeInfoExtension().FullShortName ?? typeof(TTargetRepo).Name);
            var entityTraits = typeof(TEntity);

            this._cacheLocker.EnterReadLock();
            try
            {
                if (this._cachedRepo.TryGetValue(fullConfigKey, out var repoByType) &&
                    repoByType.TryGetValue(entityTraits, out var cacheRepository))
                {
                    return (IReadOnlyRepository<TEntity, TEntityId>)cacheRepository;
                }
            }
            finally
            {
                this._cacheLocker.ExitReadLock();
            }

            this._cacheLocker.EnterWriteLock();
            try
            {
                if (this._cachedRepo.TryGetValue(fullConfigKey, out var repoByType) &&
                    repoByType.TryGetValue(entityTraits, out var cacheRepository))
                {
                    return (IReadOnlyRepository<TEntity, TEntityId>)cacheRepository;
                }

                this._factory ??= ActivatorUtilities.CreateInstance<TSpecificFactory>(serviceProvider);

                var service = this._factory.Get<TTargetRepo, TEntity, TEntityId>(serviceProvider, request);

                Dictionary<Type, object>? cacheByType;
                if (!this._cachedRepo.TryGetValue(fullConfigKey, out cacheByType))
                {
                    cacheByType = new Dictionary<Type, object>();
                    this._cachedRepo.Add(fullConfigKey, cacheByType);
                }

                cacheByType[entityTraits] = service;
                return (IReadOnlyRepository<TEntity, TEntityId>)service;
            }
            finally
            {
                this._cacheLocker.ExitWriteLock();
            }
        }

        #endregion
    }
}
