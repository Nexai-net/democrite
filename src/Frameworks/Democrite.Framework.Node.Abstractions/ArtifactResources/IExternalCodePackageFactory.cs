// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using Microsoft.Extensions.Logging;

    using System.Threading.Tasks;

    /// <summary>
    /// Factory in charge to managed link between static information <see cref="IArtifactCodePackageResource"/> and 
    /// remote guided execution through <see cref="IExternalCodeExecutor"/>
    /// </summary>
    public interface IExternalCodePackageFactory
    {
        /// <summary>
        /// Build <see cref="IExternalCodeExecutor"/> from <see cref="IArtifactCodePackageResource"/>
        /// </summary>
        ValueTask<IExternalCodeExecutor> BuildAsync(IArtifactCodePackageResource artifactCodePackageResource, ILogger logger, CancellationToken token);
    }
}
