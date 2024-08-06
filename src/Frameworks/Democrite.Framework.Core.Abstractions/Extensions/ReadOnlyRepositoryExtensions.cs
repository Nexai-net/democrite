// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class ReadOnlyRepositoryExtensions
    {
        #region Methods

        /// <summary>
        /// Gets all value
        /// </summary>
        public static ValueTask<IReadOnlyCollection<TEntity>> GetAllAsync<TEntity, TEntityId>(this IReadOnlyRepository<TEntity, TEntityId> repo, CancellationToken token)
                where TEntity : IEntityWithId<TEntityId>
                where TEntityId : notnull, IEquatable<TEntityId>
        {
            return repo.GetValuesAsync(null, token);
        }

        #endregion
    }
}
