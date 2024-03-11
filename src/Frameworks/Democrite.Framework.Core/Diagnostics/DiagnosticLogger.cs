// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Diagnostics
{
    using Democrite.Framework.Core.Abstractions.Diagnostics;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge to send <see cref="IDiagnosticLog"/> to <see cref="IDiagnosticLogConsumer"/>(s)
    /// </summary>
    /// <seealso cref="IDiagnosticLogger" />
    public sealed class DiagnosticLogger : SafeDisposable, IDiagnosticLogger
    {
        #region Fields

        private readonly IReadOnlyCollection<IDiagnosticLogConsumer> _diagnosticLogConsumers;
        private readonly Subject<IDiagnosticLog> _logAsyncQueue;
        private readonly IDisposable _treatQueueToken;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticLogger"/> class.
        /// </summary>
        public DiagnosticLogger(IEnumerable<IDiagnosticLogConsumer> diagnosticLogConsumers, ILogger<IDiagnosticLogger>? logger = null)
        {
            this._diagnosticLogConsumers = diagnosticLogConsumers?.ToArray() ?? EnumerableHelper<IDiagnosticLogConsumer>.ReadOnlyArray;
            this.HasConsumer = this._diagnosticLogConsumers.Any();

            this._logger = logger ?? NullLogger<IDiagnosticLogger>.Instance;
            this._logAsyncQueue = new Subject<IDiagnosticLog>();

            var connecter = this._logAsyncQueue.ObserveOn(TaskPoolScheduler.Default).Publish();
            connecter.Connect();

            this._treatQueueToken = connecter.Subscribe(SendLog);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance has <see cref="IDiagnosticLogConsumer"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has <see cref="IDiagnosticLogConsumer"/>; otherwise, <c>false</c>.
        /// </value>
        public bool HasConsumer { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Sends log <typeparamref name="IDiagnosticLog"/> to consumers
        /// </summary>
        void IDiagnosticLogger.Log<TLogType>(TLogType log)
        {
            if (this.IsDisposed || !this.HasConsumer)
                return;

            this._logAsyncQueue.OnNext(log);
        }

        /// <summary>
        /// Sends the log to log consumers.
        /// </summary>
        private void SendLog(IDiagnosticLog log)
        {
            if (this.IsDisposed || !this.HasConsumer)
                return;

            var timeoutToken = CancellationHelper.Timeout();

            Task.Run(async () =>
            {
                if (this.IsDisposed || !this.HasConsumer)
                    return;

                try
                {
                    var sendLogTasks = this._diagnosticLogConsumers.Select(d => d.LogAsync(log, timeoutToken)).ToArray();
                    await Task.WhenAll(sendLogTasks);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    // NO exception MUST append but in case
                    Debugger.Break();

                    this._logger.OptiLog(LogLevel.Critical, "[{class}] - '{method}' - Exception: {exception}", nameof(DiagnosticLogger), nameof(SendLog), ex);
                }
            }, timeoutToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected override void DisposeBegin()
        {
            this._treatQueueToken.Dispose();

            base.DisposeBegin();
        }

        #endregion
    }
}
