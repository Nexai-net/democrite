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
    public interface IReadOnlyRepository<TEntity, TEntityId>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : notnull, IEquatable<TEntityId>
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the repository support expression filter.
        /// </summary>
        bool SupportExpressionFilter { get; }

        #endregion

        #region Methods

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

        /// <summary>
        /// Gets the value by its id.
        /// </summary>
        ValueTask<TEntity?> GetValueByIdAsync([NotNull] TEntityId entityId, CancellationToken token);

        /// <summary>
        /// Gets the value by its id and project to <see cref="TProjection"/>
        /// </summary>
        ValueTask<TProjection?> GetValueByIdAsync<TProjection>([NotNull] TEntityId entityId, CancellationToken token);

        /// <summary>
        /// Gets the values by its ids and project to <see cref="TProjection"/>
        /// </summary>
        ValueTask<IReadOnlyCollection<TProjection>> GetValueByIdAsync<TProjection>([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token);

        /// <summary>
        /// Gets the values by its ids
        /// </summary>
        ValueTask<IReadOnlyCollection<TEntity>> GetValueByIdAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token);

        #endregion
    }
}
