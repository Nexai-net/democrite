// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Events;

    using Elvex.Toolbox.Disposables;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    /// <summary>
    /// Execution context used as followup in the command execution
    /// </summary>
    /// <seealso cref="SafeDisposable" />
    internal sealed class CommandExecutionContext : SafeDisposable
    {
        #region Fields

        private readonly CancellationToken _cancellationToken;
        private readonly CommandExecutionContext? _parent;
        private readonly Queue<BlackboardEvent> _eventQueue = null!;
        private readonly Queue<BlackboardEvent> _eventConsumed = null!;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutionContext"/> class.
        /// </summary>
        public CommandExecutionContext(CommandExecutionContext parent)
            : this(parent.CancellationToken, parent.Depth + 1, parent.Logger)
        {
            this._parent = parent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutionContext"/> class.
        /// </summary>
        public CommandExecutionContext(CancellationToken cancellationToken, ILogger logger)
            : this(cancellationToken, depth: 0, logger: logger)
        {
            this._eventQueue = new Queue<BlackboardEvent>();
            this._eventConsumed = new Queue<BlackboardEvent>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutionContext"/> class.
        /// </summary>
        private CommandExecutionContext(CancellationToken cancellationToken,
                                       int depth,
                                       ILogger logger)
        {
            this._cancellationToken = cancellationToken;
            this.Depth = depth;
            this.Logger = logger;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public ref readonly CancellationToken CancellationToken
        {
            get { return ref this._cancellationToken; }
        }

        /// <summary>
        /// Gets the depth.
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the events.
        /// </summary>
        public IReadOnlyCollection<BlackboardEvent> ConsumeEvents()
        {
            if (this._parent is not null)
                return this._parent.ConsumeEvents();

            lock (this._eventQueue)
            {
                var evts = this._eventQueue.ToArray();
                this._eventQueue.Clear();

                foreach (var evt in evts)
                    this._eventConsumed.Enqueue(evt);

                return evts;
            }
        }

        /// <summary>
        /// Push an event occured during the command execution
        /// </summary>
        public void EnqueueEvent(BlackboardEvent @event)
        {
            if (this._parent is not null)
            {
                this._parent.EnqueueEvent(@event);
                return;
            }

            lock (this._eventQueue)
            {
                this._eventQueue.Enqueue(@event);
            }
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected override void DisposeBegin()
        {
            if (this._parent is null)
            {
                lock (this._eventQueue)
                {
                    this._eventQueue.Clear();
                }
            }

            base.DisposeBegin();
        }

        #endregion
    }
}
