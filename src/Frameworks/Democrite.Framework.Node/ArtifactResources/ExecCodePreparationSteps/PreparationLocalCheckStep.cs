// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ArtifactResources.ExecCodePreparationSteps
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Node.Resources;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Check file/resources are correctly in local
    /// </summary>
    /// <seealso cref="IExternalCodeExecutorPreparationStep" />
    public sealed class PreparationLocalCheckStep : ExternalCodeExecutorBasePreparationStep, IExternalCodeExecutorPreparationStep
    {
        #region Fields

        public const string KEY = "CheckLocalFileSystem";

        private readonly IFileSystemHandler _fileSystemHandler;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparationLocalCheckStep"/> class.
        /// </summary>
        public PreparationLocalCheckStep(IFileSystemHandler fileSystemHandler)
        {
            this._fileSystemHandler = fileSystemHandler;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override ValueTask<bool> SetupAsync(IArtifactCodePackageResource artifactCodePackageResource, ILogger logger, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(artifactCodePackageResource);
            ArgumentNullException.ThrowIfNull(logger);

            bool failed = false;

            var packageSourceAbsUri = artifactCodePackageResource.PackageSource != null
                                                ? this._fileSystemHandler.MakeUriAbsolute(artifactCodePackageResource.PackageSource)
                                                : null;

            if (packageSourceAbsUri != null && !string.IsNullOrEmpty(packageSourceAbsUri.LocalPath))
            {
                var exists = this._fileSystemHandler.Exists(packageSourceAbsUri);
                if (!exists)
                {
                    logger.OptiLog(LogLevel.Critical,
                                   NodeExceptionSR.ArtifactMissing,
                                   artifactCodePackageResource.Type,
                                   artifactCodePackageResource.PackageSource);

                    failed = true;
                }
            }

            if (packageSourceAbsUri != null &&
                artifactCodePackageResource.ExecutablePath != null &&
                artifactCodePackageResource.PackageSource != null &&
                !string.IsNullOrEmpty(artifactCodePackageResource.ExecutablePath))
            {
                var execUri = new Uri(packageSourceAbsUri, artifactCodePackageResource.ExecutablePath);
                var exists = this._fileSystemHandler.Exists(execUri);
                if (!exists)
                {
                    logger.OptiLog<ArtifactResourceTypeEnum, string>(LogLevel.Critical,
                                                                     NodeExceptionSR.ArtifactMissing,
                                                                     artifactCodePackageResource.Type,
                                                                     artifactCodePackageResource.ExecutablePath);

                    failed = true;
                }
            }

            return ValueTask.FromResult(!failed);
        }

        #endregion
    }
}
