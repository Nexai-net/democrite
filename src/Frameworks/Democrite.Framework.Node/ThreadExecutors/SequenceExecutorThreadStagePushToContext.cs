// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ThreadExecutors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Sequence.Stages;
    using Democrite.Framework.Node.Abstractions;
    using Democrite.Framework.Node.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Disposables;
    using Elvex.Toolbox.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread stage executor managing simple input filter
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageSourceProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorThreadStagePushToContext : ISequenceExecutorThreadStageHandler
    {
        #region Fields
        
        private static readonly MethodInfo s_tryPushData;

        private readonly IDemocriteSerializer _democriteSerializer;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorFilterThreadStageProvider"/> class.
        /// </summary>
        static SequenceExecutorThreadStagePushToContext()
        {
            var tryPushData = (((Expression<Func<IExecutionContext, bool>>)((IExecutionContext ctx) => ctx.TryPushContextData<int>(42, false, null!))).Body as MethodCallExpression)!.Method.GetGenericMethodDefinition();
            Debug.Assert(tryPushData != null);

            s_tryPushData = tryPushData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorThreadStagePushToContext"/> class.
        /// </summary>
        public SequenceExecutorThreadStagePushToContext(ITimeManager timeManager,
                                                        IDemocriteSerializer democriteSerializer)
        {
            this._timeManager = timeManager;
            this._democriteSerializer = democriteSerializer;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<StageStepResult> ExecAsync(SequenceStageDefinition step,
                                                    object? input,
                                                    IExecutionContext sequenceContext,
                                                    ILogger logger,
                                                    IDiagnosticLogger diagnosticLogger,
                                                    IVGrainProvider vgrainProvider,
                                                    Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            var toPush = (SequenceStagePushToContextDefinition)step;

            var inputType = input is not null ? input.GetType() : (Type?)null;
            var inputTypeArg = inputType is not null ? TypedArgument.From(inputType) : null;

            SequenceExecutorThreadStageHandlerExtensions.StartStageWorking(this,
                                                                           diagnosticLogger,
                                                                           sequenceContext,
                                                                           this._timeManager,
                                                                           inputTypeArg);
            // Filter
            object? dataToPush = toPush.AccessExpression?.Resolve(input);

            var pushSucceed = false;
            if (dataToPush is not null)
            {
                pushSucceed = (bool)s_tryPushData.MakeGenericMethod(dataToPush.GetType())
                                                 .Invoke(sequenceContext, new object[] { dataToPush, toPush.Override, this._democriteSerializer })!;
            }

            if (!pushSucceed)
                throw new SequenceExecutionException("Issue pushing data to context");

            // output diagnosticlog
            SequenceExecutorThreadStageHandlerExtensions.EndStageWorking(this,
                                                                         diagnosticLogger,
                                                                         sequenceContext,
                                                                         this._timeManager,
                                                                         inputTypeArg);

            return ValueTask.FromResult(new StageStepResult(inputType, inputType is null ? Task.CompletedTask : input.GetTaskFrom(inputType)));
        }

        #endregion
    }
}
