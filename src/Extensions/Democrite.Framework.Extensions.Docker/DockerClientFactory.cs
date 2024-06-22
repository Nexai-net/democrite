// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker
{
    using Democrite.Framework.Extensions.Docker.Abstractions;

    using global::Docker.DotNet;
    using global::Docker.DotNet.Models;

    internal sealed class DockerClientFactory : IDockerClientFactory
    {
        #region Fields

        private readonly static AuthConfig s_anonymousConfig;

        private DockerClient? _localClient;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DockerClientFactory"/> class.
        /// </summary>
        static DockerClientFactory()
        {
            s_anonymousConfig = new AuthConfig();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDockerClientProxy GetLocal(string? configurationName = null, string? minimalVersionRequired = null)
        {
            if (this._localClient is null)
            {
                this._localClient = new DockerClientConfiguration()
                                        .CreateClient(string.IsNullOrEmpty(minimalVersionRequired)
                                                                ? null
                                                                : System.Version.Parse(minimalVersionRequired));
            }

            return new DockerClientProxy(this._localClient);
        }

        /// <inheritdoc />
        public AuthConfig GetRepositoryAuthentication(string? repository, string? configurationName)
        {
            // TODO : authentication by repo
            return s_anonymousConfig;
        }

        #endregion
    }
}
