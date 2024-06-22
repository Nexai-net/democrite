// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Extensions.Docker.Abstractions;
    using Democrite.Framework.Extensions.Docker.Abstractions.Models;
    using Democrite.Framework.Extensions.Docker.Abstractions.Options;
    using Democrite.Framework.Node.Abstractions.Artifacts;

    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Supports;

    using global::Docker.DotNet.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service used to check the docker context
    /// </summary>
    internal class DockerDemocriteEnvironmentService : SafeDisposable, IInitService
    {
        #region Fields

        private readonly SupportInitializationImplementation<IServiceProvider> _initCtx;
        private readonly IArtifactDefinitionProvider _artifactDefinitionProvider;
        private readonly ILogger<DockerDemocriteEnvironmentService> _logger;
        private readonly IOptions<DockerEnvironementOptions> _options;

        private readonly IDockerClientFactory _dockerClientFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize a new instance of the class <see cref="DockerDemocriteEnvironmentService"/>
        /// </summary>
        public DockerDemocriteEnvironmentService(IOptions<DockerEnvironementOptions> option,
                                                 IArtifactDefinitionProvider artifactDefinitionProvider,
                                                 IDockerClientFactory dockerClientFactory,
                                                 ILogger<DockerDemocriteEnvironmentService>? logger = null)
        {
            this._options = option;
            this._artifactDefinitionProvider = artifactDefinitionProvider;

            this._logger = logger ?? NullLogger<DockerDemocriteEnvironmentService>.Instance;

            this._dockerClientFactory = dockerClientFactory;
            this._initCtx = new SupportInitializationImplementation<IServiceProvider>(OnInitializeAsync);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool ExpectOrleanStarted
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return this._initCtx.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return this._initCtx.IsInitialized; }
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(IServiceProvider? initializationState, CancellationToken token = default)
        {
            return this._initCtx.InitializationAsync(initializationState, token);
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return this._initCtx.InitializationAsync(token);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check if docker environement in well started and ready to be used
        /// </summary>
        private async ValueTask OnInitializeAsync(IServiceProvider? provider, CancellationToken token)
        {
            if (this._options.Value.PreLoadAllDependenciesImages == false)
                return;

            // TODO : Used the configuration name to extract a configured depo

            var artifact = await this._artifactDefinitionProvider.GetAllValuesAsync(token);

            var dockerDependencyImages = artifact.OfType<ArtifactExecutableDefinition>()
                                                 .Select(a => a.Environment)
                                                 .OfType<ArtifactExecutableDockerEnvironmentDefinition>()
                                                 .Where(env => env.EnvironmentName == ArtifactExecutableDockerEnvironmentDefinition.ENVIRONMENT_KEY)
                                                 .GroupBy(env => env.GetFullImageFormatedName())
                                                 .Select(grp => grp.First())
                                                 .ToArray();

            var client = this._dockerClientFactory.GetLocal();

            foreach (var img in dockerDependencyImages)
            {
                var fullImageName = img.GetFullImageFormatedName();
                this._logger.OptiLog(LogLevel.Information, "[Docker] Check if image is in local {image}", fullImageName);

                try
                {
                    if (img.OnlyLocal)
                    {
                        var imgs = await client.Images.ListImagesAsync(new ImagesListParameters()
                        {
                            All = true,
                            Filters = new Dictionary<string, IDictionary<string, bool>>()
                            {
                                { "reference", new Dictionary<string, bool>()
                                {
                                    { fullImageName, true }
                                } }
                            }
                        }, token);

                        if (!imgs.Any())
                            throw new KeyNotFoundException("Image not found in local, could be due to version (TAG)");

                        this._logger.OptiLog(LogLevel.Debug, "[Docker] Image {image} detected in local repository", fullImageName);

                        continue;
                    }

                    await client.Images.CreateImageAsync(new ImagesCreateParameters()
                    {
                        Tag = img.Tag,
                        FromImage = img.Image,
                        Repo = img.Repository,
                    }, this._dockerClientFactory.GetRepositoryAuthentication(img.Repository, img.ConfigurationName),
                    new Progress<JSONMessage>(m =>
                    {
                        this._logger.OptiLog(LogLevel.Debug, "[Docker] Loading {image} {status} => {message} {error}", fullImageName, m.Status, m.ProgressMessage, m.ErrorMessage);
                    }), token);

                    this._logger.OptiLog(LogLevel.Debug, "[Docker] Image {image} ready in local", fullImageName);
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Critical, "[Docker] Image pull failed '{image}' : exception {exception}", fullImageName, ex);
                }
            }
        }

        #endregion
    }
}
