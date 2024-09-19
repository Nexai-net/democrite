// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Executions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.References;

    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Launcher that used the <see cref="IDemocriteReferenceHandlerVGrain"/> as reference solver
    /// </summary>
    internal readonly struct ExecutionRefLauncher<TRefExecCommand> : IExecutionRefLauncher
         where TRefExecCommand : IRefExecuteCommand
    {
        #region Fields
        
        private static readonly MethodInfo s_genericRunWithOutput;

        private readonly IDemocriteReferenceHandlerVGrain _refHandler;
        private readonly TRefExecCommand _refExecCommand;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExecutionRefLauncher{TRefExecCommand}"/> struct.
        /// </summary>
        static ExecutionRefLauncher()
        {
            Expression<Func<ExecutionRefLauncher<TRefExecCommand>, Task<IExecutionResult<int>>>> expr = e => e.RunAsync<int>(default);
            s_genericRunWithOutput = ((MethodCallExpression)expr.Body).Method.GetGenericMethodDefinition();

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionRefLauncher"/> class.
        /// </summary>
        public ExecutionRefLauncher(IGrainFactory factory, in TRefExecCommand refExecCommand)
        {
            // Appy a random to get the executor to distribute but don't have a grain by call
            this._refHandler = factory.GetGrain<IDemocriteReferenceHandlerVGrain>(Random.Shared.Next(0, 42));
            this._refExecCommand = refExecCommand;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task Fire()
        {
            return this._refHandler.Fire(this._refExecCommand);
        }

        /// <inheritdoc />
        public Task<Guid> FireWithDeferred<TResult>()
        {
            return this._refHandler.FireWithDeferred<TResult, TRefExecCommand>(this._refExecCommand);
        }

        /// <inheritdoc />
        public async Task<IExecutionResult> RunAsync(CancellationToken token = default)
        {
            using (var grainCancellation = token.ToGrainCancellationTokenSource())
            {
                return await this._refHandler.RunAsync(this._refExecCommand, grainCancellation.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IExecutionResult<TExpectedOutput>> RunAsync<TExpectedOutput>(CancellationToken token = default)
        {
            using (var grainCancellation = token.ToGrainCancellationTokenSource())
            {
                return await this._refHandler.RunAsync<TExpectedOutput, TRefExecCommand>(this._refExecCommand, grainCancellation.Token);
            }
        }

        /// <inheritdoc />
        public async Task<IExecutionResult> RunAsync(Type expectedOutput, CancellationToken token = default)
        {
            var result = s_genericRunWithOutput.MakeGenericMethod(expectedOutput).Invoke(this, new object?[] { token })! as Task;

            ArgumentNullException.ThrowIfNull(result);
            await result;

            return result.GetResult<IExecutionResult>()!;
        }

        /// <inheritdoc />
        public async Task<IExecutionResult> RunWithAnyResultAsync(CancellationToken token = default)
        {
            using (var grainCancellation = token.ToGrainCancellationTokenSource())
            {
                return await this._refHandler.RunWithAnyResultAsync(this._refExecCommand, grainCancellation.Token);
            }
        }

        #endregion
    }
}
