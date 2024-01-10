// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Core.Abstractions;

    using System;

    /// <summary>
    /// Wizard used to setup vgrain
    /// </summary>
    public interface IDemocriteNodeVGrainWizard
    {
        /// <summary>
        /// Gets the Wizard root.
        /// </summary>
        IDemocriteNodeWizard Root { get; }

        /// <summary>
        /// Removes all automatic discovery vgrain.
        /// </summary>
        IDemocriteNodeVGrainWizard RemoveAllAutoDiscoveryVGrain();

        /// <summary>
        /// Adds a specific <typeparamref name="TVGrain"/> <see cref="IVGrain"/>
        /// </summary>
        IDemocriteNodeVGrainWizard Add<TVGrain, TVGrainImplementation>()
            where TVGrain : IVGrain
            where TVGrainImplementation : TVGrain;

        /// <summary>
        /// Remoev a specific <typeparamref name="TVGrain"/> <see cref="IVGrain"/>
        /// </summary>
        IDemocriteNodeVGrainWizard Remove<TVGrain>()
            where TVGrain : IVGrain;

        /// <summary>
        /// Remoev a specific <paramref name="vgrain"/> <see cref="IVGrain"/>
        /// </summary>
        IDemocriteNodeVGrainWizard Remove(Type vgrain);

        /// <summary>
        /// Adds a specific <typeparamref name="TVGrain"/> <see cref="IVGrain"/>
        /// </summary>
        IDemocriteNodeVGrainWizard Add(Type vgrainInterface, Type vgrainImplementation);
    }
}
