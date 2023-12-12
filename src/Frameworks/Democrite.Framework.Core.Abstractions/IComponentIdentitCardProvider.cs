// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Orleans.Runtime;
    using Orleans.Services;

    /// <summary>
    /// Provider of <see cref="IComponentIdentityCard"/>
    /// </summary>
    internal interface IComponentIdentitCardProvider : IGrainService
    {
        /// <summary>
        /// Gets the component identity card from <paramref name="componentId"/>
        /// </summary>
        ValueTask<IComponentIdentityCard> GetComponentIdentityCardAsync(GrainId grainId, Guid componentId);
    }
}
