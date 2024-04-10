// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using Orleans.Configuration;

    using System;

    /// <summary>
    /// Produce <see cref="MemoryGrainTrackStorage"/> associate to name
    /// </summary>
    internal static class MemoryGrainStorageRepositoryFactory
    {
        /// <summary>
        /// Creates a dedicated <see cref="MemoryGrainTrackStorage"/> associate to name
        /// </summary>
        internal static MemoryGrainTrackStorage Create(IServiceProvider services, string name)
        {
            return ActivatorUtilities.CreateInstance<MemoryGrainTrackStorage>(services,
                                                                              services.GetRequiredService<IOptionsMonitor<MemoryGrainStorageOptions>>().Get(name),
                                                                              name);
        }
    }
}
