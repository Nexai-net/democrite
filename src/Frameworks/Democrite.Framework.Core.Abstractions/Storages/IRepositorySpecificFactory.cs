// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Storages
{
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;

    /// <summary>
    /// Factory used to produce dedicated repository, this factory is linked to config name
    /// </summary>
    /// <remarks>
    ///     A global factory is use on top level to get access without knowing in advanced the config name associate
    /// </remarks>
    public interface IRepositorySpecificFactory
    {
        /// <summary>
        /// Get a new repository based on <paramref name="storageName"/> and <paramref name="stateName"/>
        /// </summary>
        IReadOnlyRepository<TEntity, TEntityId> Get<TTargetRepo, TEntity, TEntityId>(IServiceProvider serviceProvider, RepositoryGetOptions request)
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : IEquatable<TEntityId>
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>;
    }
}
