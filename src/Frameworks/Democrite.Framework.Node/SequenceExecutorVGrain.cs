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
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.Models;
    using Democrite.Framework.Node.Services;
    using Democrite.Framework.Node.ThreadExecutors;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Excutor in charge of a specific sequence
    /// </summary>
    /// <seealso cref="ISequenceExecutorVGrain" />
    [DemocriteSystemVGrain]
    internal sealed class SequenceExecutorVGrain : VGrainBase<SequenceExecutorState, SequenceExecutorStateSurrogate, SequenceExecutorStateConverter, ISequenceExecutorVGrain>, ISequenceExecutorVGrain
    {
        #region Fields

        private readonly ISequenceVGrainProviderFactory _sequenceVGrainProviderFactory;
        private readonly ISequenceDefinitionProvider _sequenceDefinitionManager;
        private readonly ISequenceExecutorThreadStageProvider _stageProvider;
        private readonly IDemocriteSerializer _democriteSerializer;
        private readonly IDiagnosticLogger _diagnosticLogger;
        private readonly IObjectConverter _objectConverter;
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
                                      [PersistentState("SequenceExecutor", DemocriteConstants.DefaultDemocriteStateConfigurationKey)] IPersistentState<SequenceExecutorStateSurrogate> sequenceExecutorState,
                                      ITimeManager timeManager,
                                      IObjectConverter objectConverter,
                                      IDemocriteSerializer democriteSerializer,
                                      ISequenceExecutorThreadStageProvider stageProvider,
                                      ISequenceVGrainProviderFactory sequenceVGrainProviderFactory)
            : base(logger, sequenceExecutorState)
        {
            this._sequenceVGrainProviderFactory = sequenceVGrainProviderFactory;
            this._sequenceDefinitionManager = sequenceDefinitionManager;
            this._democriteSerializer = democriteSerializer;
            this._diagnosticLogger = diagnosticLogger;
            this._objectConverter = objectConverter;
            this._loggerFactory = loggerFactory;
            this._stageProvider = stageProvider;
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

            var execCancelToken = executionContext.CancellationToken;

            try
            {
                var sequenceDefintion = await this._sequenceDefinitionManager.GetFirstValueByIdAsync(state.SequenceDefinitionId, execCancelToken);

                if (sequenceDefintion is null)
                    throw new MissingDefinitionException(typeof(SequenceDefinition), state.SequenceDefinitionId.ToString());

                execLogger.OptiLog(LogLevel.Information,
                                   "Start sequence '{sequenceDefinitionDisplayName}' (Id: {sequenceDefinitionUID})",
                                   sequenceDefintion.DisplayName,
                                   sequenceDefintion.Uid);

                // Loop to navigate through pipeline stages
                var sequencesDone = false;

                try
                {
                    if (state.MainThread == null)
                        state.Initialize(executionContext, sequenceDefintion, input, this._democriteSerializer);

                    Debug.Assert(state.MainThread != null);

                    var mainExecutionThread = await SequenceExecutorExecThread.BuildFromAsync(state.MainThread,
                                                                                              id => this._sequenceDefinitionManager.GetFirstValueByIdAsync(id, execCancelToken),
                                                                                              this._stageProvider,
                                                                                              this._objectConverter,
                                                                                              this._timeManager,
                                                                                              this._democriteSerializer);

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

        #endregion
    }
}
