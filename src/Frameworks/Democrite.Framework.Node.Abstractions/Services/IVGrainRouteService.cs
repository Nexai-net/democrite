// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;

    using System.Collections.Generic;

    /// <summary>
    /// Service in charge to apply grain redirection base on global and contextual rules
    /// </summary>
    public interface IVGrainRouteService
    {
        #region Properties

        /// <summary>
        /// Gets the cache etag.
        /// </summary>
        /// <remarks>
        ///     Used to synchronize cache through multiple level. 
        ///     If child doesn't have the same parent etag cache that mean it need to clear is cache to be up to data
        /// </remarks>
        string CacheEtag { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new <see cref="IVGrainRouteService"/> with priority redirection rule
        /// </summary>
        IVGrainRouteScopedService CreateScope(IReadOnlyCollection<VGrainRedirectionDefinition> localRedirectionRules);

        /// <summary>
        /// Gets the route to access target grain based on redirection rules
        /// </summary>
        (Type TargetGrain, string? GrainPrefixExtension, bool Cachable) GetRoute(Type originTarget, object? input, IExecutionContext? context, string? grainClassNamePrefix);

        #endregion
    }

    /// <summary>
    /// Disposable Service in charge to apply grain redirection base on global and contextual rules
    /// </summary>
    public interface IVGrainRouteScopedService : IVGrainRouteService, IDisposable
    {

    }
}
