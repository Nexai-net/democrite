// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Node.ArtifactResources.ExecCodePreparationSteps;
    using Democrite.Framework.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc cref="IExternalCodePackageFactory" />
    /// <seealso cref="IExternalCodePackageFactory" />
    public sealed class ExternalCodePackageFactory : IExternalCodePackageFactory
    {
        #region Fields

        private readonly IProcessSystemService _processSystemService;
        private readonly IFileSystemHandler _fileSystemHandler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IJsonSerializer _jsonSerializer;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCodePackageFactory"/> class.
        /// </summary>
        public ExternalCodePackageFactory(IJsonSerializer jsonSerializer,
                                          IServiceProvider serviceProvider,
                                          IFileSystemHandler fileSystemHandler,
                                          IProcessSystemService processSystemService)
        {
            this._jsonSerializer = jsonSerializer;
            this._serviceProvider = serviceProvider;
            this._fileSystemHandler = fileSystemHandler;
            this._processSystemService = processSystemService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<IExternalCodeExecutor> BuildAsync(IArtifactCodePackageResource artifactCodePackageResource,
                                                           ILogger logger,
                                                           CancellationToken token)
        {
            var steps = new List<IExternalCodeExecutorPreparationStep>(5);

            // check local - if not download

            // ZIP : unzip if needed
            if (artifactCodePackageResource.PackageType == ArtifactPackageTypeEnum.Zip)
            {
            }

            // Check if exec are in local
            steps.Add(this._serviceProvider.GetRequiredServiceByKey<string, IExternalCodeExecutorPreparationStep>(PreparationLocalCheckStep.KEY));

            // Check if executor are in local
            steps.Add(this._serviceProvider.GetRequiredServiceByKey<string, IExternalCodeExecutorPreparationStep>(PreparationExecutorCheckStep.KEY));

            IExternalCodeExecutor executor;

            if (!artifactCodePackageResource.AllowPersistence)
            {
                executor = new ExternalCodeCLIExecutor(artifactCodePackageResource,
                                                       steps,
                                                       this._processSystemService,
                                                       this._fileSystemHandler,
                                                       this._jsonSerializer,
                                                       logger);
            }
            else
            {
                throw new NotImplementedException("Remote persistent is not available yet");
            }

            return ValueTask.FromResult<IExternalCodeExecutor>(executor);
        }

        #endregion
    }
}
