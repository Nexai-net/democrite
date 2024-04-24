// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Extensions.Docker.Abstractions;
    using Democrite.Framework.Extensions.Docker.Abstractions.Models;

    using Elvex.Toolbox.Abstractions.Services;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class DockerProcessorFactory : IDockerProcessorFactory
    {
        /// <inheritdoc />
        public async Task<IExternalProcess> BuildProcessHandler(string executor,
                                                                List<string> args,
                                                                ArtifactExecutableDefinition definition,
                                                                CancellationToken token)
        {
            var inst = new DockerExternalProcess(executor,
                                                 args,
                                                 definition,
                                                 (ArtifactExecutableDockerEnvironmentDefinition)definition.Environment!,
                                                 token);

            await inst.RunAsync();

            return inst;
        }
    }
}
