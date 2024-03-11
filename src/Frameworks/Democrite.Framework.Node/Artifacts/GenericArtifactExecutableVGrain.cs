// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Elvex.Toolbox;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain in charge to using external code to solve the input. (Script, c++ binary, python, ...) 
    /// </summary>
    /// <seealso cref="VGrainBase{TVGrainInterface}" />
    public sealed class GenericArtifactExecutableVGrain : ArtifactExecutableBaseVGrain<IGenericArtifactExecutableVGrain>, IGenericArtifactExecutableVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericArtifactExecutableVGrain"/> class.
        /// </summary>
        public GenericArtifactExecutableVGrain(ILogger<IGenericArtifactExecutableVGrain> logger,
                                               IArtifactDefinitionProvider artifactProvider,
                                               IArtifactExecutorFactory artifactExecutorFactory,
                                               ILoggerFactory loggerFactory)
            : base(logger, artifactProvider, artifactExecutorFactory, loggerFactory)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Fire(IExecutionContext<Guid> executionContext)
        {
            Task.Factory.StartNew(async () => await base.RunAsync(executionContext).ConfigureAwait(false));
        }

        Task IGenericContextedExecutor<Guid>.Fire(IExecutionContext<Guid> executionContext)
        {
            return Task.Factory.StartNew(async () => await base.RunAsync(executionContext).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public Task Fire<TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return Task.Factory.StartNew(async () => await base.RunAsync<NoneType, TInput>(input, executionContext).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public Task<TOutput?> RunAsync<TOutput>(IExecutionContext<Guid> executionContext)
        {
            return base.RunAsync<TOutput, NoneType>(NoneType.Instance, executionContext);
        }

        /// <inheritdoc />
        public Task RunAsync(IExecutionContext<Guid> executionContext)
        {
            return base.RunAsync<NoneType>(executionContext);
        }

        /// <inheritdoc />
        public Task<TOutput?> RunAsync<TOutput, TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return base.RunAsync<TOutput, TInput>(input, executionContext);
        }

        /// <inheritdoc />
        public Task RunWithInputAsync<TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return RunAsync<NoneType, TInput>(input, executionContext);
        }

        /// <inheritdoc />
        protected override Guid GetArtifactId<TInput>(TInput? input, IExecutionContext executionContext) 
            where TInput : default
        {
            var fullCtx = executionContext as IExecutionContext<Guid>;
            Debug.Assert(fullCtx != null);
            return fullCtx.Configuration;
        }

        #endregion
    }
}
