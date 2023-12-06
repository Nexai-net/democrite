// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Models
{
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// State class use to store signal status
    /// </summary>
    public sealed class DoorSignalReceivedStatus
    {
        #region Fiels

        private readonly Queue<SignalMessage> _signalReceivedHistory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorSignalReceivedStatus"/> class.
        /// </summary>
        public DoorSignalReceivedStatus(Guid signalId)
        {
            this.SignalId = signalId;
            this._signalReceivedHistory = new Queue<SignalMessage>(20);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorSignalReceivedStatus"/> class.
        /// </summary>
        /// <remarks>Used to recover from the serialization</remarks>
        internal DoorSignalReceivedStatus(DoorSignalReceivedStatusSurrogate surrogate)
            : this(surrogate.SignalId)
        {
            this.LastSignalReceivedNotConsomed = surrogate.LastSignalReceivedNotConsomed;
            this._signalReceivedHistory = new Queue<SignalMessage>(surrogate.SignalReceivedHistory
                                                                   ?.OrderBy(p => p.SendUtcTime) ?? EnumerableHelper<SignalMessage>.Enumerable);

            this._signalReceivedHistory.EnsureCapacity(20);

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the signal identifier.
        /// </summary>
        public Guid SignalId { get; }

        /// <summary>
        /// Gets the last signal received.
        /// </summary>
        public SignalMessage? LastSignalReceivedNotConsomed { get; private set; }

        /// <summary>
        /// Gets the last signal received.
        /// </summary>
        public SignalMessage? LastSignalReceived { get; private set; }

        /// <summary>
        /// Gets the signal received history.
        /// </summary>
        public IReadOnlyCollection<SignalMessage> SignalReceivedHistory
        {
            get { return this._signalReceivedHistory; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Push the <paramref name="signal"/> into history to update the status information.
        /// </summary>
        internal void Update(in SignalMessage signal)
        {
            ArgumentNullException.ThrowIfNull(signal);

            if (this.LastSignalReceivedNotConsomed == null || this.LastSignalReceivedNotConsomed.SendUtcTime >= signal.SendUtcTime)
                this.LastSignalReceivedNotConsomed = signal;

            if (this.LastSignalReceived == null || this.LastSignalReceived.SendUtcTime >= signal.SendUtcTime)
                this.LastSignalReceived = signal;

            if (signal != null)
                this._signalReceivedHistory.Enqueue(signal);
        }

        /// <summary>
        /// Inform the current status the <paramref name="signal"/> have been used to trigger the door.
        /// </summary>
        internal void UseToFire(in SignalMessage signal)
        {
            if (this.LastSignalReceivedNotConsomed == signal)
                this.LastSignalReceivedNotConsomed = null;
        }

        /// <summary>
        /// Clean historical values received before <paramref name="maxUtcTime"/>
        /// </summary>
        internal void CleanHistory(in DateTime maxUtcTime)
        {
            while (this._signalReceivedHistory.Count > 0)
            {
                var peek = this._signalReceivedHistory.Peek();
                if (peek.SendUtcTime > maxUtcTime)
                    break;

                this._signalReceivedHistory.Dequeue();

                if (this.LastSignalReceivedNotConsomed == peek)
                    this.LastSignalReceivedNotConsomed = null;
            }
        }

        #endregion
    }
}
