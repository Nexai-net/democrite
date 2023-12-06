// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Node.Models;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Executor used to handled and controlled a remote virtual grain in one shot.
    /// Argument are pass by command line
    /// </summary>
    /// <seealso cref="SafeAsyncDisposable" />
    /// <seealso cref="IExternalCodeExecutor" />
    public sealed class ExternalCodeCLIExecutor : ExternalCodeBaseExecutor, IExternalCodeExecutor
    {
        #region Fields

        private readonly IProcessSystemService _processSystemService;
        private readonly IFileSystemHandler _fileSystemHandler;
        private readonly IJsonSerializer _jsonSerializer;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCodeCLIExecutor"/> class.
        /// </summary>
        public ExternalCodeCLIExecutor(IArtifactCodePackageResource artifactCodePackageResource,
                                       IEnumerable<IExternalCodeExecutorPreparationStep> preparationSteps,
                                       IProcessSystemService processSystemService,
                                       IFileSystemHandler fileSystemHandler,
                                       IJsonSerializer jsonSerializer,
                                       ILogger logger)
            : base(artifactCodePackageResource, preparationSteps, logger)
        {
            this._fileSystemHandler = fileSystemHandler;
            this._processSystemService = processSystemService;
            this._jsonSerializer = jsonSerializer;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override async ValueTask<TOutput?> AskAsync<TOutput, TInput>(TInput? input,
                                                                                   IExecutionContext executionContext,
                                                                                   CancellationToken cancellationToken)
            where TOutput : default
            where TInput : default
        {
            var command = new RemoteExecutionCommand<TInput>(executionContext.CurrentExecutionId, input);

            var cmdJson = this._jsonSerializer.Serialize(command);

            var base64Cmd = Convert.ToBase64String(Encoding.UTF8.GetBytes(cmdJson));

            var args = new List<string>(3)
            {
                base64Cmd,
            };

            var executor = this.ArtifactCodePackageResource.ExecutablePath;

            if (!string.IsNullOrEmpty(this.ArtifactCodePackageResource.Executor))
            {
                executor = this.ArtifactCodePackageResource.Executor;
                var indexOfSeparator = executor.IndexOf(":");
                if (indexOfSeparator > -1)
                    executor = executor.Substring(0, indexOfSeparator);

                args.Insert(0, this.ArtifactCodePackageResource.ExecutablePath);
            }

            var workingDir = this._fileSystemHandler.MakeUriAbsolute(this.ArtifactCodePackageResource.PackageSource);

            using (var processor = await this._processSystemService.StartAsync(executor,
                                                                               workingDir.LocalPath,
                                                                               cancellationToken,
                                                                               args.ToArray()))
            {
                await processor.GetAwaiterTask();

                var resultTag = executionContext.CurrentExecutionId + ":";

                var resultLog = processor.StandardOutput.FirstOrDefault(l => l.StartsWith(resultTag));

                // Managed result
                if (!string.IsNullOrEmpty(resultLog))
                {
                    var resultBytes = Convert.FromBase64String(resultLog.Substring(resultTag.Length));
                    var resultJson = Encoding.UTF8.GetString(resultBytes);

                    var response = this._jsonSerializer.Deserialize<RemoteExecutionResponse<TOutput>>(resultJson);

                    if (response != null)
                    {
                        if (!string.IsNullOrEmpty(response.Message))
                            this.Logger.OptiLog(response.Success ? LogLevel.Information : LogLevel.Error, response.Message, response.ErrorCode);

                        if (response.Success)
                            return response.Content;
                    }
                }
            }

            throw new NotImplementedException("Error gestion not done yet");
        }

        #endregion
    }
}
