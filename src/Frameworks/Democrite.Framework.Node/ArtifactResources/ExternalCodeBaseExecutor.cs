// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Node.Abstractions.Exceptions;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base implementation of common part Executor used to handled and controlled a remote virtual grain
    /// </summary>
    /// <seealso cref="SafeAsyncDisposable" />
    /// <seealso cref="IExternalCodeExecutor" />
    public abstract class ExternalCodeBaseExecutor : SafeAsyncDisposable, IExternalCodeExecutor
    {
        #region Fields

        private readonly IReadOnlyCollection<IExternalCodeExecutorPreparationStep> _preparationSteps;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCodeCLIExecutor"/> class.
        /// </summary>
        public ExternalCodeBaseExecutor(IArtifactCodePackageResource artifactCodePackageResource,
                                        IEnumerable<IExternalCodeExecutorPreparationStep> preparationSteps,
                                        ILogger logger)
        {
            this.Logger = logger;
            this.ArtifactCodePackageResource = artifactCodePackageResource;

            this._preparationSteps = preparationSteps?.ToArray() ?? EnumerableHelper<IExternalCodeExecutorPreparationStep>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the artifact code package resource.
        /// </summary>
        protected IArtifactCodePackageResource ArtifactCodePackageResource { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <remarks>
        /// Give back the hand when the remote process is ready to received command
        /// </remarks>
        public async ValueTask StartAsync(IExecutionContext executionContext, CancellationToken cancellationToken)
        {
            foreach (var step in this._preparationSteps)
            {
                try
                {
                    if (!await step.SetupAsync(this.ArtifactCodePackageResource, this.Logger, cancellationToken))
                    {
                        throw new ArtifactPreparationFailedException(step.ToString()!, executionContext);
                    }
                }
                catch (Exception inner)
                {
                    throw new ArtifactPreparationFailedException(step.ToString()!, executionContext, inner);
                }
            }
        }

        /// <inheritdoc />
        public abstract ValueTask<TOutput?> AskAsync<TOutput, TInput>(TInput? input,
                                                                      IExecutionContext executionContext,
                                                                      CancellationToken cancellationToken);

        /// <inheritdoc />
        public async Task StopAsync(IExecutionContext executionContext, CancellationToken cancellationToken)
        {
            foreach (var step in this._preparationSteps)
            {
                try
                {
                    if (!await step.UnsetupAsync(this.ArtifactCodePackageResource, this.Logger, cancellationToken))
                    {
                        throw new ArtifactPreparationFailedException(step.ToString()!, executionContext);
                    }
                }
                catch (Exception inner)
                {
                    throw new ArtifactPreparationFailedException(step.ToString()!, executionContext, inner);
                }
            }
        }

        #endregion
    }
}
