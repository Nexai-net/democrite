// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// KEEP : Democrite.Framework.Core.Abstractions
namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using System;

    /// <summary>
    /// 
    /// </summary>
    public static class ArtifactExecutableVGrainExtensions
    {
        /// <summary>
        /// Execute external VGrain define by artifact executable
        /// </summary>
        public static IExecutionDirectBuilder<IGenericArtifactExecutableVGrain> ExternalVGrain(this IDemocriteExecutionHandler handler, Guid artifactId)
        {
            return handler.VGrain<IGenericArtifactExecutableVGrain>(artifactId);
        }
    }
}
