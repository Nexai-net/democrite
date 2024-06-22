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

    /// <summary>
    /// Factory used to create and configured all the "Processor" that will handled docker execution
    /// </summary>
    internal sealed class DockerProcessorFactory : IDockerProcessorFactory
    {
        #region Fields
        
        private readonly IDockerClientFactory _dockerClientFactory;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerProcessorFactory"/> class.
        /// </summary>
        public DockerProcessorFactory(IDockerClientFactory dockerClientFactory)
        {
            this._dockerClientFactory = dockerClientFactory;
        }

        #endregion

        #region Methods

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
                                                 this._dockerClientFactory,
                                                 token);

            await inst.RunAsync();

            return inst;
        }

        #endregion
    }
}
