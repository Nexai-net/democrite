﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Node.Abstractions.Models;
    using Democrite.Framework.Toolbox.Abstractions.Disposables;

    using Microsoft.Extensions.Logging;

    using System.Threading.Tasks;

    /// <summary>
    /// Handler execution of the <see cref="ISequenceStageDefinition"/>
    /// </summary>
    public interface ISequenceExecutorThreadStageHandler
    {
        /// <summary>
        /// Stage execution
        /// </summary>
        ValueTask<StageStepResult> ExecAsync(ISequenceStageDefinition step,
                                             object? input,
                                             IExecutionContext sequenceContext,
                                             ILogger logger,
                                             IDiagnosticLogger diagnosticLogger,
                                             IVGrainProvider vgrainProvider,
                                             Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor);
    }
}
