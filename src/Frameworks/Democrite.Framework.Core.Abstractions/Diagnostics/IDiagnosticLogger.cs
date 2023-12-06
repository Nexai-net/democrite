// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Diagnostics
{
    /// <summary>
    /// Logger about <see cref="IDiagnosticLog"/>
    /// </summary>
    public interface IDiagnosticLogger
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance has <see cref="IDiagnosticLogConsumer"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has <see cref="IDiagnosticLogConsumer"/>; otherwise, <c>false</c>.
        /// </value>
        bool HasConsumer { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Log diagnostic informaiton call, execution context change, ...
        /// </summary>
        void Log<TLogType>(TLogType log) where TLogType : class, IDiagnosticLog;

        #endregion
    }
}
