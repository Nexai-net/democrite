// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Node.Abstractions.Exceptions;
    using Democrite.Framework.Node.Resources;
    using Democrite.Framework.Toolbox;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain in charge to using external code to solve the input. (Script, c++ binary, python, ...) 
    /// </summary>
    /// <seealso cref="VGrainBase{TVGrainInterface}" />
    public sealed class VGrainRemoteArtefactCodeController : VGrainBase<IVGrainRemoteArtefactCodeController>, IVGrainRemoteArtefactCodeController
    {
        #region Fields

        private readonly IExternalCodePackageFactory _externalCodePackageFactory;
        private readonly IArtifactResourceProvider _artifactResourceProvider;
        private readonly ILoggerProvider _loggerProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainRemoteArtefactCodeController"/> class.
        /// </summary>
        public VGrainRemoteArtefactCodeController(ILogger<VGrainRemoteArtefactCodeController> logger,
                                                 IArtifactResourceProvider artifactResourceProvider,
                                                 IExternalCodePackageFactory externalCodePackageFactory,
                                                 ILoggerProvider loggerProvider)
            : base(logger)
        {
            this._loggerProvider = loggerProvider;
            this._artifactResourceProvider = artifactResourceProvider;
            this._externalCodePackageFactory = externalCodePackageFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Fire(IExecutionContext<Guid> executionContext)
        {
            Task.Run(async () => await RunAsync(executionContext).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public Task<TOutput?> RunAsync<TOutput>(IExecutionContext<Guid> executionContext)
        {
            return RunAsync<TOutput, NoneType>(NoneType.Instance, executionContext);
        }

        /// <inheritdoc />
        public Task RunAsync(IExecutionContext<Guid> executionContext)
        {
            return RunAsync<NoneType>(executionContext);
        }

        /// <inheritdoc />
        public async Task<TOutput?> RunAsync<TOutput, TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            var logger = executionContext.GetLogger<VGrainRemoteArtefactCodeController>(this._loggerProvider) ?? NullLogger.Instance;

            var artifactResult = await this._artifactResourceProvider.TryGetFirstValueAsync(executionContext.Configuration);

            if (artifactResult.Result == false || artifactResult.value == null)
                throw new ArtifactMissingException(executionContext.Configuration, nameof(IArtifactCodePackageResource), executionContext);

            var artifactCodePackageResource = artifactResult.value as IArtifactCodePackageResource;

            if (artifactResult.value == null)
                throw new ArtifactInvalidTypeException(executionContext.Configuration, artifactResult.value!.GetType(), typeof(IArtifactCodePackageResource), executionContext);

            var executor = await this._externalCodePackageFactory.BuildAsync(artifactCodePackageResource!, logger, executionContext.CancellationToken);

            await using (executor)
            {
                try
                {
                    // StartAt external program
                    // And ping health check that external could be contacted
                    await executor.StartAsync(executionContext, executionContext.CancellationToken);

                    // Send input through communication protocol
                    // Collect result
                    var result = await executor.AskAsync<TOutput, TInput>(input, executionContext, executionContext.CancellationToken);

                    if (NoneType.IsEqualTo<TOutput>())
                        return (TOutput)(object)NoneType.Instance;

                    if (result is TOutput typedResult)
                        return typedResult;

                    throw new VGrainInvalidOperationDemocriteException(NodeExceptionSR.ExternalInvalidResult.WithArguments(result), executionContext);
                }
                finally
                {
                    // kill external process
                    await executor.StopAsync(executionContext, executionContext.CancellationToken);
                }
            }
        }

        Task IGenericContextedExecutor<Guid>.Fire(IExecutionContext<Guid> executionContext)
        {
            throw new NotImplementedException();
        }

        public Task Fire<TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            throw new NotImplementedException();
        }

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
