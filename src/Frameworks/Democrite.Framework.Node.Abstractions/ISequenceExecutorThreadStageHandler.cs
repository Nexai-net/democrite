// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Disposables;

    using Microsoft.Extensions.Logging;

    using System.Threading.Tasks;

    /// <summary>
    /// Handler execution of the <see cref="ISequenceStageDefinition"/>
    /// </summary>
    internal interface ISequenceExecutorThreadStageHandler
    {
        /// <summary>
        /// Stage execution
        /// </summary>
        ValueTask<StageStepResult> ExecAsync(SequenceStageDefinition step,
                                             object? input,
                                             IExecutionContext sequenceContext,
                                             ILogger logger,
                                             IDiagnosticLogger diagnosticLogger,
                                             IVGrainProvider vgrainProvider, // Provider is inject to be able to change implementation at runtime
                                             Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor);
    }
}
