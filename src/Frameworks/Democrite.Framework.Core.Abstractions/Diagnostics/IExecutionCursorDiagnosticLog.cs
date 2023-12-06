// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Diagnostics
{
    using System;

    public interface IExecutionCursorDiagnosticLog : IDiagnosticLog
    {
        /// <summary>
        /// Gets the execution cursor.
        /// </summary>
        Guid Cursor { get; }
    }
}
