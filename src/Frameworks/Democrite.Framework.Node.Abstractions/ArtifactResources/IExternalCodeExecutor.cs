// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///  Remote executor and handler of the external code
    /// </summary>
    /// <seealso cref="System.IAsyncDisposable" />
    public interface IExternalCodeExecutor : IAsyncDisposable
    {
        #region Methods

        /// <summary>
        /// Starts remote external code.
        /// </summary>
        /// <remarks>
        ///     Give back the hand when the remote process is ready to received command
        /// </remarks>
        ValueTask StartAsync(IExecutionContext executionContext, CancellationToken cancellationToken);

        /// <summary>
        /// Convert execution information into a command executable by the remote program.
        /// Execute the command and return the result
        /// </summary>
        ValueTask<TOutput?> AskAsync<TOutput, TInput>(TInput? input,
                                                      IExecutionContext executionContext,
                                                      CancellationToken cancellationToken);

        /// <summary>
        /// Stops the remote process
        /// </summary>
        /// <remarks>
        ///     Called also a dispose time
        /// </remarks>
        Task StopAsync(IExecutionContext executionContext, CancellationToken cancellationToken);

        #endregion
    }
}
