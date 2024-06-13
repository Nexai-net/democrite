// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker
{
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Extensions.Docker.Abstractions;
    using Democrite.Framework.Node.Artifacts;

    using Elvex.Toolbox.Abstractions.Services;
    using Microsoft.Extensions.Configuration;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ExternalDockerCodeDeamonExecutor : ExternalCodeDeamonExecutor
    {
        #region Fields

        private readonly IDockerProcessorFactory _dockerProcessorFactory;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalDockerCodeDeamonExecutor"/> class.
        /// </summary>
        public ExternalDockerCodeDeamonExecutor(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                IProcessSystemService processSystemService,
                                                IJsonSerializer jsonSerializer,
                                                INetworkInspector networkInspector,
                                                IDockerProcessorFactory dockerProcessorFactory,
                                                IConfiguration configuration)
            : base(artifactExecutableDefinition, processSystemService, jsonSerializer, networkInspector, configuration, null)
        {
            this._dockerProcessorFactory = dockerProcessorFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Task<IExternalProcess> LaunchProcess(string executor, List<string> args, ArtifactExecutableDefinition definition, CancellationToken token)
        {
            return this._dockerProcessorFactory.BuildProcessHandler(executor, args, definition, token);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ExternalDockerCodeCLIExecutor : ExternalCodeCLIExecutor
    {
        #region Fields

        private readonly IDockerProcessorFactory _dockerProcessorFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalDockerCodeCLIExecutor"/> class.
        /// </summary>
        public ExternalDockerCodeCLIExecutor(ArtifactExecutableDefinition artifactExecutableDefinition,
                                             IProcessSystemService processSystemService,
                                             IJsonSerializer jsonSerializer,
                                             IConfiguration configuration,
                                             IDockerProcessorFactory dockerProcessorFactory) 
            : base(artifactExecutableDefinition, processSystemService, jsonSerializer, configuration, null)
        {
            this._dockerProcessorFactory = dockerProcessorFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task<IExternalProcess> LaunchProcessAsync(string executor, List<string> args, ArtifactExecutableDefinition definition, CancellationToken token)
        {
            var handler = await this._dockerProcessorFactory.BuildProcessHandler(executor, args, definition, token);
            return handler;
        }

        #endregion
    }
}
