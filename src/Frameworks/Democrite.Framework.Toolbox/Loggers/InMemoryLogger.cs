// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Loggers
{
    using Democrite.Framework.Toolbox.Abstractions.Loggers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    /// <summary>
    /// Logger that keep in memory the logger
    /// </summary>
    /// <typeparam name="TCategoryName">The type of the category name.</typeparam>
    /// <seealso cref="ILogger{TCategoryName}" />
    public sealed class InMemoryLogger : ILogger, IInMemoryLogger
    {
        #region Fields

        private readonly IOptionsMonitor<LoggerFilterOptions> _loggerOptions;

        private readonly IObservable<string> _observableLog;
        private readonly Subject<string> _logRelay;
        private readonly Queue<string> _logs;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryLogger"/> class.
        /// </summary>
        public InMemoryLogger(IOptionsMonitor<LoggerFilterOptions> loggerOptions)
        {
            this._loggerOptions = loggerOptions;

            this._logs = new Queue<string>();
            this._logRelay = new Subject<string>();

            var observableLog = this._logRelay.ObserveOn(TaskPoolScheduler.Default).Publish();
            observableLog.Connect();

            this._observableLog = observableLog;
        }

        #endregion

        #region Properties
        #endregion

        #region Methods

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetLogsCopy()
        {
            lock (this._logs)
            {
                return this._logs.ToArray();
            }
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= this._loggerOptions.CurrentValue.MinLevel;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var msgLog = string.Format("[{0}] {1}", logLevel, formatter(state, exception));

            lock (this._logs)
            {
                this._logs.Enqueue(msgLog);
            }

            this._logRelay.OnNext(msgLog);
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<string> observer)
        {
            return this._observableLog.Subscribe(observer);
        }

        #endregion
    }
}
