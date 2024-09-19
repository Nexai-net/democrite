// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Exceptions;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Last step of the trigger process <see cref="IDemocriteExecutionHandler"/> that launch the execution and return the result
    /// </summary>
    public interface ICommonExecutionLauncher<TLauncher>
        where TLauncher : ICommonExecutionLauncher<TLauncher>
    {
        /// <summary>
        /// Specified the execution context responsable for this call
        /// </summary>
        /// <remarks>
        ///     Will tranfert the <see cref="IExecutionContext.FlowUID"/>, <see cref="IExecutionContext.CancellationToken"/>, and Data Context carry
        /// </remarks>
        TLauncher From(IExecutionContext executionContext, bool copyContextData = false);

        /// <summary>
        /// Fires the execution start without waiting any results
        /// </summary>
        Task Fire();
    }

    /// <summary>
    /// Last step of the trigger process <see cref="IDemocriteExecutionHandler"/> that launch the execution and return the result
    /// </summary>
    public interface IExecutionLauncher
    {
        /// <summary>
        /// Trigger execution, execute and provide no result
        /// </summary>
        /// <exception cref="InvalidInputDemocriteException">Raised when input provide is not expected.</exception>
        Task<IExecutionResult> RunAsync(CancellationToken token = default);

        /// <summary>
        /// Trigger execution, execute and provide result in any type
        /// </summary>
        /// <exception cref="InvalidInputDemocriteException">Raised when input provide is not expected.</exception>
        Task<IExecutionResult> RunWithAnyResultAsync(CancellationToken token = default);

        /// <summary>
        /// Trigger execution, execute and provide result.
        /// </summary>
        /// <typeparam name="TExpectedOutput">Expect result</typeparam>
        /// <exception cref="InvalidInputDemocriteException">Raised when input provide is not expected.</exception>
        /// <exception cref="InvalidOutputDemocriteException">Raised when output required doesn't match the execution schema definition.</exception>
        Task<IExecutionResult<TExpectedOutput>> RunAsync<TExpectedOutput>(CancellationToken token = default);

        /// <summary>
        /// Trigger execution, execute and provide result.
        /// </summary>
        /// <param name="expectedOutput">Expect result type</param>
        /// <exception cref="InvalidInputDemocriteException">Raised when input provide is not expected.</exception>
        /// <exception cref="InvalidOutputDemocriteException">Raised when output required doesn't match the execution schema definition.</exception>
        Task<IExecutionResult> RunAsync(Type expectedOutput, CancellationToken token = default);
    }

    /// <summary>
    /// Last step of the trigger process <see cref="IDemocriteExecutionHandler"/> that launch the execution and return the result
    /// </summary>
    public interface IExecutionLauncher<TResult>
    {
        /// <summary>
        /// Trigger execution, execute and provide result.
        /// </summary>
        /// <exception cref="InvalidInputDemocriteException">Raised when input provide is not expected.</exception>
        /// <exception cref="InvalidOutputDemocriteException">Raised when output required doesn't match the execution schema definition.</exception>
        Task<IExecutionResult<TResult>> RunAsync(CancellationToken token = default);
    }

    /// <summary>
    /// Last step of the trigger process <see cref="IDemocriteExecutionHandler"/> that launch the execution and return the result
    /// </summary>
    public interface IExecutionFlowLauncher : ICommonExecutionLauncher<IExecutionFlowLauncher>, IExecutionLauncher
    {
        /// <summary>
        /// Fires the execution start with result pass by deferred
        /// </summary>
        Task<Guid> FireWithDeferred<TResult>();
    }

    /// <summary>
    /// Last step of the trigger process <see cref="IDemocriteExecutionHandler"/> that launch the execution and return the result
    /// </summary>
    public interface IExecutionRefLauncher : IExecutionLauncher
    {
        /// <summary>
        /// Fires the execution start with result pass by deferred
        /// </summary>
        Task<Guid> FireWithDeferred<TResult>();

        /// <summary>
        /// Fires the execution start without waiting any results
        /// </summary>
        Task Fire();
    }

    /// <summary>
    /// Last step of the trigger process <see cref="IDemocriteExecutionHandler"/> that launch the execution and return the result
    /// </summary>
    public interface IExecutionFlowLauncher<TResult> : ICommonExecutionLauncher<IExecutionFlowLauncher<TResult>>, IExecutionLauncher<TResult>
    {
    }

    /// <summary>
    /// Last step of the trigger process <see cref="IDemocriteExecutionHandler"/> that launch the execution and return the result
    /// </summary>
    public interface IExecutionDirectLauncher : ICommonExecutionLauncher<IExecutionDirectLauncher>, IExecutionLauncher
    {
    }

    /// <summary>
    /// Last step of the trigger process <see cref="IDemocriteExecutionHandler"/> that launch the execution and return the result
    /// </summary>
    public interface IExecutionDirectLauncher<TResult> : ICommonExecutionLauncher<IExecutionDirectLauncher<TResult>>, IExecutionLauncher<TResult>
    {
    }
}
