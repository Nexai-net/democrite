// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Models
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Customizations;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Core.Diagnostics;
    using Democrite.Framework.Core.Models;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Node.Services;
    using Democrite.Framework.Node.ThreadExecutors;

    using Elvex.Toolbox;
    using Elvex.Toolbox.Abstractions.Disposables;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Execution thread used by <see cref="SequenceExecutorVGrain"/> to execute a flow step
    /// </summary>
    internal sealed class SequenceExecutorExecThread : SafeDisposable, ISequenceExecutorExecThread
    {
        #region Fields

        private const string ERROR_NOT_ALLOW_INNER = "[Technical] MUST not execute foreach if already exist some inner threads ({IExecutionContext})";

        //private static readonly IReadOnlyCollection<ISequenceExecutorThreadStageSourceProvider> s_democriteStageProviders;
        private readonly ISequenceExecutorThreadStageProvider _stageProvider;

        private Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>? _postProcessHook;

        private readonly IDemocriteSerializer _democriteSerializer;
        private readonly IObjectConverter _objectConverter;
        private readonly ITimeManager _timeManager;

        private readonly SequenceDefinition _sequenceDefinition;
        private readonly bool _forceFirstContextGeneration;
        private readonly SemaphoreSlim _locker;

        private ISecureContextToken<ISequenceExecutorThreadHandler>? _secureContext;

        private IReadOnlyCollection<SequenceExecutorExecThread> _innerThreads;
        private SequenceStageDefinition? _currentStage;
        private IExecutionContext _executionContext;
        private Task? _currentTaskExecution;
        private object? _nextStageInput;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorExecThread"/> class.
        /// </summary>
        private SequenceExecutorExecThread(SequenceExecutorExecThreadState state,
                                           SequenceDefinition sequenceDefinition,
                                           IEnumerable<SequenceExecutorExecThread>? innerThreads,
                                           ISequenceExecutorThreadStageProvider sequenceExecutorProvider,
                                           IObjectConverter objectConverter,
                                           ITimeManager timeManager,
                                           IDemocriteSerializer democriteSerializer,
                                           SequenceExecutorState rootState,
                                           IExecutionContext sourceExecutionContext,
                                           bool forceFirstContextGeneration = false)
        {
            this.State = state;
            this.RootState = rootState;

            this._timeManager = timeManager;
            this._democriteSerializer = democriteSerializer;
            this._objectConverter = objectConverter;
            this._stageProvider = sequenceExecutorProvider;

            this._forceFirstContextGeneration = forceFirstContextGeneration;

            this._executionContext = new ExecutionContext(state.FlowUid,
                                                          state.CurrentStageExecId,
                                                          state.ParentStageExecId);

            if (sourceExecutionContext is IExecutionContextInternal executionContextInternal && executionContextInternal.GrainCancellationToken is not null)
                ((IExecutionContextInternal)this._executionContext).ForceGrainCancellationToken(executionContextInternal.GrainCancellationToken);

            this._locker = new SemaphoreSlim(1);

            this._innerThreads = innerThreads?.ToArray() ?? EnumerableHelper<SequenceExecutorExecThread>.ReadOnlyArray;
            this._sequenceDefinition = sequenceDefinition;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the state of the root.
        /// </summary>
        public SequenceExecutorState RootState { get; }

        /// <summary>
        /// Gets the current thread state.
        /// </summary>
        public SequenceExecutorExecThreadState State { get; }

        /// <summary>
        /// Gets the current execution context.
        /// </summary>
        public IExecutionContext ExecutionContext
        {
            get
            {
                this._locker.Wait();
                try
                {
                    return this._executionContext;
                }
                finally
                {
                    this._locker.Release();
                }
            }
        }

        /// <summary>
        /// Gets the customizations.
        /// </summary>
        public ExecutionCustomizationDescriptions? Customizations { get; private set; }

        #endregion

        #region Nested

        /// <summary>
        /// Handler used to open controller thread handler
        /// </summary>
        /// <seealso cref="ISequenceExecutorThreadHandler" />
        private sealed class SequenceExecutorThreadHandler : ISequenceExecutorThreadHandler
        {
            #region Fields

            private readonly SequenceExecutorExecThread _sequenceExecutorExecThread;

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="SequenceExecutorThreadHandler"/> class.
            /// </summary>
            public SequenceExecutorThreadHandler(SequenceExecutorExecThread sequenceExecutorExecThread)
            {
                this._sequenceExecutorExecThread = sequenceExecutorExecThread;
            }

            #endregion

            #region Properties

            /// <inheritdoc />
            public bool AllInnerThreadsJobDone
            {
                get { return this._sequenceExecutorExecThread?._innerThreads?.All(w => w.State.JobDone) ?? false; }
            }

            /// <inheritdoc />
            public bool HasInnerThreads
            {
                get { return (this._sequenceExecutorExecThread?._innerThreads?.Count ?? 0) > 0; }
            }

            #endregion

            #region Methods

            /// <inheritdoc />
            public ISequenceExecutorExecThread CreateInnerThread(SequenceExecutorExecThreadState sequenceExecutorExecThreadState, SequenceDefinition innerFlow, IExecutionContext sourceExecutionContext)
            {
                return new SequenceExecutorExecThread(sequenceExecutorExecThreadState,
                                                      innerFlow,
                                                      null,
                                                      this._sequenceExecutorExecThread._stageProvider,
                                                      this._sequenceExecutorExecThread._objectConverter,
                                                      this._sequenceExecutorExecThread._timeManager,
                                                      this._sequenceExecutorExecThread._democriteSerializer,
                                                      this._sequenceExecutorExecThread.RootState,
                                                      sourceExecutionContext);
            }

            /// <inheritdoc />
            public ISequenceExecutorExecThreadState GetCurrentDoneThreadState()
            {
                return this._sequenceExecutorExecThread.State;
            }

            /// <inheritdoc />
            /// <remarks>
            ///     Used by post process to evaluate the current thread information
            /// </remarks>
            public ISequenceExecutorExecThreadState GetCurrentInProcessThreadState()
            {
                var savedState = this._sequenceExecutorExecThread.State;
                var currentExecContext = this._sequenceExecutorExecThread._executionContext;
                return new SequenceExecutorExecThreadState(savedState.FlowUid,
                                                           savedState.FlowDefinitionId,
                                                           currentExecContext.CurrentExecutionId,
                                                           currentExecContext.ParentExecutionId,
                                                           this._sequenceExecutorExecThread._currentStage?.Uid,
                                                           savedState.Output,
                                                           this._sequenceExecutorExecThread._nextStageInput,
                                                           this._sequenceExecutorExecThread._innerThreads?.Select(i => i.State),
                                                           null,
                                                           true,
                                                           false);
            }

            /// <inheritdoc />
            public ExecutionCustomizationDescriptions? GetSequenceExecutionCustomization()
            {
                //Customization
                return this._sequenceExecutorExecThread.RootState.Customization;
            }

            /// <inheritdoc />
            public IReadOnlyCollection<ISequenceExecutorExecThread> PullInnerThreads(bool emptyOnPull)
            {
                var threads = this._sequenceExecutorExecThread._innerThreads;

                if (emptyOnPull)
                    this._sequenceExecutorExecThread._innerThreads = EnumerableHelper<SequenceExecutorExecThread>.ReadOnlyArray;

                return threads;
            }

            /// <inheritdoc />
            public void RegisterPostProcess(Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>> postProcessCallback)
            {
                this._sequenceExecutorExecThread._postProcessHook = postProcessCallback;
            }

            /// <inheritdoc />
            public void SetInnerThreads(IReadOnlyCollection<ISequenceExecutorExecThread> innerThreads)
            {
                ArgumentNullException.ThrowIfNull(innerThreads);

                this._sequenceExecutorExecThread._innerThreads = innerThreads?.OfType<SequenceExecutorExecThread>().ToArray() ?? EnumerableHelper<SequenceExecutorExecThread>.ReadOnlyArray;

                var innerState = innerThreads?.Select(i => i.GetSecurityThreadHandler())
                                              .ToArray() ?? EnumerableHelper<ISecureContextToken<ISequenceExecutorThreadHandler>>.ReadOnlyArray;

                try
                {

                    this._sequenceExecutorExecThread.State.Update(this._sequenceExecutorExecThread.State.Cursor,
                                                                  this._sequenceExecutorExecThread.State.CurrentStageExecId,
                                                                  this._sequenceExecutorExecThread.State.Output,
                                                                  innerState.Select(i => (SequenceExecutorExecThreadState)i.Token.GetCurrentDoneThreadState()));
                }
                finally
                {
                    foreach (var inner in innerState)
                        inner.Dispose();
                }
            }

            #endregion
        }

        /// <summary>
        /// Secure token used to handled thread manipulation be external sources mainly <see cref="ISequenceExecutorThreadStageHandler"/>
        /// </summary>
        /// <seealso cref="SafeDisposable" />
        /// <seealso cref="ISecureContextToken{ISequenceExecutorExecThread}" />
        private sealed class SecureContextHandler : SafeDisposable, ISecureContextToken<ISequenceExecutorThreadHandler>
        {
            #region Fields

            private readonly SequenceExecutorExecThread _sequenceExecutorExecThread;
            private readonly SequenceExecutorThreadHandler _content;

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="SecureContextHandler"/> class.
            /// </summary>
            public SecureContextHandler(SequenceExecutorExecThread sequenceExecutorExecThread)
            {
                this._sequenceExecutorExecThread = sequenceExecutorExecThread;
                this._content = new SequenceExecutorThreadHandler(sequenceExecutorExecThread);

                this.SecureContextId = Guid.NewGuid();

                this._sequenceExecutorExecThread._secureContext = this;
            }

            #endregion

            #region Properties

            /// <inheritdoc />
            public ISequenceExecutorThreadHandler Token
            {
                get
                {
                    if (!object.ReferenceEquals(this._sequenceExecutorExecThread._secureContext, this))
                        throw new VGrainSecurityDemocriteException("Access ISequenceExecutorThreadHandler Content secure by another ISecureContextToken");

                    return this._content;
                }
            }

            /// <inheritdoc />
            public Guid SecureContextId { get; }

            #endregion

            #region Methods

            /// <summary>
            /// Locks the specified cancellation token.
            /// </summary>
            public void Lock(CancellationToken cancellationToken)
            {
                this._sequenceExecutorExecThread._locker.Wait(cancellationToken);
            }

            /// <summary>
            /// Call at the end of the dispose process
            /// </summary>
            protected override void DisposeEnd()
            {
                this._sequenceExecutorExecThread._secureContext = null;
                this._sequenceExecutorExecThread._locker.Release();
                base.DisposeEnd();

            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the execution thread from state
        /// </summary>
        internal static Task<SequenceExecutorExecThread> BuildFromAsync(SequenceExecutorExecThreadState threadState,
                                                                        Func<Guid, ValueTask<SequenceDefinition?>> getSequenceDef,
                                                                        ISequenceExecutorThreadStageProvider stageProviders,
                                                                        IObjectConverter objectConverter,
                                                                        ITimeManager timeManager,
                                                                        IDemocriteSerializer democriteSerializer,
                                                                        SequenceExecutorState rootState,
                                                                        IExecutionContext sourceExecutionContext)
        {
            return BuildFromImplAsync(threadState,
                                      getSequenceDef,
                                      0,
                                      stageProviders,
                                      objectConverter,
                                      timeManager,
                                      democriteSerializer,
                                      rootState,
                                      sourceExecutionContext);
        }

        /// <summary>
        /// Builds the execution thread from state
        /// </summary>
        private static async Task<SequenceExecutorExecThread> BuildFromImplAsync(SequenceExecutorExecThreadState threadState,
                                                                                 Func<Guid, ValueTask<SequenceDefinition?>> getSequenceDef,
                                                                                 int depth,
                                                                                 ISequenceExecutorThreadStageProvider stageProviders,
                                                                                 IObjectConverter objectConverter,
                                                                                 ITimeManager timeManager,
                                                                                 IDemocriteSerializer democriteSerializer,
                                                                                 SequenceExecutorState rootState,
                                                                                 IExecutionContext sourceExecutionContext)
        {
            var def = await getSequenceDef(threadState.FlowDefinitionId) ?? throw new SequenceDefinitionMissingException(threadState.FlowDefinitionId);

            var innerThreads = new List<SequenceExecutorExecThread>();

            foreach (var innerState in threadState.InnerThreads)
            {
                var innerThread = await BuildFromImplAsync(innerState, async id =>
                {
                    if (id == def.Uid)
                        return def;

                    var stage = def[id];

                    if (stage is IFlowHostStageDefinition host && host.InnerFlow.Uid == id)
                        return host.InnerFlow;

                    return await getSequenceDef(id);
                }, depth + 1, stageProviders, objectConverter, timeManager, democriteSerializer, rootState, sourceExecutionContext);

                innerThreads.Add(innerThread);
            }

            return new SequenceExecutorExecThread(threadState,
                                                  def,
                                                  innerThreads,
                                                  stageProviders,
                                                  objectConverter,
                                                  timeManager,
                                                  democriteSerializer,
                                                  rootState,
                                                  sourceExecutionContext,
                                                  depth == 0);
        }

        /// <summary>
        /// Secures the thread handling access.
        /// </summary>
        public ISecureContextToken<ISequenceExecutorThreadHandler> GetSecurityThreadHandler()
        {
            var token = new SecureContextHandler(this);
            token.Lock(default);
            return token;
        }

        /// <summary>
        /// Gets the current execution thread tasks
        /// </summary>
        internal Task? GetThreadStageExecutionTasksAsync(ILogger logger,
                                                         ISequenceVGrainProvider sequenceVGrainProvider,
                                                         IDiagnosticLogger diagnosticLogger)
        {
            this._locker.Wait();
            try
            {
                // If inner then exec them
                var innerTasks = this._innerThreads.Where(a => !a.State.JobDone)
                                                   .Select(inner => inner.GetThreadStageExecutionTasksAsync(logger, sequenceVGrainProvider, diagnosticLogger))
                                                   .Where(t => t != null)
                                                   .OfType<Task>()
                                                   .ToArray();

                if (innerTasks.Any())
                    return Task.WhenAny(innerTasks);

                // If current job is done then return null
                if (this.State.JobDone)
                    return null;

                if (this._currentTaskExecution != null)
                {
                    // If current execution is not done; continue
                    if (!this._currentTaskExecution.IsCompleted)
                        return this._currentTaskExecution;

                    // If exec failed then raise issue
                    if (this._currentTaskExecution.IsCanceled || this._currentTaskExecution.IsFaulted)
                    {
                        logger.OptiLog(LogLevel.Error, "[Stage {stageId}] -- Failed -- {exception}", this.State.CurrentStageExecId, this._currentTaskExecution.Exception);
                        this.State.SetError(this._currentTaskExecution.Exception);
                        return null;
                    }
                }

                // if inner done exec post process if exit
                if (this._postProcessHook != null)
                {
                    var tmp = this._postProcessHook;
                    this._postProcessHook = null;

                    ArgumentNullException.ThrowIfNull(this._currentStage);

                    // Force in another thread to ensure GetSecurityThreadHandler will not dead lock
                    return Task.Factory.StartNew(async () => await tmp(this._currentStage, GetSecurityThreadHandler)
                                                                                .ContinueWith(EndPostProcess))
                                       .Unwrap();
                }

                // If current job is done then return null
                if (this.State.JobDone)
                    return null;

                // If state is restored or first execution the use state cursor instead of next
                var nextCursor = this.State.Running == false// || this.State.Started) 
                                        ? this.State.Cursor
                                        : this._sequenceDefinition.GetNextStage(this.State.Cursor);

                var stage = this._sequenceDefinition[nextCursor];

                if (stage is null || nextCursor is null)
                {
                    this.State.SetJobIsDone(this._nextStageInput);
                    return null;
                }

                // Use previous ouput as input
                var stageInput = this._nextStageInput;

                // At thread start use ThreadInput
                if (!this.State.Started)
                    stageInput = this.State.ThreadInput;

                // Prevent going to next context when restore state or first execution
                bool contextUpdated = false;

                this._executionContext = this._executionContext.NextContext();
                contextUpdated = true;

                this._currentStage = stage;

                this.State.Update(nextCursor,
                                  this._executionContext.CurrentExecutionId,
                                  stageInput,
                                  this._innerThreads?.Select(i => i.State));

                this._nextStageInput = stageInput;

                if (contextUpdated)
                {
                    var logCtxUpdate = ExecutionContextChangeDiagnosticLog.From(this._executionContext);
                    logger.OptiLog(LogLevel.Debug, "[Exec Id {executionId}] [ContextUpdated] -- {log}", logCtxUpdate.CurrentExecutionId, logCtxUpdate);
                    diagnosticLogger.Log(logCtxUpdate);
                }

                var log = ExecutionCursorDiagnosticLog.From(nextCursor.Value, this._executionContext);
                logger.OptiLog(LogLevel.Debug, "[Exec Id {executionId}] -- {log}", log.CurrentExecutionId, log);
                diagnosticLogger.Log(log);

                this._currentTaskExecution = Task.Factory.StartNew(async () => await ExecuteStageAsync(stage,
                                                                                                       stageInput,
                                                                                                       this._executionContext,
                                                                                                       logger,
                                                                                                       diagnosticLogger,
                                                                                                       sequenceVGrainProvider)).Unwrap();

                return this._currentTaskExecution;
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Call after a post process to managed result
        /// </summary>
        private void EndPostProcess(Task<StageStepResult> t)
        {
            if (t.IsCompleted && !t.IsCanceled && !t.IsFaulted)
            {
                this._locker.Wait();
                try
                {
                    this._nextStageInput = ProcessStageStepResultOutput(t.Result);
                }
                finally
                {
                    this._locker.Release();
                }
            }
        }

        /// <summary>
        /// Call to process <see cref="StageStepResult"/>
        /// </summary>
        private object? ProcessStageStepResultOutput(StageStepResult execTask)
        {
            if (execTask.expectedResultType != null && execTask.result != null && execTask.expectedResultType != typeof(NoneType))
                return execTask.result.GetResult();
            return NoneType.Instance;
        }

        /// <summary>
        /// Execute all steps of a stage
        /// </summary>
        private async Task ExecuteStageAsync(SequenceStageDefinition stage,
                                             object? input,
                                             IExecutionContext sequenceContext,
                                             ILogger logger,
                                             IDiagnosticLogger diagnosticLogger,
                                             ISequenceVGrainProvider sequenceVGrainProvider)
        {
            // Exec all steps
            logger.OptiLog(LogLevel.Trace, "-- Start step : '{stage}' for input '{input}'", stage, input);

            var execTask = await HandleStageAsync(stage,
                                                  input,
                                                  sequenceContext,
                                                  logger,
                                                  diagnosticLogger,
                                                  sequenceVGrainProvider);

            if (execTask.result != null)
                await execTask.result;

            object? output = ProcessStageStepResultOutput(execTask);

            logger.OptiLog(LogLevel.Trace, "-- End step : '{stage}' for output '{output}'", stage, output);

            await this._locker.WaitAsync(sequenceContext.CancellationToken);

            try
            {
                this._nextStageInput = output;
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Generic step execution
        /// </summary>
        private async ValueTask<StageStepResult> HandleStageAsync(SequenceStageDefinition step,
                                                                  object? input,
                                                                  IExecutionContext sequenceContext,
                                                                  ILogger logger,
                                                                  IDiagnosticLogger diagnosticLogger,
                                                                  ISequenceVGrainProvider sequenceVGrainProvider)
        {
            ArgumentNullException.ThrowIfNull(step);

            var executor = this._stageProvider.Provide(step)
                                        ?? throw new InvalidOperationException($"Doesn't know how to handled step definition {step.GetType()}");

            try
            {
                // Try convert input to ISequenceStageDefinition.Input type if needed
                if (step.Input is not null && input != null)
                {
                    var stepInputType = step.Input!.ToType();
                    var inputType = input.GetType();
                    if (step.Input != inputType && !inputType.IsAssignableTo(stepInputType))
                    {
                        try
                        {
                            if (NoneType.Trait == stepInputType)
                                input = NoneType.Instance;

                            else if (this._objectConverter.TryConvert(input, stepInputType, out var convertInput))
                                input = convertInput;
                        }
                        catch (Exception ex)
                        {
                            logger.OptiLog(LogLevel.Error, "Failed convert input {source} to {targetType} : {exception}", input, stepInputType, ex);
                            throw;
                        }
                    }
                }

                return await executor.ExecAsync(step,
                                                input,
                                                sequenceContext,
                                                logger,
                                                diagnosticLogger,
                                                sequenceVGrainProvider.GetGrainProvider(ref step),
                                                GetSecurityThreadHandler);
            }
            catch (Exception ex)
            {
                logger.OptiLog(LogLevel.Error, "[Step: {step}] execution failed : {exception}", step, ex);
                throw;
            }
        }

        #endregion
    }
}
