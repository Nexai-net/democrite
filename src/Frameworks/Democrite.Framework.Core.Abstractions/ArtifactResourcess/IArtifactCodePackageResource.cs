// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    /// <summary>
    /// Define an artifact about executable code that can be managed by a remoting vgrain
    /// </summary>
    public interface IArtifactCodePackageResource : IArtifactPackageResource, IArtifactExecutableResource
    {
    }
}
