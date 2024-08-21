// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Repositories
{
    public interface IEntityWithId
    {

    }

    /// <summary>
    /// Define an entity with a unique identity
    /// </summary>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    public interface IEntityWithId<TEntityId> : IEntityWithId
        where TEntityId : notnull, IEquatable<TEntityId>
    {
        /// <summary>
        /// Gets the entity unique identifier.
        /// </summary>
        TEntityId Uid { get; }
    }
}
