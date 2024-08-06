// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Abstractions.Repositories
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using MongoDB.Driver;

    using System;

    /// <summary>
    /// Advanced repository that expose directly Mongo accessor.
    /// This service is used to provide full mongo feature
    /// </summary>
    public interface IMongoRepository<TEntity, TEntityId> : IRepository<TEntity, TEntityId>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : notnull, IEquatable<TEntityId>
    {
        #region Properties

        /// <summary>
        /// Gets the mongo collection.
        /// </summary>
        IMongoCollection<TEntity> MongoCollection { get; }

        #endregion
    }
}
