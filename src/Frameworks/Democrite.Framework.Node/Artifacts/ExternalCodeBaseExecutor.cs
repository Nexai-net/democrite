// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Node.Models;
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base implementation of common part Executor used to handled and controlled a remote virtual grain
    /// </summary>
    /// <seealso cref="SafeAsyncDisposable" />
    /// <seealso cref="IArtifactExternalCodeExecutor" />
    public abstract class ExternalCodeBaseExecutor : SafeAsyncDisposable, IArtifactExternalCodeExecutor
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCodeCLIExecutor"/> class.
        /// </summary>
        public ExternalCodeBaseExecutor(ArtifactExecutableDefinition artifactExecutableDefinition,
                                        IJsonSerializer jsonSerializer,
                                        Uri workingDirectory)
        {
            this.ArtifactExecutableDefinition = artifactExecutableDefinition;
            this.WorkingDir = workingDirectory;
            this.JsonSerializer = jsonSerializer;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the artifact executable definition.
        /// </summary>
        public ArtifactExecutableDefinition ArtifactExecutableDefinition { get; }

        /// <summary>
        /// Gets the working dir absolute path.
        /// </summary>
        public Uri WorkingDir { get; }

        /// <summary>
        /// Gets the json serializer.
        /// </summary>
        public IJsonSerializer JsonSerializer { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <remarks>
        /// Give back the hand when the remote process is ready to received command
        /// </remarks>
        public virtual ValueTask StartAsync(IExecutionContext executionContext, ILogger logger, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public abstract ValueTask<TOutput?> AskAsync<TOutput, TInput>(TInput? input,
                                                                      IExecutionContext executionContext,
                                                                      ILogger logger,
                                                                      CancellationToken cancellationToken);

        /// <inheritdoc />
        public virtual ValueTask StopAsync(IExecutionContext executionContext, ILogger logger, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        #region Tools

        /// <summary>
        /// Manageds the client result.
        /// </summary>
        protected TOutput? ManagedClientResult<TOutput>(string? resultBase64,
                                                        IExecutionContext executionContext,
                                                        IReadOnlyCollection<string>? errorMessages,
                                                        IReadOnlyCollection<string>? logMessages,
                                                        ILogger logger)
        {
            if (logMessages is not null && logMessages.Any())
            {
                logger.OptiLog(LogLevel.Information, "{logs}", logMessages.AggregateStrings());
            }

            string errorMessagesAggr = string.Empty;

            if (errorMessages != null && errorMessages.Any())
            {
                errorMessagesAggr = errorMessages.AggregateStrings();
                logger.OptiLog(LogLevel.Error, "External Code {artifactUid} : {error}", this.ArtifactExecutableDefinition.Uid, errorMessagesAggr);
            }

            // Managed result
            if (!string.IsNullOrEmpty(resultBase64))
            {
                var resultBytes = Convert.FromBase64String(resultBase64);
                var resultJson = Encoding.UTF8.GetString(resultBytes);

                if (NoneType.IsEqualTo<TOutput>())
                {
                    var jobj = JObject.Parse(resultJson);
                    if (jobj.Remove(nameof(RemoteExecutionResponse<NoneType>.Content)))
                    {
                        resultJson = jobj.ToString();
                    }
                }

                var response = this.JsonSerializer.Deserialize<RemoteExecutionResponse<TOutput>>(resultJson);

                if (response != null)
                {
                    if (!string.IsNullOrEmpty(response.Message))
                        logger.OptiLog(response.Success ? LogLevel.Information : LogLevel.Error, response.Message, response.ErrorCode);

                    if (response.Success)
                    {
                        if (NoneType.IsEqualTo<TOutput>())
                            return (TOutput)(object)NoneType.Instance;
                        return response.Content;
                    }

                    throw new ArtifactExecutionException(this.ArtifactExecutableDefinition.Uid, response.Message, executionContext);
                }
            }

            throw new ArtifactExecutionException(this.ArtifactExecutableDefinition.Uid, errorMessagesAggr, executionContext);
        }

        /// <summary>
        /// Extracts the command line execute parameters
        /// </summary>
        protected void ExtractCommandLineExec(out string executor, out List<string> arguments)
        {
            arguments = new List<string>(5);
            executor = this.ArtifactExecutableDefinition.ExecutablePath;

            if (!string.IsNullOrEmpty(this.ArtifactExecutableDefinition.Executor))
            {
                executor = this.ArtifactExecutableDefinition.Executor;
                var indexOfSeparator = executor.IndexOf(":");
                if (indexOfSeparator > -1)
                    executor = executor.Substring(0, indexOfSeparator);

                arguments.Add(this.ArtifactExecutableDefinition.ExecutablePath);
            }

            if (this.ArtifactExecutableDefinition.Arguments != null && this.ArtifactExecutableDefinition.Arguments.Any())
            {
                foreach (var arg in this.ArtifactExecutableDefinition.Arguments)
                    arguments.Add(arg);
            }
        }

        /// <summary>
        /// Manageds message from external executed.
        /// </summary>
        protected void ManagedExecutionMessage(string? type, LogLevel level, string? message, ILogger logger)
        {
            if (string.Equals(type, "LOG", StringComparison.OrdinalIgnoreCase))
            {
                logger.OptiLog(level, "{log}", message);
                return;
            }

            logger.OptiLog(LogLevel.Warning, "Message type '{type}' is not supported yet : {message}", type, message);
        }

        #endregion

        #endregion
    }
}
