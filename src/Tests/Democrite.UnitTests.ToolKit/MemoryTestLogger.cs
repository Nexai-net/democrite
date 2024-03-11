// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit
{
    using Elvex.Toolbox.Disposables;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// InMemory Logger for testing purpose
    /// </summary>
    public class MemoryTestLogger : SafeDisposable, ILogger
    {
        #region Fields

        private readonly ConcurrentBag<MemoryTestLogger> _children;
        private readonly ConcurrentQueue<TestLog> _logs;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryTestLogger"/> class.
        /// </summary>
        public MemoryTestLogger(object? state = null, MemoryTestLogger? parentLogger = null)
        {
            this._logs = new ConcurrentQueue<TestLog>();
            this.Logs = this._logs;

            this._children = new ConcurrentBag<MemoryTestLogger>();
            this.Children = this._children;

            this.State = state;
            this.Parent = parentLogger;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the state.
        /// </summary>
        public object? State { get; }

        /// <summary>
        /// Gets the parent logger
        /// </summary>
        public MemoryTestLogger? Parent { get; }

        /// <summary>
        /// Gets the logs.
        /// </summary>
        public IReadOnlyCollection<TestLog> Logs { get; }

        /// <summary>
        /// Gets or sets the type of the context.
        /// </summary>
        public Type? ContextType { get; protected set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        public IReadOnlyCollection<MemoryTestLogger> Children { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            var child = new MemoryTestLogger(state, this);
            this._children.Add(child);
            return child;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel _)
        {
            return true;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            this._logs.Enqueue(new TestLog(logLevel, eventId, state, exception, formatter(state, exception)));
        }

        /// <summary>
        /// Creates a child logger.
        /// </summary>
        public MemoryTestLogger<TInst> CreateChild<TInst>(object? state = null)
            where TInst : class
        {
            var child = new MemoryTestLogger<TInst>(state, this);
            this._children.Add(child);
            return child;
        }

        /// <summary>
        /// Creates a child logger.
        /// </summary>
        public MemoryTestLogger CreateChild(object? state = null)
        {
            var child = new MemoryTestLogger(state, this);
            this._children.Add(child);
            return child;
        }

        /// <summary>
        /// Flatterns all logs with childrens.
        /// </summary>
        public IEnumerable<TestLog> Flattern()
        {
            foreach (var log in this._logs)
            {
                yield return log;
            }

            foreach (var child in this._children)
            {
                foreach (var childLog in child.Flattern())
                {
                    yield return childLog;
                }
            }
        }

        #endregion
    }

    public class MemoryTestLogger<TInst> : MemoryTestLogger, ILogger<TInst>
        where TInst : class
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryTestLogger"/> class.
        /// </summary>
        public MemoryTestLogger(object? state = null, MemoryTestLogger? parentLogger = null)
            : base(state, parentLogger)
        {
            base.ContextType = typeof(TInst);
        }

        #endregion
    }
}
