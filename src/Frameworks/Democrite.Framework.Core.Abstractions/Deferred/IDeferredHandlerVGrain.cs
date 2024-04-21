// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Deferred
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using Elvex.Toolbox.Models;

    /// <summary>
    /// Singleton instance used to managed all the deferred work status through the cluster
    /// </summary>
    [VGrainIdSingleton]
    public interface IDeferredHandlerVGrain : IVGrain
    {
        /// <summary>
        /// Subscribes the specified deferred observer.
        /// </summary>
        Task Subscribe(IDeferredObserver deferredObserver);

        /// <summary>
        /// Unsubscribes the specified deferred observer.
        /// </summary>
        Task Unsubscribe(IDeferredObserver deferredObserver);

        /// <summary>
        /// Gets the last deferred word status.
        /// </summary>
        Task<DeferredStatusMessage?> GetLastDeferredStatusAsync(Guid deferredId);

        /// <summary>
        /// Consumes the deferred reponse.
        /// </summary>
        Task<TResponse?> ConsumeDeferredResponseAsync<TResponse>(Guid deferredId);

        /// <summary>
        /// Cleans up deferred work information
        /// </summary>
        Task CleanUpDeferredWork(Guid deferredId);

        /// <summary>
        /// Gets the last deferred word status from <paramref name="sourceId"/>
        /// </summary>
        Task<IReadOnlyCollection<DeferredStatusMessage>> GetLastDeferredStatusBySourceIdAsync(Guid sourceId, IIdentityCard? identityCardFilter = null);

        /// <summary>
        /// Gets the last deferred word status from <paramref name="sourceId"/>
        /// </summary>
        Task<IReadOnlyCollection<DeferredStatusMessage>> GetLastDeferredStatusByEmitterAsync(IIdentityCard? identityCardFilter = null);

        /// <summary>
        /// Create a new deferred work.
        /// </summary>
        Task<Guid> CreateDeferredWorkAsync(Guid? sourceId, IIdentityCard identityCard, ConcretBaseType expectedResponseType, DateTime? utcEndValidity = null);

        /// <summary>
        /// Keep deferred work alive.
        /// </summary>
        /// <remarks>
        ///     Only the owner of the deferred work (The one that create throught CreateDeferredWorkAsync)
        /// </remarks>
        Task<bool> KeepDeferredWorkStatusAsync(Guid deferredWorkUid, IIdentityCard identityCard);

        /// <summary>
        /// Cancel the deferred work status.
        /// </summary>
        /// <remarks>
        ///     Only the owner of the deferred work (The one that create throught CreateDeferredWorkAsync)
        /// </remarks>
        Task<bool> CancelDeferredWorkStatusAsync(Guid deferredWorkUid, IIdentityCard identityCard);

        /// <summary>
        /// Cancel the deferred work status.
        /// </summary>
        /// <remarks>
        ///     Only the owner of the deferred work (The one that create throught CreateDeferredWorkAsync)
        /// </remarks>
        Task<bool> ExceptionDeferredWorkStatusAsync(Guid deferredWorkUid, IIdentityCard identityCard, DemocriteInternalException exception);

        /// <summary>
        /// Finish deferred work
        /// </summary>
        /// <remarks>
        ///     Only the owner of the deferred work (The one that create throught CreateDeferredWorkAsync)
        /// </remarks>
        Task<bool> FinishDeferredWorkWithDataAsync<TData>(Guid deferredWorkUid, IIdentityCard identityCard, TData response);

        /// <summary>
        /// Finish deferred work
        /// </summary>
        /// <remarks>
        ///     Only the owner of the deferred work (The one that create throught CreateDeferredWorkAsync)
        /// </remarks>
        Task<bool> FinishDeferredWorkWithResultAsync(Guid deferredWorkUid, IIdentityCard identityCard, IExecutionResult response);
    }
}
