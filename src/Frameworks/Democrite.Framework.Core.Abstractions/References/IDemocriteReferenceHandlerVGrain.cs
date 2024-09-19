// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.References
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Concurrency;

    /// <summary>
    /// VGrain in charge to execute cluster side <see cref="IDemocriteExecutionHandler"/> call with references
    /// </summary>
    /// <remarks>
    ///     Democrite References need to be on cluster side 
    /// </remarks>
    /// <seealso cref="IVGrain" />
    [VGrainStatelessWorker]
    [DemocriteSystemVGrain]
    internal interface IDemocriteReferenceHandlerVGrain : IVGrain, IGrainWithIntegerKey
    {
        #region Methods

        /// <inheritdoc cref="ICommonExecutionLauncher{IExecutionRefLauncher}.Fire" />
        [OneWay]
        [ReadOnly]
        Task Fire<TRefExecCommand>(TRefExecCommand execCommand)
            where TRefExecCommand : IRefExecuteCommand;

        /// <inheritdoc cref="IExecutionRefLauncher.FireWithDeferred{TResult}" />
        [ReadOnly]
        Task<Guid> FireWithDeferred<TResult, TRefExecCommand>(TRefExecCommand execCommand)
            where TRefExecCommand : IRefExecuteCommand;

        /// <inheritdoc cref="IExecutionLauncher.RunAsync(GrainCancellationToken)" />
        [ReadOnly]
        Task<IExecutionResult> RunAsync<TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token)
            where TRefExecCommand : IRefExecuteCommand;

        /// <inheritdoc cref="IExecutionLauncher.RunAsync{TExpectedOutput}(CancellationToken)" />
        [ReadOnly]
        Task<IExecutionResult<TExpectedOutput>> RunAsync<TExpectedOutput, TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token)
            where TRefExecCommand : IRefExecuteCommand;

        /// <inheritdoc cref="IExecutionLauncher.RunWithAnyResultAsync(CancellationToken)" />
        [ReadOnly]
        Task<IExecutionResult> RunWithAnyResultAsync<TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token)
            where TRefExecCommand : IRefExecuteCommand;

        // TODO : Ask Definition Guid based on refId

        #endregion
    }
}
