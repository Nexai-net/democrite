// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Models
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Events;
    using Elvex.Toolbox.Disposables;

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

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutionContext"/> class.
        /// </summary>
        public CommandExecutionContext(CommandExecutionContext parent)
            : this(parent.CancellationToken, parent.Depth + 1)
        {
            this._parent = parent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutionContext"/> class.
        /// </summary>
        public CommandExecutionContext(CancellationToken cancellationToken)
            : this(cancellationToken, depth: 0)
        {
            this._eventQueue = new Queue<BlackboardEvent>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutionContext"/> class.
        /// </summary>
        private CommandExecutionContext(CancellationToken cancellationToken,
                                       int depth)
        {
            this._cancellationToken = cancellationToken;
            this.Depth = depth;
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
        /// Gets the events.
        /// </summary>
        public IReadOnlyCollection<BlackboardEvent> Events
        {
            get { return this._parent?.Events ?? this._eventQueue; }
        }

        /// <summary>
        /// Gets the depth.
        /// </summary>
        public int Depth { get; }

        #endregion

        #region Methods

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

            this._eventQueue.Enqueue(@event);
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected override void DisposeBegin()
        {
            if (this._parent == null)
                this._eventQueue.Clear();

            base.DisposeBegin();
        }

        #endregion
    }
}
