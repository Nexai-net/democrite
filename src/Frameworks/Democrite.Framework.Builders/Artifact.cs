// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Artifacts;

    /// <summary>
    /// Artifact builder
    /// </summary>
    public static class Artifact
    {
        /// <summary>
        /// Get a VGrain code artifact 
        /// </summary>
        public static IArtifactExecutablePackageResourceBuilder VGrain(string simpleNameIdentifier,
                                                                       string? displayName = null,
                                                                       Guid? uid = null)
        {
            return new ArtifactExecutablePackageBuilder(simpleNameIdentifier, displayName, uid);
        }
    }
}
