// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.Models;
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Abstractions.Models;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    /// Excutor in charge of a specific sequence
    /// </summary>
    /// <seealso cref="ISequenceExecutorVGrain" />
    [Guid("5F886D59-E817-4970-A108-F34A38B87B45")]
    [DemocriteSystemVGrain]
    internal sealed class SequenceExecutorVGrain : VGrainBase<SequenceExecutorState, SequenceExecutorStateSurrogate, SequenceExecutorStateConverter, ISequenceExecutorVGrain>, ISequenceExecutorVGrain
    {
        #region Fields

        private readonly IReadOnlyCollection<ISequenceExecutorThreadStageProvider> _stageProviders;
        private readonly ISequenceDefinitionProvider _sequenceDefinitionManager;
        private readonly IDiagnosticLogger _diagnosticLogger;
        private readonly IObjectConverter _objectConverter;
        private readonly IVGrainProvider _vgrainProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceVGrain"/> class.
        /// </summary>
        public SequenceExecutorVGrain(ILogger<SequenceExecutorVGrain> logger,
                                      ILoggerFactory loggerFactory,
                                      IDiagnosticLogger diagnosticLogger,
                                      ISequenceDefinitionProvider sequenceDefinitionManager,
                                      IVGrainProvider vgrainProvider,
                                      [PersistentState("SequenceExecutor", nameof(Democrite))] IPersistentState<SequenceExecutorStateSurrogate> sequenceExecutorState,
                                      ITimeManager timeManager,
                                      IObjectConverter objectConverter,
                                      IEnumerable<ISequenceExecutorThreadStageProvider>? stageProviders = null)
            : base(logger, sequenceExecutorState)
        {
            this._loggerFactory = loggerFactory;
            this._sequenceDefinitionManager = sequenceDefinitionManager;
            this._diagnosticLogger = diagnosticLogger;
            this._vgrainProvider = vgrainProvider;
            this._stageProviders = stageProviders?.ToReadOnly() ?? EnumerableHelper<ISequenceExecutorThreadStageProvider>.ReadOnlyArray;
            this._timeManager = timeManager;
            this._objectConverter = objectConverter;
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

            if (state == null || state.SequenceDefinitionId == Guid.Empty)
            {
                var newState = new SequenceExecutorState(executionContext.Configuration,
                                                         executionContext.FlowUID,
                                                         this._timeManager.UtcNow);
                await base.PushStateAsync(newState, default);

                state = base.State ?? newState;
            }

            var defaultResult = default(TOutput);

            if (typeof(TOutput) == NoneType.Trait)
                defaultResult = (TOutput)(object)NoneType.Instance;

            ArgumentNullException.ThrowIfNull(state);

            var execLogger = executionContext.GetLogger<SequenceExecutorVGrain>(this._loggerFactory);

            try
            {
                var sequenceDefintion = await this._sequenceDefinitionManager.GetFirstValueByIdAsync(state.SequenceDefinitionId);

                ArgumentNullException.ThrowIfNull(sequenceDefintion);

                execLogger.OptiLog(LogLevel.Information,
                                   "Start sequence '{sequenceDefinitionDisplayName}' (Id: {sequenceDefinitionUID})",
                                   sequenceDefintion.DisplayName,
                                   sequenceDefintion.Uid);

                // Loop to navigate through pipeline stages
                var sequencesDone = false;

                try
                {
                    if (state.MainThread == null)
                        state.Initialize(executionContext, sequenceDefintion, input);
                    //state.Initialize(state.InstanceId, state.FlowUid, sequenceDefintion, input);

                    Debug.Assert(state.MainThread != null);

                    var mainExecutionThread = await SequenceExecutorExecThread.BuildFromAsync(state.MainThread,
                                                                                              id => this._sequenceDefinitionManager.GetFirstValueByIdAsync(id),
                                                                                              this._stageProviders,
                                                                                              this._objectConverter);

                    do
                    {
                        Debug.Assert(state.MainThread != null, "MUST be setup by state.Initialize(input)");

                        await (mainExecutionThread.GetThreadStageExecutionTasks(execLogger, this._vgrainProvider, this._diagnosticLogger) ?? Task.CompletedTask);

                        // Push each time to allow recovery on crash and dashboard follow up
                        await PushStateAsync(state, mainExecutionThread.ExecutionContext.CancellationToken);

                        if (state.MainThread.JobDone)
                        {
                            if (state.MainThread.Exception != null)
                                throw state.MainThread.Exception;

                            if (!string.IsNullOrEmpty(state.MainThread.ErrorMessage))
                                throw new SequenceExecutionException(state.MainThread.ErrorMessage);

                            var output = state.MainThread.Output;
                            if (typeof(TOutput) != NoneType.Trait)
                            {
                                if (output is TOutput castOutput)
                                    return castOutput;

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
                catch (Exception ex)
                {
                    execLogger.OptiLog(LogLevel.Critical,
                                       "[{sequenceDefinitionDisplayName}-{sequenceDefinitionUID}] Sequence execution crash {message} - State : {stateDisplayName} - {exception}",
                                       sequenceDefintion.DisplayName,
                                       sequenceDefintion.Uid,
                                       ex.Message,
                                       state.ToDebugDisplayName(),
                                       ex);

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

            return defaultResult;
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

        #endregion
    }
}
