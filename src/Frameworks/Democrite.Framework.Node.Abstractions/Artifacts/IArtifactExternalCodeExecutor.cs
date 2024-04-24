// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Artifacts
{
    using Democrite.Framework.Core.Abstractions;

    using Elvex.Toolbox.Abstractions.Disposables;

    using Microsoft.Extensions.Logging;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///  Remote executor and handler of the external code
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public interface IArtifactExternalCodeExecutor : ISafeAsyncDisposable
    {
        #region Methods

        /// <summary>
        /// Starts remote external code.
        /// </summary>
        /// <remarks>
        ///     Give back the hand when the remote process is ready to received command
        /// </remarks>
        ValueTask StartAsync(IExecutionContext executionContext, ILogger logger, CancellationToken cancellationToken);

        /// <summary>
        /// Convert execution information into a command executable by the remote program.
        /// Execute the command and return the result
        /// </summary>
        ValueTask<TOutput?> AskAsync<TOutput, TInput>(TInput? input,
                                                      IExecutionContext executionContext,
                                                      ILogger logger,
                                                      CancellationToken cancellationToken);

        /// <summary>
        /// Stops the remote process
        /// </summary>
        /// <remarks>
        ///     Called also a dispose time
        /// </remarks>
        ValueTask StopAsync(IExecutionContext executionContext, ILogger logger, CancellationToken cancellationToken);

        #endregion
    }
}
