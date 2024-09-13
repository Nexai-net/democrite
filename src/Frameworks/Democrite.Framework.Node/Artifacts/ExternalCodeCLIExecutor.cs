// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Disposables;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Executor used to handled and controlled a remote virtual grain in one shot.
    /// Argument are pass by command line
    /// </summary>
    /// <seealso cref="SafeAsyncDisposable" />
    /// <seealso cref="IArtifactExternalCodeExecutor" />
    public class ExternalCodeCLIExecutor : ExternalCodeBaseExecutor, IArtifactExternalCodeExecutor
    {
        #region Fields

        private static readonly Regex s_logTypeRegex = new Regex("^LOG:(?<level>[0-6]{1}|Error|Warning|Trace|Debug|Information|Critical):(?<message>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly IProcessSystemService _processSystemService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCodeCLIExecutor"/> class.
        /// </summary>
        public ExternalCodeCLIExecutor(ArtifactExecutableDefinition artifactExecutableDefinition,
                                       IProcessSystemService processSystemService,
                                       IJsonSerializer jsonSerializer,
                                       IConfiguration configuration,
                                       Uri? workingDirectory)
            : base(artifactExecutableDefinition, jsonSerializer, configuration, workingDirectory)
        {
            this._processSystemService = processSystemService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override async ValueTask<TOutput?> AskAsync<TOutput, TInput>(TInput? input,
                                                                                   IExecutionContext executionContext,
                                                                                   ILogger logger,
                                                                                   CancellationToken cancellationToken)
            where TOutput : default
            where TInput : default
        {
            var base64Cmd = FormatCommand(input, executionContext);

            ExtractCommandLineExec(out var executor, out var args);
            JoinArgument(args, "--cmd", "'" + base64Cmd + "'");

            using (var processor = await LaunchProcessAsync(executor, args, this.ArtifactExecutableDefinition, cancellationToken))
            {
                await processor.GetAwaiterTask();

                var resultTag = executionContext.CurrentExecutionId + ":";

                var logs = new List<string>(42);
                var errorLogs = new List<string>(42);

                string resultLog = string.Empty;
                foreach (var log in processor.StandardOutput ?? EnumerableHelper<string>.ReadOnly)
                {
                    if (log.StartsWith(resultTag, StringComparison.OrdinalIgnoreCase))
                    {
                        resultLog = ExtractResultFromLog(log, resultTag.Length);
                        continue;
                    }

                    if (ProcessLogMessage(log, logger))
                        continue;

                    logs.Add(log);
                }

                foreach (var log in processor.ErrorOutput ?? EnumerableHelper<string>.ReadOnly)
                {
                    if (ProcessLogMessage(log, logger))
                        continue;

                    errorLogs.Add(log);
                }

                return ManagedClientResult<TOutput>(resultLog,
                                                    executionContext,
                                                    errorLogs,
                                                    logs,
                                                    logger);
            }
        }

        /// <summary>
        /// Launches the process.
        /// </summary>
        protected virtual async Task<IExternalProcess> LaunchProcessAsync(string executor, List<string> args, ArtifactExecutableDefinition definition, CancellationToken token)
        {
            return await this._processSystemService.StartAsync(executor,
                                                               this.WorkingDir!.LocalPath,
                                                               token,
                                                               args.Select(s => Encoding.UTF8.GetString(Encoding.Default.GetBytes(s))).ToArray());
        }

        /// <summary>
        /// Processes the log message format.
        /// </summary>
        private bool ProcessLogMessage(string log, ILogger logger)
        {
            var match = s_logTypeRegex.Match(log);
            if (match.Success)
            {
                var level = match.Groups["level"].Value;
                var message = match.Groups["message"].Value;

                var logLevel = LogLevel.Information;
                if (!string.IsNullOrEmpty(level))
                {
                    if (int.TryParse(level, out int result) && result >= (int)LogLevel.Trace && result < (int)LogLevel.None)
                        logLevel = (LogLevel)result;
                    else if (Enum.TryParse<LogLevel>(level, true, out var strLevel) && strLevel != LogLevel.None)
                        logLevel = strLevel;
                }

                ManagedExecutionMessage("LOG", logLevel, message, logger);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Extracts the result from log between quote
        /// </summary>
        private string ExtractResultFromLog(in string resultLog, int tagLenght)
        {
            ReadOnlySpan<char> results = resultLog;

            results = results.Slice(tagLenght);

            var quoteIndexOf = results.IndexOf('\'');
            if (quoteIndexOf > -1)
                results = results.Slice(quoteIndexOf).Trim().Trim('\'');
            return results.ToString();
        }

        #endregion
    }
}
