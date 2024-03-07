//// Copyright (c) Nexai.
//// The Democrite licenses this file to you under the MIT license.
//// Produce by nexai & community (cf. docs/Teams.md)

//namespace Democrite.Framework.Core.Abstractions.Storages
//{
//    using System.Linq.Expressions;

//    /// <summary>
//    /// Service in charge to  inspect data in any storage
//    /// </summary>
//    public interface IStorageReadRepository<TEntity>
//    {
//        /// <summary>
//        /// Gets all stored entity
//        /// </summary>
//        Task<IReadOnlyCollection<TEntity>> FindAllAsync(CancellationToken token);

//        /// <summary>
//        /// Gets all stored entity correspoding to <paramref name="expression"/> filter
//        /// </summary>
//        /// <remarks>
//        ///     Attention if data are store in cluster Memory orleang storage, search could have heavy consequence.
//        ///     In those condition the repository have to download all data before applying filter.
//        ///     
//        ///     This is due to memory storage to have data in serialize <see cref="ReadOnlyMemory{T}"/> state and not the direct object.
//        /// </remarks>
//        Task<IReadOnlyCollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken token);

//    }
//}
