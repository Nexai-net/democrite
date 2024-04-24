// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Extensions.Docker.Abstractions.Models;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Services;

    using global::Docker.DotNet;
    using global::Docker.DotNet.Models;

    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class DockerExternalProcess : ExternalBaseProcess, IExternalProcess
    {
        #region Fields

        private readonly ArtifactExecutableDefinition _definition;
        private readonly DockerClient _client;
        private readonly string _executor;
        private readonly Config _config;
        private readonly bool _deamonMode;
        private string? _containerId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerExternalProcess"/> class.
        /// </summary>
        public DockerExternalProcess(string executor,
                                     IReadOnlyCollection<string> arguments,
                                     ArtifactExecutableDefinition definition,
                                     ArtifactExecutableDockerEnvironmentDefinition env,
                                     CancellationToken cancellationToken)
            : base(arguments, cancellationToken)
        {
            this._definition = definition;
            this._executor = executor.ToLowerInvariant();

            this.Executable = $"{definition.DisplayName}:{definition.Uid}:{env.Image}:{env.Tag}->{executor}";

            this._config = new Config()
            {
                Image = definition.PackageType == ArtifactPackageTypeEnum.EnvironmentRepository
                                ? env.Image + ":" + (string.IsNullOrEmpty(env.Tag) ? "latest" : env.Tag)
                                : definition.Uid + ":latest",
                AttachStdout = true,
                AttachStderr = true,
                Cmd = this._executor.AsEnumerable()
                                    .Concat(arguments)
                                    .ToList(),
                Tty = true
            };

            this._deamonMode = arguments.FirstOrDefault(a => a.StartsWith("--port:")) is not null;

            this._client = new DockerClientConfiguration()
                                    .CreateClient(string.IsNullOrEmpty(env.MinimalRequiredVersion)
                                                            ? null
                                                            : System.Version.Parse(env.MinimalRequiredVersion));
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Executable { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return this.Executable;
        }

        /// <inheritdoc />
        protected override void OnDisposeThreadSafeBegin()
        {
            Task.Run(async () => await KillAsync(default)).Wait();
            this._client.Dispose();
        }

        /// <inheritdoc />
        protected override async Task OnRunAsync(CancellationToken cancellationToken)
        {
            var createParam = new CreateContainerParameters(this._config)
            {
                Name = this._definition.DisplayName.ToLowerInvariant().Replace(":", "-") + "-" + this._definition.Uid,
                Tty = true
            };

            if (this._deamonMode)
            {
                createParam.HostConfig = new HostConfig()
                {
                    ExtraHosts = new List<string>()
                    {
                        "host.docker.internal:host-gateway"
                    }
                };

                createParam.Cmd.Add("--server:host.docker.internal");
            }

            await ClearExistingContainerAsync(createParam.Name);

            var createResponse = await this._client.Containers.CreateContainerAsync(createParam, this.CancellationToken);

            if (createResponse is null)
                throw new InvalidOperationException("Docker container creation failed : " + string.Join(Environment.NewLine, createResponse!.Warnings));

            this._containerId = createResponse.ID;

            var start = await this._client.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters(), this.CancellationToken);

            if (!start)
                throw new InvalidOperationException("Docker container start failed : " + this.Executable);

            var runningTask = Task.Run(async () =>
            {
                var progress = new Progress<string>();
                progress.ProgressChanged += (e, a) => base.ProcessOutputDataReceived(a);

                // Listen stdout && stderr

#pragma warning disable CS0618 // Type or member is obsolete

                // Implementation using GetContainerLogsAsync with multiplex stream failed (deadlock)

                using (var stream = await this._client.Containers.GetContainerLogsAsync(createResponse.ID,
                                                                                        new ContainerLogsParameters()
                                                                                        {
                                                                                            Follow = true,
                                                                                            ShowStdout = true,
                                                                                            ShowStderr = true,
                                                                                        },
                                                                                        this.CancellationToken))

                using (var reader = new StreamReader(stream))
                {
                    var line = await reader.ReadLineAsync(this.CancellationToken);

                    if (!string.IsNullOrEmpty(line))
                        base.ProcessOutputDataReceived(line);
                }
#pragma warning restore CS0618 // Type or member is obsolete

                //var multiplexStream = await this._client.Containers.GetContainerLogsAsync(createResponse.ID,
                //                                                                       false,
                //                                                                       new ContainerLogsParameters()
                //                                                                       {
                //                                                                           Follow = true,
                //                                                                           ShowStdout = true,
                //                                                                           ShowStderr = true,
                //                                                                       },
                //                                                                       this.CancellationToken);

                //using (multiplexStream)

                //using (var stdoutStream = new MemoryStream())
                //using (var stdReader = new StreamReader(stdoutStream))

                //using (var stderrStream = new MemoryStream())
                //using (var errReader = new StreamReader(stderrStream))

                //{
                //    var done = false;

                //    var stdTask = stdReader.StreamLinesAsync(s => base.ProcessOutputDataReceived(s), this.CancellationToken, () => done);
                //    var errTask = errReader.StreamLinesAsync(s => base.ProcessErrorDataReceived(s), this.CancellationToken, () => done);

                //    await multiplexStream.CopyOutputToAsync(Stream.Null, stdoutStream, stderrStream, this.CancellationToken);

                //    done = true;

                //    await stdoutStream.FlushAsync();
                //    await stderrStream.FlushAsync();

                //    stdoutStream.Close();
                //    stderrStream.Close();

                //    await stdTask;
                //    await errTask;
                //}

                await this._client.Containers.WaitContainerAsync(createResponse.ID, this.CancellationToken);

                return;
            });

            SetProcessWaitingTask(runningTask);
        }

        /// <summary>
        /// Determines whether [is alive asynchronous] [the specified cancellation token].
        /// </summary>
        public async Task<bool> IsAliveAsync(ArtifactExecutableDefinition definition, CancellationToken cancellationToken)
        {
            // TODO : check definition comment vs hash

            if (string.IsNullOrEmpty(this._containerId))
                return false;

            try
            {
                var containerStatus = await this._client.Containers.InspectContainerAsync(this._containerId, cancellationToken);
                return containerStatus.State.Dead == false;
            }
            catch { }

            return false;
        }

        /// <inheritdoc />
        public override async Task KillAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(this._containerId))
                    await this._client.Containers.KillContainerAsync(this._containerId, new ContainerKillParameters());
            }
            catch
            {
            }

            try
            {
                if (!string.IsNullOrEmpty(this._containerId))
                {
                    await this._client.Containers.RemoveContainerAsync(this._containerId, new ContainerRemoveParameters()
                    {
                        Force = true,
                    });
                }
            }
            catch
            {
            }
        }

        #region Tools

        /// <summary>
        /// Search for existing <paramref name="containerName"/> and remove them
        /// </summary>
        private async Task ClearExistingContainerAsync(string containerName)
        {
            var existingContainer = await this._client.Containers.ListContainersAsync(new ContainersListParameters()
            {
                Limit = 1,
                Filters = new Dictionary<string, IDictionary<string, bool>>()
                {
                    { "name", new Dictionary<string, bool>()
                    {
                        { containerName, true }
                    } }
                }
            });

            if (existingContainer.Count > 0)
            {
                foreach (var existing in existingContainer)
                {
                    try
                    {
                        await this._client.Containers.RemoveContainerAsync(existing.ID, new ContainerRemoveParameters() { Force = true }, this.CancellationToken);
                    }
                    catch
                    {

                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
