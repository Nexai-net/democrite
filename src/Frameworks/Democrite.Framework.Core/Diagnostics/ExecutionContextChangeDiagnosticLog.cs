// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Diagnostics
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Democrite.Framework.Core.Abstractions.Enums;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Log generate when new context is generated
    /// </summary>
    /// <seealso cref="DiagnosticBaseLog" />
    /// <seealso cref="IDiagnosticCallLog" />
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class ExecutionContextChangeDiagnosticLog : DiagnosticBaseLog, IExecutionContextChangeDiagnosticLog
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContextChangeDiagnosticLog"/> class.
        /// </summary>
        public ExecutionContextChangeDiagnosticLog(DiagnosticLogTypeEnum type,
                                                   Guid? flowUid,
                                                   Guid? currentExecutionId,
                                                   Guid? callerId,
                                                   OrientationEnum orientation,
                                                   DateTime createOn)
            : base(type, flowUid, currentExecutionId, callerId, orientation, createOn)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        internal static IExecutionContextChangeDiagnosticLog From(IExecutionContext executionContext)
        {
            return new ExecutionContextChangeDiagnosticLog(DiagnosticLogTypeEnum.ContextGenerated,
                                                           executionContext.FlowUID,
                                                           executionContext.CurrentExecutionId,
                                                           executionContext.ParentExecutionId,
                                                           OrientationEnum.None,
                                                           DateTime.UtcNow);
        }

        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return nameof(ExecutionContextChangeDiagnosticLog);
        }

        #endregion
    }
}
