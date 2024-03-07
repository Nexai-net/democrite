// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Repositories
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    /// <summary>
    /// External Storage  call repository access service to access data, With read access only
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IReadOnlyRepository<TEntity>
    {
        /// <summary>
        /// Gets the first <see cref="TEntity"/> that match the condition <paramref name="filterExpression"/>
        /// </summary>
        ValueTask<TEntity?> GetFirstValueAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token);

        /// <summary>
        /// Gets the first <see cref="TEntity"/> that match the condition <paramref name="filterExpression"/>
        /// </summary>
        ValueTask<TProjection?> GetFirstValueAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token);

        /// <summary>
        /// Gets the values <see cref="TEntity"/> that match the condition <paramref name="filterExpression"/>
        /// </summary>
        ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token);

        /// <summary>
        /// Gets the values <see cref="TEntity"/> that match the condition <paramref name="filterExpression"/>
        /// </summary>
        ValueTask<IReadOnlyCollection<TEntity>> GetValuesAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token);
    }

    /// <summary>
    /// External Storage  call repository access service to access data, With read access only
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IReadOnlyRepository<TEntity, TEntityId> : IReadOnlyRepository<TEntity>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        /// <summary>
        /// Gets the first <see cref="TEntity"/> that match the condition <paramref name="filterExpression"/>
        /// </summary>
        ValueTask<TEntity?> GetByIdValueAsync([NotNull] TEntityId entityId, CancellationToken token);

        /// <summary>
        /// Gets the first <see cref="TEntity"/> that match the condition <paramref name="filterExpression"/>
        /// </summary>
        ValueTask<IReadOnlyCollection<TEntity>> GetByIdsValueAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token);
    }
}
