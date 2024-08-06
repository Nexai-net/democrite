// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Services
{
    //using Democrite.Framework.Core.Abstractions.Repositories;
    //using Democrite.Framework.Core.Abstractions.Storages;
    //using Democrite.Framework.Extensions.Mongo.Repositories;
    //using Democrite.Framework.Node.Abstractions.Repositories;

    //using Microsoft.Extensions.DependencyInjection;

    //using System;
    //using System.Linq;

    ///// <summary>
    ///// Factory used to create <see cref="IReadOnlyRepository"/>
    ///// </summary>
    ///// <seealso cref="DefaultSpecificRepositoryBaseFactory" />
    ///// <seealso cref="IRepositorySpecificFactory" />
    //public sealed class MongoStorageSpecificRepositoryFactory : IRepositorySpecificFactory
    //{
    //    //#region Fields

    //    //private static readonly Type s_monoROWithEntityIdRepo;
    //    //private static readonly Type s_monoRORepo;
    //    //private static readonly Type s_monoRepo;

    //    //#endregion

    //    //#region Ctor

    //    ///// <summary>
    //    ///// Initializes the <see cref="MongoStorageSpecificRepositoryFactory"/> class.
    //    ///// </summary>
    //    //static MongoStorageSpecificRepositoryFactory()
    //    //{
    //    //    s_monoRORepo = typeof(MongoStorageReadOnlyRepository<>);
    //    //    s_monoROWithEntityIdRepo = typeof(MongoStorageReadOnlyRepository<,>);
    //    //    s_monoRepo = typeof(MongoStorageRepository<,>);
    //    //}

    //    ///// <summary>
    //    ///// Initializes a new instance of the <see cref="MongoStorageSpecificRepositoryFactory"/> class.
    //    ///// </summary>
    //    //public MongoStorageSpecificRepositoryFactory(string StorageName)
    //    //    : base(s_monoRORepo, s_monoROWithEntityIdRepo, s_monoRepo, s_monoRepo, StorageName, true)
    //    //{
    //    //}

    //    //#endregion

    //    //#region Methods

    //    ///// <inheritdoc />
    //    //protected override object InstanciateRepository(IServiceProvider serviceProvider,
    //    //                                                string stateName,
    //    //                                                Type serviceTargetTrait,
    //    //                                                Type repositoryType,
    //    //                                                Type repositoryWithEntityIdType)
    //    //{
    //    //    var instType = repositoryType;

    //    //    var genericParams = serviceTargetTrait.GetGenericArguments();

    //    //    var entityType = genericParams.First();
    //    //    var entityIdInterface = entityType.GetTypeInfoExtension()
    //    //                                      .GetAllCompatibleTypes()
    //    //                                      .FirstOrDefault(i => i.IsInterface &&
    //    //                                                           i.IsGenericType &&
    //    //                                                           i.GetGenericTypeDefinition() == typeof(IEntityWithId<>));

    //    //    if (entityIdInterface is null)
    //    //        throw new InvalidCastException($"To get a repository on {entityType} with write access the entity MUST inherite from IEntityWithId<>");

    //    //    if (genericParams.Length == 1 && instType == s_monoRepo)
    //    //        genericParams = genericParams.Concat(entityIdInterface.GetGenericArguments()).ToArray();

    //    //    instType = instType.MakeGenericType(genericParams);

    //    //    return ActivatorUtilities.CreateInstance(serviceProvider, instType, stateName);
    //    //}

    //    // #endregion

    //    /// <inheritdoc />
    //    public IReadOnlyRepository<TEntity, TEntityId> Get<TEntity, TEntityId>(IServiceProvider serviceProvider, string StorageName, string configurationName, bool readOnly)
    //        where TEntity : IEntityWithId<TEntityId>
    //        where TEntityId : IEquatable<TEntityId>
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
