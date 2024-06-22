// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker
{
    using Democrite.Framework.Extensions.Docker.Abstractions;

    using global::Docker.DotNet;

    using System;

    /// <summary>
    /// 
    /// </summary>
    internal readonly struct DockerClientProxy : IDockerClientProxy
    {
        #region Fields

        private readonly DockerClient _dockerClient;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerClientProxy"/> struct.
        /// </summary>
        public DockerClientProxy(DockerClient dockerClient)
        {
            this._dockerClient = dockerClient;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public DockerClientConfiguration Configuration
        {
            get { return this._dockerClient.Configuration; }
        }

        /// <inheritdoc />
        public TimeSpan DefaultTimeout
        {
            get { return this._dockerClient.DefaultTimeout; }
            set { this._dockerClient.DefaultTimeout = value; }
        }

        /// <inheritdoc />
        public IContainerOperations Containers
        {
            get { return this._dockerClient.Containers; }
        }

        /// <inheritdoc />
        public IImageOperations Images
        {
            get { return this._dockerClient.Images; }
        }

        /// <inheritdoc />
        public INetworkOperations Networks
        {
            get { return this._dockerClient.Networks; }
        }

        /// <inheritdoc />
        public IVolumeOperations Volumes
        {
            get { return this._dockerClient.Volumes; }
        }

        /// <inheritdoc />
        public ISecretsOperations Secrets
        {
            get { return this._dockerClient.Secrets; }
        }

        /// <inheritdoc />
        public IConfigOperations Configs
        {
            get { return this._dockerClient.Configs; }
        }

        /// <inheritdoc />
        public ISwarmOperations Swarm
        {
            get { return this._dockerClient.Swarm; }
        }

        /// <inheritdoc />
        public ITasksOperations Tasks
        {
            get { return this._dockerClient.Tasks; }
        }

        /// <inheritdoc />
        public ISystemOperations System
        {
            get { return this._dockerClient.System; }
        }

        /// <inheritdoc />
        public IPluginOperations Plugin
        {
            get { return this._dockerClient.Plugin; }
        }

        /// <inheritdoc />
        public IExecOperations Exec
        {
            get { return this._dockerClient.Exec; }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        #endregion
    }
}
