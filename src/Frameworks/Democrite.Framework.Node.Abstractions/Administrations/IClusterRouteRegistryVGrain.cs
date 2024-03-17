// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Administrations
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Models;

    using Orleans.Concurrency;

    /// <summary>
    /// VGrain singleton in charge to provide route (Redirection, ...) information at scale of the cluster
    /// </summary>
    [VGrainIdSingleton]
    [DemocriteSystemVGrain]
    public interface IClusterRouteRegistryVGrain : IVGrain, IGrainWithGuidKey
    {
        /// <summary>
        /// Subscribes to the event Route change. The subscriber MUST inherite from <see cref="IAdminEventReceiver"/>
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<Guid> SubscribeRouteChangeAsync(DedicatedGrainId<IAdminEventReceiver> target, GrainCancellationToken cancellationToken);

        /// <summary>
        /// Delete subscription through it's id
        /// </summary>
        Task UnsubscribeAsync(Guid subscriptionId);

        /// <summary>
        /// Make request to append a new globale redirection rule
        /// </summary>
        /// <returns>
        ///     <c>false</c> if the redirection could be added due to any reason, conflict, already exist, security ... otherwise <c>true</c>
        /// </returns>
        Task<bool> RequestAppendRedirectionAsync(VGrainRedirectionDefinition grainRedirections, IIdentityCard identity);

        /// <summary>
        /// Make request to append a new globale redirection rules
        /// </summary>
        /// <returns>
        ///     <c>false</c> if the redirections could be added due to any reason, conflict, already exist, security ... otherwise <c>true</c>
        /// </returns>
        Task<bool> RequestAppendRedirectionAsync(IReadOnlyCollection<VGrainRedirectionDefinition> grainRedirections, IIdentityCard identity);

        /// <summary>
        /// Make request to pop a redirection rule
        /// </summary>
        /// <return>
        ///     <c>False</c> if the rule could not be removed due to any reason doesn't exist, security, ...otherwise <c>true</c>
        /// </return>
        Task<bool> RequestPopRedirectionAsync(Guid redirectionId, IIdentityCard identity);

        /// <summary>
        /// Make request to append a new globale redirection rule
        /// </summary>
        /// <return>
        ///     <c>False</c> if the rule could not be removed due to any reason doesn't exist, security, ...otherwise <c>true</c>
        /// </return>
        Task<bool> RequestPopRedirectionAsync(IReadOnlyCollection<Guid> redirectionIds, IIdentityCard identity);

        /// <summary>
        /// Gets the global redirection used to apply a cluster scale
        /// </summary>
        /// <param name="etagInCache">Send to last etag you received, it is used to compare if you data are up to date or not</param>
        /// <returns>
        ///     <c>null</c> if your etag if the lastest, otherwise return the full information
        /// </returns>
        [AlwaysInterleave]
        Task<EtagContainer<IReadOnlyCollection<VGrainRedirectionDefinition>>?> GetGlobalRedirection(string etagInCache, GrainCancellationToken cancellationToken);
    }
}
