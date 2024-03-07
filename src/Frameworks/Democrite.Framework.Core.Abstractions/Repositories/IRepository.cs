// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Repositories
{
    /// <summary>
    /// External Storage call repository access service to access data, Read and write
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
    {
        /// <summary>
        /// Pushes the data record into storage
        /// </summary>
        Task<bool> PushDataRecordAsync(TEntity entity, bool insertIfNew, CancellationToken token);
    }

    /// <summary>
    /// External Storage  call repository access service to access data, Read and write
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity, TEntityId> : IRepository<TEntity>, IReadOnlyRepository<TEntity, TEntityId>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : notnull, IEquatable<TEntityId>
    {
        /// <summary>
        /// Deletes a record by its unique id.
        /// </summary>
        Task<bool> DeleteRecordAsync(TEntityId uid, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes records by they unique id.
        /// </summary>
        Task<bool> DeleteRecordsAsync(CancellationToken cancellationToken, IEnumerable<TEntityId> uids);
    }
}
