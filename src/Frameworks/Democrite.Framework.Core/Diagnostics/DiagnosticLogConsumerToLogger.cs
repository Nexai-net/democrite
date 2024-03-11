// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Diagnostics
{
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// <see cref="IDiagnosticLogConsumer"/> that relay log to classic logger
    /// </summary>
    /// <seealso cref="IDiagnosticLogConsumer" />
    public sealed class DiagnosticLogConsumerToLogger : IDiagnosticLogConsumer
    {
        #region Fields

        private readonly ILogger? _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticLogConsumerToLogger"/> class.
        /// </summary>
        public DiagnosticLogConsumerToLogger(ILogger<IDiagnosticLogConsumer>? logger = null)
        {
            this._logger = logger;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        Task IDiagnosticLogConsumer.LogAsync<TLog>(TLog log, CancellationToken token)
        {
            this._logger?.OptiLog(LogLevel.Debug, "{diagnosticLog}", log);
            return Task.CompletedTask;
        }

        #endregion
    }
}
