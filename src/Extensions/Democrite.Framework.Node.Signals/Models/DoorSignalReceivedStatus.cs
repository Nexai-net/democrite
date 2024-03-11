// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Models
{
    using Democrite.Framework.Core.Abstractions.Signals;
    using Elvex.Toolbox.Helpers;

    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// State class use to store signal status
    /// </summary>
    public sealed class DoorSignalReceivedStatus : IEquatable<DoorSignalReceivedStatus>
    {
        #region Fiels

        private readonly List<SignalMessage> _signalsReceivedHistory;
        private readonly LinkedList<SignalMessage> _signalsReceivedNotConsumed;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorSignalReceivedStatus"/> class.
        /// </summary>
        public DoorSignalReceivedStatus(Guid signalId)
        {
            this.SignalId = signalId;

            this._signalsReceivedHistory = new List<SignalMessage>(20);
            this._signalsReceivedNotConsumed = new LinkedList<SignalMessage>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorSignalReceivedStatus"/> class.
        /// </summary>
        /// <remarks>Used to recover from the serialization</remarks>
        internal DoorSignalReceivedStatus(DoorSignalReceivedStatusSurrogate surrogate)
            : this(surrogate.SignalId)
        {
            this._signalsReceivedHistory = new List<SignalMessage>(surrogate.SignalReceivedHistory ?? EnumerableHelper<SignalMessage>.Enumerable);

            this._signalsReceivedHistory.EnsureCapacity(this._signalsReceivedHistory.Count + 20);

            this._signalsReceivedNotConsumed = new LinkedList<SignalMessage>(surrogate.LastSignalReceivedNotConsumed
                                                                                    ?.OrderBy(p => p.SendUtcTime) ?? EnumerableHelper<SignalMessage>.Enumerable);

            this.LastSignalReceived = surrogate.LastSignalReceived;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the signal identifier.
        /// </summary>
        public Guid SignalId { get; }

        /// <summary>
        /// Gets the not consume signal sort for oldest to newest based on <see cref="SignalMessage.SendUtcTime"/> ordered from older to recent
        /// </summary>
        /// <remarks>
        ///     To be consumed like a stack
        /// </remarks>
        public IReadOnlyCollection<SignalMessage> SignalsReceivedNotConsumed
        {
            get { return this._signalsReceivedNotConsumed; }
        }

        /// <summary>
        /// Gets the last signal received.
        /// </summary>
        public SignalMessage? LastSignalReceived { get; private set; }

        /// <summary>
        /// Gets the signal received history.
        /// </summary>
        public IReadOnlyCollection<SignalMessage> SignalsReceivedHistory
        {
            get { return this._signalsReceivedHistory; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Push the <paramref name="signal"/> into status content <see cref="SignalsReceivedNotConsumed"/> and <see cref="SignalsReceivedHistory"/>
        /// </summary>
        internal void Push(in SignalMessage signal)
        {
            ArgumentNullException.ThrowIfNull(signal);

            var sentSignalTime = signal.SendUtcTime;

            this._signalsReceivedNotConsumed.AddAfterWhen((_, next, newOne) => next.SendUtcTime > sentSignalTime, signal);

            if (this.LastSignalReceived == null || signal.SendUtcTime > this.LastSignalReceived.SendUtcTime)
                this.LastSignalReceived = signal;
        }

        /// <summary>
        /// Inform the current status the <paramref name="signal"/> have been used.
        /// </summary>
        internal void MarkAsUsed(in SignalMessage signal, bool enableHistoryRetention)
        {
            this._signalsReceivedNotConsumed.Remove(signal);

            if (enableHistoryRetention)
                this._signalsReceivedHistory.Add(signal);
        }

        /// <summary>
        /// Clean not consumed values and retain only <paramref name="maxRetention"/> recent
        /// </summary>
        internal bool ClearNotConsumed(in uint maxRetention)
        {
            var toRemovedNotConsumed = this._signalsReceivedNotConsumed.Nodes()
                                                                       .OrderBy(s => s.Value.SendUtcTime)
                                                                       .SkipLast((int)maxRetention)
                                                                       .ToArray();

            foreach (var node in toRemovedNotConsumed)
                this._signalsReceivedNotConsumed.Remove(node);

            return toRemovedNotConsumed.Length > 0;
        }

        /// <summary>
        /// Clean historical values and retain only <paramref name="maxRetention"/> recent
        /// </summary>
        internal bool ClearHistory(in uint maxRetention)
        {
            var toRemovedNotConsumed = this._signalsReceivedHistory.OrderBy(k => k.SendUtcTime)
                                                                   .Skip((int)maxRetention)
                                                                   .ToArray();

            foreach (var node in toRemovedNotConsumed)
                this._signalsReceivedHistory.Remove(node);

            return toRemovedNotConsumed.Length > 0;
        }

        /// <summary>
        /// Clean historical values received before <paramref name="maxUtcTimeRetentions"/>
        /// </summary>
        internal bool ClearHistoryAndNotConsumed(DateTime maxUtcTimeRetentions)
        {
            var nbRemoved = this._signalsReceivedHistory.RemoveAll(s => s.SendUtcTime < maxUtcTimeRetentions);

            var toRemovedNotConsumed = this._signalsReceivedNotConsumed.Nodes()
                                                                       .Where(s => s.Value.SendUtcTime < maxUtcTimeRetentions)
                                                                       .ToArray();

            foreach (var node in toRemovedNotConsumed)
                this._signalsReceivedNotConsumed.Remove(node);

            return nbRemoved > 0 || toRemovedNotConsumed.Length > 0;
        }

        /// <inheritdoc />
        public bool Equals(DoorSignalReceivedStatus? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.SignalId == other.SignalId &&
                   this.LastSignalReceived == other.LastSignalReceived &&
                   this.SignalsReceivedNotConsumed.SequenceEqual(other.SignalsReceivedNotConsumed) &&
                   this.SignalsReceivedHistory.SequenceEqual(other.SignalsReceivedHistory);
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.SignalId,
                                    this.LastSignalReceived,
                                    this.SignalsReceivedNotConsumed.OrderBy(s => s.Uid).Aggregate(0, (acc, s) => acc ^ s.GetHashCode()),
                                    this.SignalsReceivedHistory.OrderBy(s => s.Uid).Aggregate(0, (acc, s) => acc ^ s.GetHashCode()));
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is DoorSignalReceivedStatus other)
                return Equals(other);

            return false;
        }

        #endregion
    }
}
