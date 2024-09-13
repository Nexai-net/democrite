// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Core.Abstractions.Configurations;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Node.Models;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using System;
    using System.Linq.Expressions;
    using System.Reflection;
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
        #region Fields

        private static readonly MethodInfo s_solveConfigurationValue;
        private readonly IConfiguration _configuration;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExternalCodeBaseExecutor"/> class.
        /// </summary>
        static ExternalCodeBaseExecutor()
        {
            //private static (string StrValue, bool UseBase64) SolveConfigurationValue<TData>(ConfigurationBaseDefinition cfg)
            Expression<Func<ExternalCodeBaseExecutor, ConfigurationBaseDefinition, Tuple<string, bool>>> solveConfigurationValue = (t, c) => t.SolveConfigurationValue<int>(c);
            s_solveConfigurationValue = ((MethodCallExpression)solveConfigurationValue.Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCodeCLIExecutor"/> class.
        /// </summary>
        public ExternalCodeBaseExecutor(ArtifactExecutableDefinition artifactExecutableDefinition,
                                        IJsonSerializer jsonSerializer,
                                        IConfiguration configuration,
                                        Uri? workingDirectory)
        {
            this.ArtifactExecutableDefinition = artifactExecutableDefinition;
            this._configuration = configuration;
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
        public Uri? WorkingDir { get; }

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
        /// Formats the input.
        /// </summary>
        protected string FormatInput<TInput>(TInput? input)
        {
            byte[] inputSerialized;

            var typeInfo = typeof(TInput).GetTypeInfoExtension();
            if (typeInfo.IsCSharpScalarType)
                inputSerialized = Encoding.UTF8.GetBytes(input?.ToString() ?? string.Empty);
            else
                inputSerialized = this.JsonSerializer.Serialize(input);
            var inputBase64 = Convert.ToBase64String(inputSerialized);
            return inputBase64;
        }

        /// <summary>
        /// Formats the input.
        /// </summary>
        protected string FormatCommand<TInput>(TInput? input, IExecutionContext executionContext)
        {
            var inputJsonBase64 = FormatInput(input);

            var command = new RemoteExecutionCommand(executionContext.FlowUID,
                                                     executionContext.CurrentExecutionId,
                                                     inputJsonBase64);

            var cmdJson = this.JsonSerializer.Serialize(command);
            var base64Cmd = Convert.ToBase64String(cmdJson);
            return base64Cmd;
        }

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
                    if (jobj.Remove(nameof(RemoteExecutionResponse.Content)))
                    {
                        resultJson = jobj.ToString();
                    }
                }

                var response = this.JsonSerializer.Deserialize<RemoteExecutionResponse>(resultJson);

                if (response != null)
                {
                    if (!string.IsNullOrEmpty(response.Message))
                        logger.OptiLog(response.Success ? LogLevel.Information : LogLevel.Error, response.Message, response.ErrorCode);

                    if (response.Success)
                    {
                        if (NoneType.IsEqualTo<TOutput>())
                            return (TOutput)(object)NoneType.Instance;

                        if (!string.IsNullOrEmpty(response.Content))
                        {
                            var resultContentBytes = Convert.FromBase64String(response.Content);
                            var resultContentJson = Encoding.UTF8.GetString(resultContentBytes);

                            return this.JsonSerializer.Deserialize<TOutput>(resultContentJson);
                        }
                        return default;
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
            var def = this.ArtifactExecutableDefinition;
            arguments = new List<string>(10);
            executor = def.ExecutablePath;

            var executorArgumentSeparator = def.ExecutorArgumentSeparator ?? ':';

            if (!string.IsNullOrEmpty(def.Executor))
            {
                executor = def.Executor;
                var indexOfSeparator = executor.IndexOf(":");
                if (indexOfSeparator > -1)
                    executor = executor.Substring(0, indexOfSeparator);

                arguments.AddRange(def.ExecutorArguments);

                arguments.Add(def.ExecutablePath);
            }

            if (def.Arguments != null && def.Arguments.Any())
            {
                foreach (var arg in def.Arguments)
                    arguments.Add(arg);
            }

            if (def.Verbose != ArtifactExecVerboseEnum.Minimal)
                JoinArgument(arguments, "--verbose", def.Verbose.ToString());

            if (def.Configurations is not null)
            {
                foreach (var cfg in def.Configurations)
                {
                    var cfgKvValue = (Tuple<string, bool>)(s_solveConfigurationValue.MakeGenericMethodWithCache(cfg.ExpectedConfigurationType.ToType())
                                                               .Invoke(this, new object[] { cfg }))!;

                    JoinArgument(arguments, "--config" + ((cfgKvValue.Item2) ? "_b64" : ""), cfg.ConfigName + "='" + cfgKvValue.Item1 + "'");
                }
            }
        }

        protected void JoinArgument(IList<string> args, string key, string value)
        {
            var executorArgumentSeparator = this.ArtifactExecutableDefinition.ExecutorArgumentSeparator ?? ':';
            if (executorArgumentSeparator == ' ')
            {
                args.Add(key);
                args.Add(value);
            }
            else
            {
                args.Add(key + executorArgumentSeparator + value);
            }
        }

        /// <summary>
        /// Solves the configuration value.
        /// </summary>
        private Tuple<string, bool> SolveConfigurationValue<TData>(ConfigurationBaseDefinition cfg)
        {
            TData? data = default;
            if (cfg is ConfigurationDirectDefinition<TData> direct)
            {
                data = direct.Data;
            }
            else if (cfg is ConfigurationFromSectionPathDefinition<TData> fromCfg)
            {
                data = this._configuration.GetSection(fromCfg.SectionPath).Get<TData>() ?? fromCfg.DefaultData;
            }

            var type = typeof(TData).GetTypeInfoExtension();

            if (data is null || type.IsCSharpScalarType)
                return Tuple.Create(data?.ToString() ?? string.Empty, false);

            var jsonData = this.JsonSerializer.Serialize<TData>(data);
            return Tuple.Create(Convert.ToBase64String(jsonData), true);
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
