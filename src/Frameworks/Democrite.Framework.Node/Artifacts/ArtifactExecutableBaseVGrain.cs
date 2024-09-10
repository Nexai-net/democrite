// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Node.Resources;
    using Elvex.Toolbox;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base VGrain in charge to using external code to solve the input. (Script, c++ binary, python, ...) 
    /// </summary>
    /// <seealso cref="VGrainBase{TVGrainInterface}" />
    public abstract class ArtifactExecutableBaseVGrain<TGrainInterface> : VGrainBase<TGrainInterface>
        where TGrainInterface : IVGrain
    {
        #region Fields

        private readonly IArtifactExecutorFactory _artifactExecutorFactory;
        private readonly IArtifactDefinitionProvider _artifactProvider;
        private readonly ILoggerFactory _loggerFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutableBaseVGrain"/> class.
        /// </summary>
        protected ArtifactExecutableBaseVGrain(ILogger<TGrainInterface> logger,
                                               IArtifactDefinitionProvider artifactProvider,
                                               IArtifactExecutorFactory artifactExecutorFactory,
                                               ILoggerFactory loggerFactory)
            : base(logger)
        {
            this._loggerFactory = loggerFactory;
            this._artifactProvider = artifactProvider;
            this._artifactExecutorFactory = artifactExecutorFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="IGenericContextedExecutor{}.Fire" />
        protected void Fire(IExecutionContext executionContext)
        {
            Task.Run(async () => await RunAsync(executionContext).ConfigureAwait(false));
        }

        /// <inheritdoc cref="IGenericContextedExecutor{}.RunAsync" />
        protected Task<TOutput?> RunAsync<TOutput>(IExecutionContext executionContext)
        {
            return RunAsync<TOutput, NoneType>(NoneType.Instance, executionContext);
        }

        /// <inheritdoc cref="IGenericContextedExecutor{}.RunAsync" />
        protected Task RunAsync(IExecutionContext executionContext)
        {
            return RunAsync<NoneType>(executionContext);
        }

        /// <inheritdoc cref="IGenericContextedExecutor{}.RunAsync" />
        protected async Task<TOutput?> RunAsync<TOutput, TInput>(TInput? input, IExecutionContext executionContext)
        {
            var logger = executionContext.GetLogger(this._loggerFactory, this.GetType()) ?? NullLogger.Instance;

            var artifactId = GetArtifactId(input, executionContext);

            var artifactResult = await this._artifactProvider.TryGetFirstValueAsync(artifactId, executionContext.CancellationToken);

            if (artifactResult.Result == false || artifactResult.value == null)
                throw new ArtifactMissingException(artifactId, nameof(ArtifactExecutableDefinition), executionContext);

            var artifactCodePackageResource = artifactResult.value as ArtifactExecutableDefinition;

            if (artifactResult.value == null)
                throw new ArtifactInvalidTypeException(artifactId, artifactResult.value!.GetType(), typeof(ArtifactExecutableDefinition), executionContext);

            var executor = await this._artifactExecutorFactory.BuildAsync(artifactCodePackageResource!,
                                                                          executionContext,
                                                                          logger,
                                                                          executionContext.CancellationToken);

            try
            {
                if (artifactCodePackageResource?.Verbose == ArtifactExecVerboseEnum.Full)
                    logger.OptiLog(LogLevel.Information, "[External VGrain] {app} starting ...", artifactCodePackageResource.DisplayName);

                // StartAt external program
                // And ping health check that external could be contacted
                await executor.StartAsync(executionContext, logger, executionContext.CancellationToken);

                // Send input through communication protocol
                // Collect result
                var result = await executor.AskAsync<TOutput, TInput>(input, executionContext, logger, executionContext.CancellationToken);

                if (NoneType.IsEqualTo<TOutput>())
                    return (TOutput)(object)NoneType.Instance;

                if (result is TOutput typedResult)
                    return typedResult;

                throw new VGrainInvalidOperationDemocriteException(NodeExceptionSR.ExternalInvalidResult.WithArguments(result), executionContext);
            }
            finally
            {
                // kill external process
                await executor.StopAsync(executionContext, logger, executionContext.CancellationToken);
            }
        }

        /// <summary>
        /// Gets the artifact identifier.
        /// </summary>
        protected abstract Guid GetArtifactId<TInput>(TInput? input, IExecutionContext executionContext);

        #region Tools

        /// <inheritdoc />
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(reason, cancellationToken);
        }

        #endregion

        #endregion
    }
}
