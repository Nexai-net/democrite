// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using Elvex.Toolbox.Abstractions.Services;

    /// <summary>
    /// Produce <see cref="IExternalProcess"/> using docker
    /// </summary>
    public interface IDockerProcessorFactory
    {
        /// <summary>
        /// Launches the process.
        /// </summary>
        Task<IExternalProcess> BuildProcessHandler(string executor, List<string> args, ArtifactExecutableDefinition definition, CancellationToken token);
    }
}
