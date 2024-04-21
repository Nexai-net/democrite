// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Deferred
{
    /// <summary>
    /// Define a service to managed await deferred work
    /// </summary>
    public interface IDeferredAwaiterHandler
    {
        /// <summary>
        /// Gets the deferred work awaiter.
        /// </summary>
        Task<IExecutionResult<TResponse>> GetDeferredWorkAwaiterAsync<TResponse>(DeferredId id, CancellationToken token = default);

        /// <summary>
        /// Gets the deferred work awaiter.
        /// </summary>
        Task<IExecutionResult<TResponse>> GetDeferredWorkAwaiterAsync<TResponse>(Guid id, CancellationToken token = default);

        /// <summary>
        /// Reserve a slot to be filled later on
        /// </summary>
        Task<Guid> ReservedDeferredWorkSlot<TResponse>(Guid? sourceId = null);

        /// <summary>
        /// Cleans up deferred work information
        /// </summary>
        Task CleanUpDeferredWork(Guid deferredId);
    }
}
