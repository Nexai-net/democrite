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
    /// Log cursor executed
    /// </summary>
    /// <seealso cref="DiagnosticBaseLog" />
    /// <seealso cref="IDiagnosticCallLog" />
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class ExecutionCursorDiagnosticLog : DiagnosticBaseLog, IExecutionCursorDiagnosticLog
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionCursorDiagnosticLog"/> class.
        /// </summary>
        public ExecutionCursorDiagnosticLog(Guid cursor,
                                            DiagnosticLogTypeEnum type,
                                            Guid? flowUid,
                                            Guid? currentExecutionId,
                                            Guid? callerId,
                                            OrientationEnum orientation,
                                            DateTime createOn)
            : base(type, flowUid, currentExecutionId, callerId, orientation, createOn)
        {
            this.Cursor = cursor;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the execution cursor.
        /// </summary>
        [Id(0)]
        public Guid Cursor { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        internal static IExecutionCursorDiagnosticLog From(Guid cursor, IExecutionContext executionContext)
        {
            return new ExecutionCursorDiagnosticLog(cursor, DiagnosticLogTypeEnum.StateCursor,
                                                           executionContext.FlowUID,
                                                           executionContext.CurrentExecutionId,
                                                           executionContext.ParentExecutionId,
                                                           OrientationEnum.None,
                                                           DateTime.UtcNow);
        }

        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return nameof(ExecutionCursorDiagnosticLog) + "[" + this.Cursor + "]";
        }

        #endregion
    }
}
