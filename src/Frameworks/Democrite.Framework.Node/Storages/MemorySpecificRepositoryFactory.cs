// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Node.Abstractions.Repositories;

    using Microsoft.Extensions.DependencyInjection;

    using System;

    /// <summary>
    /// Factory used to produce repository based on cluster memory storage
    /// </summary>
    /// <seealso cref="IRepositorySpecificFactory" />
    public sealed class MemorySpecificRepositoryFactory : DefaultSpecificRepositoryBaseFactory, IRepositorySpecificFactory
    {

        #region Fields

        private static readonly Type s_monoWithEntityIdRepo;

        private static readonly Type s_monoRORepo;
        private static readonly Type s_monoROWithEntityIdRepo;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MemoryRepositoryFactory"/> class.
        /// </summary>
        static MemorySpecificRepositoryFactory()
        {
            s_monoWithEntityIdRepo = typeof(MemoryRepository<,>);

            s_monoRORepo = typeof(MemoryReadOnlyRepository<,>);
            s_monoROWithEntityIdRepo = typeof(MemoryReadOnlyRepository<,,>);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepositoryFactory"/> class.
        /// </summary>
        public MemorySpecificRepositoryFactory(string storageName)
            : base(s_monoRORepo, s_monoROWithEntityIdRepo, s_monoWithEntityIdRepo, s_monoWithEntityIdRepo, storageName, true)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override object InstanciateRepository(IServiceProvider serviceProvider, string stateName, Type serviceTargetTrait, Type repositoryType, Type repositoryWithEntityIdType)
        {
            var instType = repositoryType;

            var genericParams = serviceTargetTrait.GetGenericArguments();

            var entityType = genericParams.First();
            var entityIdInterface = entityType.GetTypeInfoExtension()
                                              .GetAllCompatibleTypes()
                                              .FirstOrDefault(i => i.IsInterface &&
                                                                   i.IsGenericType &&
                                                                   i.GetGenericTypeDefinition() == typeof(IEntityWithId<>));

            if (entityIdInterface is null)
                throw new InvalidCastException($"To get a repository on {entityType} with write access the entity MUST inherite from IEntityWithId<>");

            bool addRegistry = (genericParams.Length == 1 && repositoryType == s_monoRORepo) || (genericParams.Length == 2 && repositoryWithEntityIdType == s_monoROWithEntityIdRepo);
            if (genericParams.Length == 2 && repositoryWithEntityIdType == s_monoROWithEntityIdRepo)
            {
                instType = repositoryWithEntityIdType;
            }
            // Try get IRepository<TEntity> we need to search the EntityId Source to be able to construct the MemoryRepository correctly that need this entityId
            else if (genericParams.Length == 1 && repositoryType == s_monoWithEntityIdRepo)
            {
                genericParams = genericParams.Concat(entityIdInterface.GetGenericArguments()).ToArray();
            }

            if (addRegistry)
                genericParams = genericParams.Append(typeof(IMemoryStorageRepositoryRegistryGrain<>).MakeGenericType(entityIdInterface.GetGenericArguments())).ToArray();

            instType = instType.MakeGenericType(genericParams);

            return ActivatorUtilities.CreateInstance(serviceProvider, instType, stateName, this.StorageName);
        }

        #endregion
    }
}
