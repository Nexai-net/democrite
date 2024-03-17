// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Signals.Models;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// State use by <see cref="DoorBaseVGrain{TVGrain, TDoordDef}"/> to handled signal status
    /// </summary>
    /// <seealso cref="SignalSubscriptionState" />
    public sealed class DoorHandlerState : IEquatable<DoorHandlerState>
    {
        #region Fields

        private ImmutableDictionary<Guid, DoorSignalReceivedStatus> _signalReceivedStatus;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorHandlerState"/> class.
        /// </summary>
        internal DoorHandlerState(IEnumerable<DoorSignalReceivedStatusSurrogate> doorSignalListerners, IEnumerable<Guid>? listenSignals)
        {

            this._signalReceivedStatus = (doorSignalListerners ?? EnumerableHelper<DoorSignalReceivedStatusSurrogate>.ReadOnly)
                                                .Select(surrogate => new DoorSignalReceivedStatus(surrogate))
                                                .ToImmutableDictionary(k => k.SignalId);

            this.ListenSignals = listenSignals?.ToArray() ?? EnumerableHelper<Guid>.ReadOnly;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the listen signals.
        /// </summary>
        public IReadOnlyCollection<Guid> ListenSignals { get; private set; }

        /// <summary>
        /// Gets the listerners.
        /// </summary>
        internal IEnumerable<DoorSignalReceivedStatus> SignalStatus
        {
            get { return this._signalReceivedStatus.Values; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the last active signal since.
        /// </summary>
        /// <param name="activeWindowLoopUp">Define a window period when the signal must have been fire; if null it pick the oldest</param>
        internal IReadOnlyCollection<SignalMessage> GetLastActiveSignalSince(DateTime? activeWindowLoopUp = null)
        {
            if (activeWindowLoopUp == null)
            {
                return this._signalReceivedStatus.Where(s => s.Value.SignalsReceivedNotConsumed.Any())
                                                 .Select(s => s.Value.SignalsReceivedNotConsumed.First())
                                                 .ToArray();
            }

            if (activeWindowLoopUp.Value.Kind != DateTimeKind.Utc)
                throw new InvalidDataException("Signal handler : Only utc time are toleranted");

            return this._signalReceivedStatus.Where(s => s.Value.SignalsReceivedNotConsumed.Any())
                                             .Select(s => s.Value.SignalsReceivedNotConsumed.FirstOrDefault(n => n.SendUtcTime > activeWindowLoopUp.Value))
                                             .Where(n => n != null)
                                             .Select(s => s!)
                                             .ToArray();
        }

        /// <summary>
        /// Initializes the signal support.
        /// </summary>
        internal void InitializeSignalSupport(in DoorDefinition doorDefinition)
        {
            this.ListenSignals = doorDefinition.DoorSourceIds
                                               .Select(s => s.Uid)
                                               .Concat(doorDefinition.SignalSourceIds.Select(s => s.Uid))
                                               .ToArray();

            var missingSignalListner = this.ListenSignals.Except(this._signalReceivedStatus.Keys)
                                                         .ToDictionary(k => k, v => new DoorSignalReceivedStatus(v));

            this._signalReceivedStatus = this._signalReceivedStatus.AddRange(missingSignalListner);

            // Add current door output to be able to use it in the fire decision.
            if (!this._signalReceivedStatus.ContainsKey(doorDefinition.DoorId.Uid))
            {
                this._signalReceivedStatus = this._signalReceivedStatus.Add(doorDefinition.DoorId.Uid,
                                                                            new DoorSignalReceivedStatus(doorDefinition.DoorId.Uid));
            }
        }

        /// <summary>
        /// Updates a signal status.
        /// </summary>
        internal bool Push(in SignalMessage signal)
        {
            if (this._signalReceivedStatus.TryGetValue(signal.From.SourceDefinitionId, out var doorSignalStatus))
            {
                doorSignalStatus.Push(signal);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Define if the signal have been used.
        /// </summary>
        internal void MarkAsUsed(in IReadOnlyCollection<SignalMessage> signals, bool enableHistoryRetention)
        {
            foreach (var signal in signals)
            {
                if (this._signalReceivedStatus.TryGetValue(signal.From.SourceDefinitionId, out var doorSignalStatus))
                    doorSignalStatus.MarkAsUsed(signal, enableHistoryRetention);
            }
        }

        /// <summary>
        /// Cleans the history past to old base on <paramref name="maxUtcTime"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ClearHistoryAndNotConsumed(DateTime maxUtcTime)
        {
            return ApplyActionOnAllSignals(s => s.ClearHistoryAndNotConsumed(maxUtcTime));
        }

        /// <summary>
        /// Cleans the history based on <paramref name="maxRetensionSize"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ClearHistory(uint maxRetensionSize)
        {
            return ApplyActionOnAllSignals(s => s.ClearHistory(maxRetensionSize));
        }

        /// <summary>
        /// Cleans the history based on <paramref name="maxRetensionSize"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ClearNotConsumed(uint maxRetensionSize)
        {
            return ApplyActionOnAllSignals(s => s.ClearNotConsumed(maxRetensionSize));
        }

        /// <summary>
        /// Gets the last signal received associate to <paramref name="signalUid"/>
        /// </summary>
        internal SignalMessage? GetLastSignalReceived(Guid signalUid)
        {
            if (this._signalReceivedStatus.TryGetValue(signalUid, out var signalStatus))
                return signalStatus.LastSignalReceived;

            return null;
        }

        private bool ApplyActionOnAllSignals(Func<DoorSignalReceivedStatus, bool> action)
        {
            var applyed = false;
            foreach (var status in this._signalReceivedStatus)
            {
                var applySucess = action(status.Value);
                applyed |= applySucess;
            }

            return applyed;
        }

        /// <inheritdoc />
        public bool Equals(DoorHandlerState? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.ListenSignals.SequenceEqual(other.ListenSignals) &&
                   this.SignalStatus.SequenceEqual(other.SignalStatus);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is DoorHandlerState other)
                return Equals(other);

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.ListenSignals?.OrderBy(s => s).Aggregate(0, (acc, l) => acc ^ l.GetHashCode()),
                                    this.SignalStatus?.OrderBy(s => s.SignalId).Aggregate(0, (acc, s) => acc ^ s.GetHashCode()));
        }

        #endregion
    }
}
