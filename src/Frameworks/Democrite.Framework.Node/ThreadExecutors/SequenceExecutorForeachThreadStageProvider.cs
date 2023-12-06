// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Core.Diagnostics;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Abstractions.Disposables;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread stage executor managing foreach call
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    public sealed class SequenceExecutorForeachThreadStageProvider : ISequenceExecutorThreadStageProvider, ISequenceExecutorThreadStageHandler
    {
        #region Fields

        private const string ERROR_NOT_ALLOW_INNER = "[Technical] MUST not execute foreach if already exist some inner threads ({IExecutionContext})";

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanHandler(ISequenceStageDefinition? stage)
        {
            return stage is SequenceStageForeachDefinition;
        }

        /// <inheritdoc />
        public ISequenceExecutorThreadStageHandler Provide(ISequenceStageDefinition? _)
        {
            return this;
        }

        /// <inheritdoc />
        public ValueTask<StageStepResult> ExecAsync(ISequenceStageDefinition stepBase,
                                                    object? input,
                                                    IExecutionContext sequenceContext,
                                                    ILogger logger,
                                                    IDiagnosticLogger diagnosticLogger,
                                                    IVGrainProvider vgrainProvider,
                                                    Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            var step = (SequenceStageForeachDefinition)stepBase;
            ArgumentNullException.ThrowIfNull(step);

            using (var token = securityThreadHandlerAccessor())
            {
                // set post process to aggregate result
                token.Content.RegisterPostProcess(ForeachSequenceStagePostProcess);

                // Create inner thread by input element
                if (!token.Content.HasInnerThreads && input is System.Collections.IEnumerable collection)
                {
                    var innerThreads = collection.Cast<object>()
                                                 .Select(subInput => token.Content.CreateInnerThread(new SequenceExecutorExecThreadState(sequenceContext.FlowUID,
                                                                                                                                        step.InnerFlow.Uid,
                                                                                                                                        Guid.NewGuid(),
                                                                                                                                        sequenceContext.CurrentExecutionId,
                                                                                                                                        step.InnerFlow,
                                                                                                                                        subInput),
                                                                                                    step.InnerFlow))
                                                 .ToArray();

                    foreach (var inner in innerThreads)
                        diagnosticLogger.Log(ExecutionContextChangeDiagnosticLog.From(inner.ExecutionContext));

                    var currentState = token.Content.GetCurrentThreadState();

                    // Register inner execution states
                    token.Content.SetInnerThreads(innerThreads);

                    var result = new StageStepResult(null, Task.CompletedTask);
                    return ValueTask.FromResult(result);
                }

                if (token.Content.HasInnerThreads)
                {
                    logger.OptiLog(LogLevel.Error, ERROR_NOT_ALLOW_INNER, sequenceContext);
                    throw new VGrainInvalidOperationDemocriteException(ERROR_NOT_ALLOW_INNER, sequenceContext);
                }
            }
            throw new InvalidOperationException();
        }

        #region Tools

        /// <summary>
        /// Call after a <see cref="ForeachSequenceStageStepDefinition"/> stage to aggregate results
        /// </summary>
        /// <remarks>
        ///     All post process are execute
        /// </remarks>
        private Task<StageStepResult> ForeachSequenceStagePostProcess(ISequenceStageDefinition sequenceStageDefinition,
                                                                            Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            using (var handlerSafeToken = securityThreadHandlerAccessor())
            {
                var collectionType = NoneType.Trait;
                Task task = Task.FromResult(NoneType.Instance);

                if (handlerSafeToken.Content.HasInnerThreads && handlerSafeToken.Content.AllInnerThreadsJobDone)
                {
                    var foreachThreads = handlerSafeToken.Content.PullInnerThreads(true);
                    var foreachThreadsHandler = foreachThreads.Select(f => f.GetSecurityThreadHandler()).ToArray();

                    try
                    {
                        var foreachIndexedState = foreachThreadsHandler.ToDictionary(k => k, kv => kv.Content.GetCurrentThreadState());

                        if (foreachIndexedState.All(i => i.Value.Exception != null))
                        {
                            var aggreException = foreachIndexedState.Where(f => f.Value?.Exception != null)
                                                                    .Select(e => e.Value.Exception!)
                                                                    .ToArray();

                            throw new AggregateException(aggreException);
                        }

                        ArgumentNullException.ThrowIfNull(sequenceStageDefinition);

                        collectionType = sequenceStageDefinition.Output?.MakeArrayType();

                        if (sequenceStageDefinition.Output != null && collectionType != null)
                        {
                            object? result = Activator.CreateInstance(collectionType, new object[] { 0 });

                            if (foreachThreadsHandler.Length > 0)
                            {
                                // Store thread results
                                var objResult = foreachThreadsHandler.Select(t => foreachIndexedState[t].Output)
                                                                     .Where(t => t != null && t is NoneType == false)
                                                                     .ToArray();

                                var resultArray = (Array?)Activator.CreateInstance(collectionType, new object[] { objResult.Length });

                                if (resultArray != null)
                                    Array.Copy(objResult, resultArray, objResult.Length);

                                result = resultArray;
                            }

                            task = result.GetTaskFrom(collectionType);
                        }
                    }
                    finally
                    {
                        foreach (var handler in foreachThreadsHandler)
                            handler.Dispose();
                    }
                }

                return Task.FromResult(new StageStepResult(collectionType, task));
            }

            #endregion
        }

        #endregion
    }
}
