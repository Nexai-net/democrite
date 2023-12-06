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
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Toolbox;
    using Democrite.Framework.Toolbox.Abstractions.Disposables;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Dedicate filter typed about filter implementation
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorFilterThreadStageHandler<TCollectionInput, TItemInput> : ISequenceExecutorThreadStageHandler
        where TCollectionInput : IEnumerable<TItemInput>
    {
        #region Fields

        private static readonly Type s_collectionTraits = typeof(TCollectionInput);

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<StageStepResult> ExecAsync(ISequenceStageDefinition stepBase,
                                                    object? input,
                                                    IExecutionContext sequenceContext,
                                                    ILogger logger,
                                                    IDiagnosticLogger diagnosticLogger,
                                                    IVGrainProvider vgrainProvider,
                                                    Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            var step = (SequenceStageFilterDefinition)stepBase;

            var collection = (TCollectionInput?)input;

            if (collection is null || !collection.Any())
                return ValueTask.FromResult(new StageStepResult(s_collectionTraits, EnumerableHelper<TItemInput>.ReadOnlyArray.GetTaskFrom(s_collectionTraits)));

            // input diagnosticlog

            var inputDiagnosticLog = new DiagnosticInOutLog(sequenceContext.FlowUID,
                                                            sequenceContext.CurrentExecutionId,
                                                            sequenceContext.ParentExecutionId,
                                                            Core.Abstractions.Enums.OrientationEnum.In,
                                                            DateTime.UtcNow,
                                                            TypedArgument.From(input, s_collectionTraits),
                                                            string.Empty);
            diagnosticLogger.Log(inputDiagnosticLog);

            // Filter

            var expressionFilter = step.Condition.ToExpression<TItemInput, bool>();
            var expressionFilterFunc = expressionFilter.Compile();

            var filteredResults = collection?.Where(expressionFilterFunc).ToArray();

            // output diagnosticlog

            var outputDiagnosticLog = new DiagnosticInOutLog(sequenceContext.FlowUID,
                                                             sequenceContext.CurrentExecutionId,
                                                             sequenceContext.ParentExecutionId,
                                                             Core.Abstractions.Enums.OrientationEnum.Out,
                                                             DateTime.UtcNow,
                                                             TypedArgument.From(filteredResults, s_collectionTraits),
                                                             string.Empty);
            diagnosticLogger.Log(outputDiagnosticLog);

            return ValueTask.FromResult(new StageStepResult(s_collectionTraits, filteredResults.GetTaskFrom(s_collectionTraits)));
        }

        #endregion
    }
}
