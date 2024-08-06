// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Repositories
{
    //using Democrite.Framework.Core.Abstractions.Exceptions;
    //using Democrite.Framework.Core.Abstractions.Repositories;
    //using Democrite.Framework.Core.Abstractions.Storages;

    //using Microsoft.Extensions.DependencyInjection;

    //using System;

    ///// <summary>
    ///// Base class of <see cref="IRepositoryFactory"/>
    ///// </summary>
    ///// <seealso cref="IRepositoryFactory" />
    //public abstract class DefaultSpecificRepositoryBaseFactory : GenericRepositorySpecificFactory, IRepositorySpecificFactory
    //{

    //    #region Fields

    //    private readonly Type _readOnlyRepositoryGenericWithEntityIdType;
    //    private readonly Type _readOnlyRepositoryGenericType;
    //    private readonly Type _repositoryGenericWithEntityIdType;
    //    private readonly Type _repositoryGenericType;
    //    private readonly bool _allowWrite;

    //    #endregion

    //    #region Ctor

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="DefaultSpecificRepositoryBaseFactory"/> class.
    //    /// </summary>
    //    protected DefaultSpecificRepositoryBaseFactory(Type readOnlyRepositoryGenericType,
    //                                                   Type readOnlyRepositoryGenericWithEntityIdType,
    //                                                   Type repositoryGenericType,
    //                                                   Type repositoryGenericWithEntityIdType,
    //                                                   string storageName,
    //                                                   bool allowWrite)
    //        : base(storageName)
    //    {
    //        this._allowWrite = allowWrite;

    //        this._readOnlyRepositoryGenericType = readOnlyRepositoryGenericType;
    //        this._readOnlyRepositoryGenericWithEntityIdType = readOnlyRepositoryGenericWithEntityIdType;

    //        this._repositoryGenericType = repositoryGenericType;
    //        this._repositoryGenericWithEntityIdType = repositoryGenericWithEntityIdType;
    //    }

    //    #endregion

    //    #region Methods

    //    /// <inheritdoc />
    //    protected override object OnCreate<TTargetRepo, TEntity>(IServiceProvider serviceProvider,
    //                                                             string stateName,
    //                                                             Type serviceTargetTrait)
    //    {
    //        var requestWrite = serviceTargetTrait.IsAssignableTo(typeof(IRepository<TEntity>));

    //        if (requestWrite && !this._allowWrite)
    //            throw new DemocriteSecurityIssueException($"Write request is not allow for thos repository configuration stateName:{stateName} storageName:{this.StorageName}", "Repository_Write_Request");

    //        var repository = InstanciateRepository(serviceProvider,
    //                                               stateName,
    //                                               serviceTargetTrait,
    //                                               requestWrite ? this._repositoryGenericType : this._readOnlyRepositoryGenericType,
    //                                               requestWrite ? this._repositoryGenericWithEntityIdType : this._readOnlyRepositoryGenericWithEntityIdType);

    //        if (repository is not null)
    //            return repository;

    //        throw new NotSupportedException("Could not create a repository for type " + serviceTargetTrait);
    //    }

    //    /// <summary>
    //    /// Instanciates the repository.
    //    /// </summary>
    //    protected virtual object InstanciateRepository(IServiceProvider serviceProvider,
    //                                                   string stateName,
    //                                                   Type serviceTargetTrait,
    //                                                   Type repositoryType,
    //                                                   Type repositoryWithEntityIdType)
    //    {
    //        var instType = repositoryType;

    //        var genericParams = serviceTargetTrait.GetGenericArguments();

    //        if (genericParams.Length == 2)
    //            instType = repositoryWithEntityIdType;

    //        instType = instType.MakeGenericType(genericParams);

    //        return ActivatorUtilities.CreateInstance(serviceProvider, instType, stateName, this.StorageName);
    //    }

    //    #endregion
    //}
}
