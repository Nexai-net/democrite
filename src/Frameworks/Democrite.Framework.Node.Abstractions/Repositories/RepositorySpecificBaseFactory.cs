// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Repositories
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;

    using System;

    /// <summary>
    /// Base class of <see cref="IRepositoryFactory"/>
    /// </summary>
    /// <seealso cref="IRepositoryFactory" />
    public abstract class RepositorySpecificBaseFactory : IRepositorySpecificFactory
    {
        #region Fields

        // StoreName, stateName, expectedType
        private static readonly Dictionary<string, Dictionary<Type, object>> s_cachedRepo;
        private static readonly ReaderWriterLockSlim s_cacheLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="RepositoryBaseFactory"/> class.
        /// </summary>
        static RepositorySpecificBaseFactory()
        {
            s_cacheLocker = new ReaderWriterLockSlim();
            s_cachedRepo = new Dictionary<string, Dictionary<Type, object>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositorySpecificBaseFactory"/> class.
        /// </summary>
        protected RepositorySpecificBaseFactory(string storageName)
        {
            this.StorageName = storageName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the storage.
        /// </summary>
        public string StorageName { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public TTargetRepo Get<TTargetRepo, TEntity>(IServiceProvider serviceProvider, string stateName)
            where TTargetRepo : IReadOnlyRepository<TEntity>
        {
            var targetServiceType = FactoryRequestType<TTargetRepo, TEntity>();

            s_cacheLocker.EnterReadLock();
            try
            {
                if (s_cachedRepo.TryGetValue(stateName, out var repoByType) &&
                    repoByType.TryGetValue(targetServiceType, out var cacheRepository))
                {
                    return (TTargetRepo)cacheRepository;
                }
            }
            finally
            {
                s_cacheLocker.ExitReadLock();
            }

            s_cacheLocker.EnterWriteLock();
            try
            {
                var service = OnCreate<TTargetRepo, TEntity>(serviceProvider, stateName, targetServiceType);

                Dictionary<Type, object>? cacheByType = null;
                if (!s_cachedRepo.TryGetValue(stateName, out cacheByType))
                {
                    cacheByType = new Dictionary<Type, object>();
                    s_cachedRepo.Add(stateName, cacheByType);
                }

                cacheByType[targetServiceType] = service;
                return (TTargetRepo)service;
            }
            finally
            {
                s_cacheLocker.ExitWriteLock();
            }
        }

        /// <remarks>
        ///     Result cached
        /// </remarks>
        /// <inheritdoc cref="IRepositoryFactory.Get{TTargetRepo, TEntity}(IServiceProvider, string, string)"/>
        protected abstract object OnCreate<TTargetRepo, TEntity>(IServiceProvider serviceProvider,
                                                                 string stateName,
                                                                 Type serviceTargetTrait)
            where TTargetRepo : IReadOnlyRepository<TEntity>;

        /// <summary>
        /// Check is target type is managed
        /// </summary>
        protected Type FactoryRequestType<TTargetRepo, TEntity>()
        {
            var requestTraits = typeof(TTargetRepo);
            var repoROTraits = typeof(IReadOnlyRepository<TEntity>);
            var repositoryTraits = typeof(IRepository<TEntity>);

            if (requestTraits == repoROTraits ||
                (requestTraits.IsAssignableTo(repoROTraits) && requestTraits.GetGenericTypeDefinition() == typeof(IReadOnlyRepository<IEntityWithId<int>, int>).GetGenericTypeDefinition()) ||
                (requestTraits == repositoryTraits) ||
                (requestTraits.IsAssignableTo(repositoryTraits) && requestTraits.GetGenericTypeDefinition() == typeof(IRepository<IEntityWithId<int>, int>).GetGenericTypeDefinition()))
            {
                return requestTraits;
            }

            throw new InvalidCastException("No support is managed for type " + requestTraits);
        }

        #endregion
    }
}
