// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Repositories
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using Orleans.Providers.MongoDB.Utils;

    using System;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class MongoRepository<TEntity, TEntityId> : MongoBaseRepository<TEntity, TEntityId>
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : notnull, IEquatable<TEntityId>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        public MongoRepository(IMongoClientFactory mongoClientFactory,
                               IServiceProvider serviceProvider,
                               string configurationName,
                               string storageName,
                               bool preventAnyKindOfDiscriminatorUsage)
            : base(mongoClientFactory, serviceProvider, configurationName, storageName, preventAnyKindOfDiscriminatorUsage)
        {
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class MongoReadOnlyRepository<TEntity, TEntityId> : MongoBaseRepository<TEntity, TEntityId>
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : notnull, IEquatable<TEntityId>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoReadOnlyRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        public MongoReadOnlyRepository(IMongoClientFactory mongoClientFactory,
                                       IServiceProvider serviceProvider,
                                       string configurationName,
                                       string storageName,
                                       bool preventAnyKindOfDiscriminatorUsage)
            : base(mongoClientFactory, serviceProvider, configurationName, storageName, true, preventAnyKindOfDiscriminatorUsage)
        {
        }

        #endregion
    }
}
