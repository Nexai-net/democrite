// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Repositories
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Extensions.Mongo.Models;

    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Providers.MongoDB.Utils;

    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class MongoRepositoryFactory : IRepositorySpecificFactory
    {
        /// <inheritdoc />
        public IReadOnlyRepository<TEntity, TEntityId> Get<TTargetRepo, TEntity, TEntityId>(IServiceProvider serviceProvider, RepositoryGetOptions request)
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : IEquatable<TEntityId>
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>
        {
            if (request.IsReadOnly)
            {
                return new MongoReadOnlyRepository<TEntity, TEntityId>(serviceProvider.GetRequiredService<IMongoClientFactory>(),
                                                                       serviceProvider,
                                                                       request.ConfigurationName!,
                                                                       request.StorageName,
                                                                       request.PreventAnyKindOfDiscriminatorUsage);
            }

            return new MongoRepository<TEntity, TEntityId>(serviceProvider.GetRequiredService<IMongoClientFactory>(),
                                                           serviceProvider,
                                                           request.ConfigurationName!,
                                                           request.StorageName,
                                                           request.PreventAnyKindOfDiscriminatorUsage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class MongoRepositoryGrainStateFactory : IRepositorySpecificFactory
    {
        /// <inheritdoc />
        public IReadOnlyRepository<TEntity, TEntityId> Get<TTargetRepo, TEntity, TEntityId>(IServiceProvider serviceProvider, RepositoryGetOptions request)
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : IEquatable<TEntityId>
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>
        {
            if (request.IsReadOnly)
            {
                return new MongoReadonlyContainerRepository<GrainStateContainer<TEntity>, TEntity, TEntityId, string>(serviceProvider.GetRequiredService<IMongoClientFactory>(),
                                                                                                                      serviceProvider,
                                                                                                                      request.ConfigurationName!,
                                                                                                                      request.StorageName,
                                                                                                                      c => c.State!,
                                                                                                                      e => throw new InvalidOperationException("You are not allowed to create grain state through repository"),
                                                                                                                      (collectionPrefix, configurationName, storageName, originalCollectionName) => originalCollectionName,
                                                                                                                      request.PreventAnyKindOfDiscriminatorUsage); //"Grains" + 
            }

            return new MongoContainerRepository<GrainStateContainer<TEntity>, TEntity, TEntityId, string>(serviceProvider.GetRequiredService<IMongoClientFactory>(),
                                                                                                          serviceProvider,
                                                                                                          request.ConfigurationName!,
                                                                                                          request.StorageName,
                                                                                                          c => c.State!,
                                                                                                          e => throw new InvalidOperationException("You are not allowed to create grain state through repository"),
                                                                                                          (collectionPrefix, configurationName, storageName, originalCollectionName) => originalCollectionName, 
                                                                                                          request.PreventAnyKindOfDiscriminatorUsage); // "Grains"
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class MongoRepositoryDefinitionFactory : IRepositorySpecificFactory
    {
        #region Fields

        private static readonly MethodInfo s_getMethodGeneric;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoRepositoryDefinitionFactory"/> class.
        /// </summary>
        static MongoRepositoryDefinitionFactory()
        {
            Expression<Func<MongoRepositoryDefinitionFactory, object>> exprGet = m => m.GetDefinitionRepo<SequenceDefinition, Guid>(null!, null!);
            s_getMethodGeneric = ((MethodCallExpression)exprGet.Body).Method.GetGenericMethodDefinition();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IReadOnlyRepository<TEntity, TEntityId> Get<TTargetRepo, TEntity, TEntityId>(IServiceProvider serviceProvider, RepositoryGetOptions request)
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : IEquatable<TEntityId>
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>
        {
            var trait = typeof(TEntity);
            if (trait.IsAssignableTo(typeof(IDefinition)) == false || !trait.IsClass)
                throw new InvalidCastException("Entity " + trait + " MUST by a class and inherit from " + typeof(IDefinition));

            var result = s_getMethodGeneric.MakeGenericMethodWithCache(new[] { typeof(TEntity), typeof(TEntityId) })
                                           .Invoke(this, new object?[] { serviceProvider, request });

            return (IReadOnlyRepository<TEntity, TEntityId>)result!;
        }

        #region Tools

        /// <inheritdoc />
        private IReadOnlyRepository<TEntity, TEntityId> GetDefinitionRepo<TEntity, TEntityId>(IServiceProvider serviceProvider, RepositoryGetOptions request)
            where TEntity : class, IEntityWithId<TEntityId>, IDefinition
            where TEntityId : IEquatable<TEntityId>
        {
            if (request.IsReadOnly)
            {
                return new MongoReadonlyContainerRepository<DefinitionContainer<TEntity>, TEntity, TEntityId, Guid>(serviceProvider.GetRequiredService<IMongoClientFactory>(),
                                                                                                                    serviceProvider,
                                                                                                                    request.ConfigurationName!,
                                                                                                                    request.StorageName,
                                                                                                                    c => c.Definition,
                                                                                                                    e => throw new InvalidOperationException("You are not allowed to create definition through repository"),
                                                                                                                    (collectionPrefix, configurationName, storageName, originalCollectionName) => "Definitions" + originalCollectionName, 
                                                                                                                    request.PreventAnyKindOfDiscriminatorUsage);
            }

            return new MongoContainerRepository<DefinitionContainer<TEntity>, TEntity, TEntityId, Guid>(serviceProvider.GetRequiredService<IMongoClientFactory>(),
                                                                                                        serviceProvider,
                                                                                                        request.ConfigurationName!,
                                                                                                        request.StorageName,
                                                                                                        c => c.Definition,
                                                                                                        d => new DefinitionContainer<TEntity>(d),
                                                                                                        (collectionPrefix, configurationName, storageName, originalCollectionName) => "Definitions" + originalCollectionName, 
                                                                                                        request.PreventAnyKindOfDiscriminatorUsage);
        }

        #endregion

        #endregion
    }
}
