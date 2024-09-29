// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Models;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.Models;
    using Democrite.Framework.Node.Services;
    using Democrite.Framework.Node.ThreadExecutors;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Excutor in charge of a specific sequence
    /// </summary>
    /// <seealso cref="ISequenceExecutorVGrain" />
    [DemocriteSystemVGrain]
    internal sealed class SequenceExecutorVGrain : VGrainBase<SequenceExecutorState, SequenceExecutorStateSurrogate, SequenceExecutorStateConverter, ISequenceExecutorVGrain>, ISequenceExecutorVGrain
    {
        #region Fields

        private static readonly MethodInfo s_generateResultStruct;

        private readonly ISequenceVGrainProviderFactory _sequenceVGrainProviderFactory;
        private readonly IVGrainDemocriteSystemProvider _grainDemocriteSystemProvider;
        private readonly IOptionsMonitor<ClusterNodeRuntimeOptions> _runtimeOptions;
        private readonly ISequenceDefinitionProvider _sequenceDefinitionManager;
        private readonly ISequenceExecutorThreadStageProvider _stageProvider;
        private readonly IDemocriteSerializer _democriteSerializer;
        private readonly IDiagnosticLogger _diagnosticLogger;
        private readonly IObjectConverter _objectConverter;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISignalService _signalService;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SequenceExecutorVGrain"/> class.
        /// </summary>
        static SequenceExecutorVGrain()
        {
            Expression<Func<SequenceExecutorVGrain, SequenceExecutorState, IExecutionResult>> generateResultStruct = (g, s) => g.GenerateResultStruct<NoneType>(s);
            s_generateResultStruct = ((MethodCallExpression)generateResultStruct.Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceVGrain"/> class.
        /// </summary>
        public SequenceExecutorVGrain(ILogger<SequenceExecutorVGrain> logger,
                                      ILoggerFactory loggerFactory,
                                      IDiagnosticLogger diagnosticLogger,
                                      ISequenceDefinitionProvider sequenceDefinitionManager,
                                      [PersistentState("SequenceExecutor", DemocriteConstants.DefaultDemocriteStateConfigurationKey)] IPersistentState<SequenceExecutorStateSurrogate> sequenceExecutorState,
                                      ITimeManager timeManager,
                                      IObjectConverter objectConverter,
                                      IDemocriteSerializer democriteSerializer,
                                      ISequenceExecutorThreadStageProvider stageProvider,
                                      ISequenceVGrainProviderFactory sequenceVGrainProviderFactory,
                                      ISignalService signalService,
                                      IOptionsMonitor<ClusterNodeRuntimeOptions> runtimeOptions,
                                      IVGrainDemocriteSystemProvider grainDemocriteSystemProvider)
            : base(logger, sequenceExecutorState)
        {
            this._sequenceVGrainProviderFactory = sequenceVGrainProviderFactory;
            this._grainDemocriteSystemProvider = grainDemocriteSystemProvider;
            this._sequenceDefinitionManager = sequenceDefinitionManager;
            this._democriteSerializer = democriteSerializer;
            this._diagnosticLogger = diagnosticLogger;
            this._objectConverter = objectConverter;
            this._runtimeOptions = runtimeOptions;
            this._loggerFactory = loggerFactory;
            this._stageProvider = stageProvider;
            this._signalService = signalService;
            this._timeManager = timeManager;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<TOutput?> RunAsync<TOutput, TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            // Get state
            var state = base.State;

            if (state == null)
            {
                // If it doesn't came from a silo the activation chain events are not called
                // Allow direct call (unit test or else) to load context state
                await base.OnActivationSetupState(default);
                state = base.State;
            }

            var needStateSaving = false;
            if (state == null || state.SequenceDefinitionId == Guid.Empty)
            {
                var newState = new SequenceExecutorState(executionContext.Configuration,
                                                         executionContext.Configuration.ToString(),
                                                         executionContext.FlowUID,
                                                         this._timeManager.UtcNow);

                needStateSaving = true;
                //await base.PushStateAsync(newState, default);

                state = newState;
            }

            var defaultResult = default(TOutput);

            if (NoneType.IsEqualTo<TOutput>())
                defaultResult = (TOutput)(object)NoneType.Instance;
            else if (AnyType.IsEqualTo<TOutput>())
                defaultResult = (TOutput)(object)AnyType.Instance;

            ArgumentNullException.ThrowIfNull(state);

            var execLogger = executionContext.GetLogger<SequenceExecutorVGrain>(this._loggerFactory);

            var execCancelToken = executionContext.CancellationToken;

            try
            {
                var sequenceDefintion = await this._sequenceDefinitionManager.GetByKeyAsync(state.SequenceDefinitionId, execCancelToken);

                if (sequenceDefintion is null)
                    throw new MissingDefinitionException(typeof(SequenceDefinition), state.SequenceDefinitionId.ToString());

                execLogger.OptiLog(sequenceDefintion.Options.Diagnostic.MinLogLevel,
                                   "Start sequence '{sequenceDefinitionDisplayName}' (Id: {sequenceDefinitionUID})",
                                   sequenceDefintion.DisplayName,
                                   sequenceDefintion.Uid);

                // Loop to navigate through pipeline stages
                var sequencesDone = false;

                try
                {
                    if (state.MainThread == null)
                        state.Initialize(executionContext, sequenceDefintion, input, this._democriteSerializer);

                    // Disabled state save to reduce saving of specific sequence
                    if (sequenceDefintion.Options.PreventSequenceExecutorStateStorage || 
                        (state.Customization?.PreventSequenceExecutorStateStorage ?? false) ||
                        (this._runtimeOptions.CurrentValue?.BlockSequenceExecutorStateStorageByDefault ?? false))
                    {
                        this.StateStorageEnabled = false;
                    }

                    if (needStateSaving && this.StateStorageEnabled)
                        await PushStateAsync(state, default);

                    Debug.Assert(state.MainThread != null);

                    var mainExecutionThread = await SequenceExecutorExecThread.BuildFromAsync(state.MainThread,
                                                                                              id => this._sequenceDefinitionManager.GetByKeyAsync(id, execCancelToken),
                                                                                              this._stageProvider,
                                                                                              this._objectConverter,
                                                                                              this._timeManager,
                                                                                              this._democriteSerializer,
                                                                                              state,
                                                                                              executionContext);

                    using (var container = this._sequenceVGrainProviderFactory.GetProvider(state.Customization))
                    {
                        do
                        {
                            Debug.Assert(state.MainThread != null, "MUST be setup by state.Initialize(input)");

                            await (mainExecutionThread.GetThreadStageExecutionTasksAsync(execLogger, container.Content, this._diagnosticLogger) ?? Task.CompletedTask);

                            // Push each time to allow recovery on crash and dashboard follow up
                            await PushStateAsync(state, mainExecutionThread.ExecutionContext.CancellationToken);

                            if (state.MainThread.JobDone)
                            {
                                var customization = state.Customization;
                                if (customization is not null && 
                                    ((customization.Value.SignalFireDescriptions is not null && customization.Value.SignalFireDescriptions.Any()) || 
                                      customization.Value.DeferredId is not null))
                                {
                                    var resultFormat = GenerateResultStruct<TOutput>(state);

                                    if (customization.Value.SignalFireDescriptions is not null && customization.Value.SignalFireDescriptions.Any())
                                    {
                                        foreach (var signal in customization.Value.SignalFireDescriptions)
                                        {
                                            if (signal.IncludeResult == false)
                                                await this._signalService.Fire(signal.SignalId, default, this);
                                            else
                                                await this._signalService.Fire(signal.SignalId, resultFormat, default, this);
                                        }
                                    }

                                    if (customization.Value.DeferredId is not null)
                                    {
                                        var deferredHandlerGrain = await this._grainDemocriteSystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this.Logger);
                                        await deferredHandlerGrain.FinishDeferredWorkWithResultAsync(customization.Value.DeferredId.Value.Uid, this.IdentityCard, resultFormat);
                                    }
                                }

                                if (state.MainThread.Exception != null)
                                    throw state.MainThread.Exception;

                                if (!string.IsNullOrEmpty(state.MainThread.ErrorMessage))
                                    throw new SequenceExecutionException(state.MainThread.ErrorMessage);

                                var output = state.MainThread.Output;
                                if (NoneType.IsEqualTo<TOutput>() == false)
                                {
                                    if (output is TOutput castOutput)
                                        return castOutput;

                                    if (AnyType.IsEqualTo<TOutput>())
                                    {
                                        var anyTypeContainer = AnyType.CreateContainer(output, output?.GetType() ?? sequenceDefintion.Output?.ToType() ?? typeof(object));
                                        if (anyTypeContainer is TOutput castAnyTypeContainer)
                                            return castAnyTypeContainer;

                                        throw new InvalidCastException($"Couldn't cast {anyTypeContainer} to {typeof(TOutput)}");
                                    }
                                    //return (TOutput) castOutput;

                                    execLogger.OptiLog(LogLevel.Error,
                                                       "Invalid output [Expected: {expectedType}] != [Get {getType}] : Details {details}",
                                                        typeof(TOutput),
                                                        output?.GetType(),
                                                        output);

                                    return defaultResult;
                                }

                                break;
                            }
                        } while (sequencesDone == false);
                    }
                }
                catch (Exception ex)
                {
                    execLogger.OptiLog(LogLevel.Critical,
                                       "[{sequenceDefinitionDisplayName}-{sequenceDefinitionUID}] Sequence execution crash {message} - State : {stateDisplayName} - {exception}",
                                       sequenceDefintion.DisplayName,
                                       sequenceDefintion.Uid,
                                       ex.Message,
                                       state.ToDebugDisplayName(),
                                       ex);

                    if (ex is OperationCanceledException)
                        throw;

                    throw new SequenceExecutionException(ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                execLogger.OptiLog(LogLevel.Critical,
                                   "[{flowUID}-{sequenceDefinitionUID}] Execution fail : {exception}",
                                   executionContext.FlowUID,
                                   executionContext.Configuration,
                                   ex);
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(this.RuntimeIdentity))
                    DeactivateOnIdle();
            }

            return defaultResult;
        }

        /// <inheritdoc />
        public Task RunWithInputAsync<TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return RunAsync<NoneType, TInput>(input, executionContext);
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
        [OneWay]
        public Task Fire(IExecutionContext<Guid> executionContext)
        {
            return RunAsync(executionContext);
        }

        /// <inheritdoc />
        [OneWay]
        public Task Fire<TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return RunAsync<NoneType, TInput>(input, executionContext);
        }

        #region Tools

        /// <summary>
        /// Generates the result structure.
        /// </summary>
        private IExecutionResult GenerateResultStruct<TOutput>(SequenceExecutorState state)
        {
            Exception? exception = null;
            TOutput? outputCast = default;

            if (state.MainThread!.Exception != null)
                exception = state.MainThread.Exception;
            else if (!string.IsNullOrEmpty(state.MainThread.ErrorMessage))
                exception = new SequenceExecutionException(state.MainThread.ErrorMessage);

            var output = state.MainThread.Output;

            if (output is TOutput castOutput)
            {
                outputCast = castOutput;
            }
            else if (output is not null) // !NoneType.IsEqualTo<TOutput>() && 
            {
                return (IExecutionResult)s_generateResultStruct.MakeGenericMethodWithCache(output.GetType()).Invoke(this, new[] { state })!;
            }

            return new ExecutionResultStruct<TOutput>(outputCast,
                                                      state.FlowUid,
                                                      exception is null,
                                                      exception is OperationCanceledException,

                                                      (exception is IDemocriteException democriteException)
                                                            ? (string?)democriteException.ErrorCode.ToString()
                                                            : (string?)null,

                                                      exception?.GetFullString(),
                                                      !NoneType.IsEqualTo<TOutput>(),
                                                      typeof(TOutput).FullName);
        }

        #endregion

        #endregion
    }
}
