// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Executions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Models;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Launcher used to call a vgrain method and provide the result
    /// </summary>
    internal abstract class ExecutionBaseLauncher<TVGrain, TResult, TLauncher> : IExecutionLauncher, IExecutionLauncher<TResult>
        where TVGrain : IVGrain
    {
        #region Fields

        private static readonly MethodInfo s_genericCallMethod;

        private readonly IVGrainProvider _vgrainProvider;
        private readonly ILogger _logger;

        private IExecutionContext? _originContext;
        private bool _copyContextData;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExecutionBaseLauncher{TResult}"/> class.
        /// </summary>
        static ExecutionBaseLauncher()
        {
            var trait = typeof(ExecutionBaseLauncher<TVGrain, TResult, TLauncher>);
            var genericCallMethod = trait.GetMethods()
                                         .FirstOrDefault(m => m.Name == nameof(IExecutionLauncher.RunAsync) &&
                                                              m.IsGenericMethod &&
                                                              m.GetParameters().Length == 1 &&
                                                              m.GetParameters().First().ParameterType == typeof(CancellationToken));

            Debug.Assert(genericCallMethod != null);
            s_genericCallMethod = genericCallMethod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionBaseLauncher{TResult}"/> class.
        /// </summary>
        protected ExecutionBaseLauncher(ILogger? logger,
                                        IVGrainProvider vgrainProvider)
        {
            this._vgrainProvider = vgrainProvider;
            this._logger = logger ?? NullLogger.Instance;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        async Task<IExecutionResult> IExecutionLauncher.RunAsync(CancellationToken token)
        {
            return await ((IExecutionLauncher)this).RunAsync<NoneType>(token);
        }

        public Task<IExecutionResult> RunWithAnyResultAsync(CancellationToken token = default)
        {
            return RunImplAsync<IAnyType>(token);
        }

        /// <inheritdoc />
        Task<IExecutionResult<TResult>> IExecutionLauncher<TResult>.RunAsync(CancellationToken token)
        {
            return ((IExecutionLauncher)this).RunAsync<TResult>(token);
        }

        /// <inheritdoc />
        [OneWay]
        public Task Fire()
        {
            return FireImpl<NoneType>();
        }

        /// <inheritdoc />
        public async Task<IExecutionResult<TExpectedOutput>> RunAsync<TExpectedOutput>(CancellationToken token)
        {
            var result = await RunImplAsync<TExpectedOutput>(token);
            return (IExecutionResult<TExpectedOutput>)(result);
        }

        /// <inheritdoc />
        async Task<IExecutionResult> IExecutionLauncher.RunAsync(Type expectedOutput, CancellationToken token)
        {
            var specialisedMethod = s_genericCallMethod.MakeGenericMethod(expectedOutput);

            var result = (ValueTask)specialisedMethod.Invoke(this, new object[] { token })!;

            var task = result.AsTask();
            await task;

            var taskResult = task.GetResult<IExecutionResult>();

            Debug.Assert(taskResult != null);

            return taskResult;
        }

        /// <inheritdoc />
        public TLauncher From(IExecutionContext executionContext, bool copyContextData = false)
        {
            this._originContext = executionContext;
            this._copyContextData = copyContextData;
            return GetLauncher();
        }

        #region Tools

        protected virtual TLauncher GetLauncher()
        {
            if (this is TLauncher launcher)
                return launcher;

            throw new InvalidOperationException("Must be overrided");
        }

        /// <inheritdoc />
        protected Task FireImpl<TExpectedResult>()
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    await RunImplAsync<TExpectedResult>(default, true);
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Error,
                                         "[{context}] Fire Task result on exception : {exception}",
                                         OnFireFailedBuildContext(ex),
                                         ex);
                }
            }).Unwrap();
        }

        /// <summary>
        ///     Generic execution able to call <typeparamref name="TExecutor"/> with diffent parameter
        /// </summary> 
        private async Task<IExecutionResult> RunImplAsync<TExpectedOutput>(CancellationToken token, bool fire = false)
        {
            var executionContext = GenerateExecutionContext(this._originContext?.FlowUID, this._originContext?.CurrentExecutionId);

            if (this._originContext is not null && this._copyContextData == true)
            {
                var data = this._originContext.GetAllDataContext() ?? EnumerableHelper<IContextDataContainer>.ReadOnly;

                foreach (var contextData in data.Where(d => d is not null))
                    executionContext.TryPushContextData(contextData, @override: false);
            }

            var input = GetInput();

            var executor = await this._vgrainProvider.GetVGrainAsync<TVGrain>(input, executionContext, this._logger);

            if (executionContext is IExecutionContextInternal executionContextInternal)
            {
                var grainRef = executor.AsReference<GrainReference>();
                executionContextInternal.AddCancelGrainReference(grainRef);
            }

            Exception? exception = null;
            Task? executionTask = null;
            try
            {
                token.ThrowIfCancellationRequested();

                // Link cancel event to orlean grain cancel system
                token.Register((ctx) => (ctx as IExecutionContext)?.Cancel(), executionContext, useSynchronizationContext: false);

                executionTask = OnRunAsync<TExpectedOutput>(executor, input, executionContext, fire);
                await executionTask;
            }
            catch (Exception ex)
            {
                exception = ex;
                await executionContext.Cancel();
            }

            if (exception is null &&
                exception is not OperationCanceledException &&
                executionTask != null &&
                executionTask.Exception != null)
            {
                exception = executionTask.Exception;
            }

            object? output = default(TExpectedOutput);

            if (executionTask is not null && executionTask.IsCompletedSuccessfully)
            {
                if (AnyType.IsEqualTo<TExpectedOutput>())
                {
                    var result = executionTask?.GetResult();
                    var resultType = result?.GetType() ?? typeof(TExpectedOutput);

                    if (result is IAnyTypeContainer container)
                    {
                        result = container.GetData();
                        resultType = container.GetDataType();
                    }

                    // Call generic ExecutionResult.Create to build result based on the output
                    return ExecutionResult.CreateWithResult(executionContext,
                                                            result,
                                                            resultType,
                                                            exception,
                                                            succeeded: executionTask?.IsCompletedSuccessfully);
                }
                else if (executionTask.IsCompletedSuccessfully && !NoneType.IsEqualTo<TExpectedOutput>() && (executionTask?.IsCompletedSuccessfully ?? false))
                {
                    output = executionTask.GetResult<TExpectedOutput>();
                }
            }

            return ExecutionResult.Create<TExpectedOutput>(executionContext,
                                                           (TExpectedOutput?)output,
                                                           exception,
                                                           succeeded: executionTask?.IsCompletedSuccessfully);
        }

        /// <summary>
        /// Gets the input based on configuration.
        /// </summary>
        protected abstract object? GetInput();

        /// <summary>
        /// Generates <see cref="IExecutionContext"/> based on configuration.
        /// </summary>
        protected abstract IExecutionContext GenerateExecutionContext(Guid? flowId, Guid? parentId);

        /// <summary>
        /// Called to call the requested vgrain
        /// </summary>
        /// <returns></returns>
        protected abstract Task OnRunAsync<TExpectedOutput>(TVGrain executor, object? input, IExecutionContext executionContext, bool fire);

        /// <summary>
        /// Called when build context information to help identify the element that failed
        /// </summary>
        protected abstract string? OnFireFailedBuildContext(Exception ex);

        #endregion

        #endregion
    }
}
