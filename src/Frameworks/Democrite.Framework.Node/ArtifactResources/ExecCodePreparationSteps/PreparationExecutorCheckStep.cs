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
    /// Check executor correctly installed
    /// </summary>
    /// <seealso cref="IExternalCodeExecutorPreparationStep" />
    public sealed class PreparationExecutorCheckStep : ExternalCodeExecutorBasePreparationStep, IExternalCodeExecutorPreparationStep
    {
        #region Fields

        public const string KEY = "CheckExecutor";

        private static readonly string[] s_verionsArg = new[] { "--version" };

        private readonly IProcessSystemService _processSystemService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="PreparationLocalCheckStep"/> class.
        /// </summary>
        public PreparationExecutorCheckStep(IProcessSystemService processSystemService)
        {
            this._processSystemService = processSystemService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override async ValueTask<bool> SetupAsync(IArtifactCodePackageResource artifactCodePackageResource,
                                                         ILogger logger,
                                                         CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(artifactCodePackageResource.Executor))
                return true;

            ExtractExecutorAndVersion(artifactCodePackageResource.Executor, out var exec, out var version);

            bool exists = false;

            /*  --version are allways pass in argument to prevent executor to enter in console mode */

            if (string.IsNullOrEmpty(version))
            {
                exists = await this._processSystemService.CheckExecAvailableAsync(exec, cancellationToken, s_verionsArg);
            }
            else
            {
                exists = await this._processSystemService.CheckExecAvailableAsync(exec,
                                                                                  version!,
                                                                                  cancellationToken,
                                                                                  s_verionsArg);
            }

            if (!exists)
            {
                logger.OptiLog<string, string?>(LogLevel.Critical, NodeExceptionSR.ArtifactExecutorMissing, exec, version);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts the executor and version. The information <see cref="IArtifactCodePackageResource.Excutor"/> are normally stored in format "exec:version"
        /// </summary>
        private static void ExtractExecutorAndVersion(in string executorWithVersion, out string exec, out string? version)
        {
            version = null;

            ReadOnlySpan<char> execPart = executorWithVersion;

            var splitVersion = execPart.IndexOf(":");
            if (splitVersion < 0)
            {
                exec = executorWithVersion;
                return;
            }

            exec = execPart.Slice(0, splitVersion).ToString();
            version = execPart.Slice(splitVersion + 1).ToString();
        }

        #endregion
    }
}
