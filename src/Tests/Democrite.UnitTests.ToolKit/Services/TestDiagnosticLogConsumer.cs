// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Services
{
    using Democrite.Framework.Core.Abstractions.Diagnostics;

    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Log consumer used to track execution flow during unit test
    /// </summary>
    /// <remarks>
    ///     Must be setup as a remote services
    /// </remarks>
    /// <seealso cref="IDiagnosticLogConsumer" />
    public sealed class TestDiagnosticLogConsumer : IDiagnosticLogConsumer
    {
        #region Fields

        private static readonly TimeSpan s_safetyDelay = TimeSpan.FromSeconds(0.5);

        private readonly ConcurrentBag<IDiagnosticLog> _logs;
        private long _logCounter;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDiagnosticLogConsumer"/> class.
        /// </summary>
        public TestDiagnosticLogConsumer()
        {
            this._logs = new ConcurrentBag<IDiagnosticLog>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logs.
        /// </summary>
        public IReadOnlyCollection<IDiagnosticLog> Logs
        {
            get { return this._logs; }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Store log for future analyses
        /// </summary>
        public Task LogAsync<TLog>(TLog log, CancellationToken token)
            where TLog : class, IDiagnosticLog
        {
            this._logs.Add(log);
            Interlocked.Increment(ref this._logCounter);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Ensure all log have been received
        /// </summary>
        /// <remarks>
        ///     Use time wetween last log received
        /// </remarks>
        public async Task FlushAsync(CancellationToken token)
        {
            long logCount;
            long currentCount;
            do
            {
                logCount = Interlocked.Read(ref this._logCounter);

                await Task.Delay(s_safetyDelay, token);
                token.ThrowIfCancellationRequested();

                currentCount = Interlocked.Read(ref this._logCounter);

                Debug.WriteLine("-- logCount : {0} - currentCount - {1}", logCount, currentCount);

                // Continue if during s_safetyDelay other log arrived
            } while (logCount != currentCount);

            // Unlock if no log have been founded during s_safetyDelay
        }

        #endregion
    }
}
