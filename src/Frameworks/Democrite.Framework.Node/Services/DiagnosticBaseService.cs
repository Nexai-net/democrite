// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Services
{
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Elvex.Toolbox.Disposables;

    /// <summary>
    /// Base diagnostic service managment
    /// </summary>
    /// <seealso cref="SafeDisposable" />
    internal abstract class DiagnosticBaseService<TDiagnosticLog> : SafeDisposable
        where TDiagnosticLog : class, IDiagnosticLog
    {
        #region Fields

        private readonly IDiagnosticLogger _diagnosticLogger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainCallDiagnosticTracerService"/> class.
        /// </summary>
        public DiagnosticBaseService(IDiagnosticLogger diagnosticLogger)
        {
            this._diagnosticLogger = diagnosticLogger;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance has <see cref="IDiagnosticLogConsumer"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has <see cref="IDiagnosticLogConsumer"/>; otherwise, <c>false</c>.
        /// </value>
        protected bool HasConsumer
        {
            get { return this._diagnosticLogger.HasConsumer; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends log <typeparamref name="TDiagnosticLog"/> to consumers
        /// </summary>
        protected void Send(TDiagnosticLog log)
        {
            if (this.IsDisposed || !this.HasConsumer)
                return;

            this._diagnosticLogger.Log(log);
        }

        #endregion
    }
}
