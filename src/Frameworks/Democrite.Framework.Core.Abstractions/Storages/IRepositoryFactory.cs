// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Storages
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    /// <summary>
    /// Factory in charge to get dedicated repository based on criteria 
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Get a new repository based on <paramref name="storageName"/> and <paramref name="stateName"/>
        /// </summary>
        /// <remarks>
        ///     Initialize the repository if needed
        /// </remarks>
        ValueTask<TTargetRepo> GetAsync<TTargetRepo, TEntity>(string stateName,
                                                              string? storageName = null,
                                                              bool blockInitialization = false,
                                                              CancellationToken cancellationToken = default)
            where TTargetRepo : IReadOnlyRepository<TEntity>;

        ///// <summary>
        ///// Get a new repository based on <paramref name="storageName"/> and <paramref name="stateName"/>
        ///// </summary>
        ///// <remarks>
        /////     Initialize the repository if needed
        ///// </remarks>
        //ValueTask<TTargetRepo> GetAsync<TTargetRepo, TEntity>(Type targetRepo,
        //                                                      Type entityType,
        //                                                      string stateName,
        //                                                      string? storageName = null,
        //                                                      bool blockInitialization = false,
        //                                                      CancellationToken cancellationToken = default)
        //    where TTargetRepo : IReadOnlyRepository<TEntity>;
    }
}
