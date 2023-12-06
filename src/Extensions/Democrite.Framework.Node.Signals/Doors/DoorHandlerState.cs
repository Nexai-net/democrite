// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Signals.Models;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// State use by <see cref="DoorBaseVGrain{TVGrain, TDoordDef}"/> to handled signal status
    /// </summary>
    /// <seealso cref="SignalSubscriptionState" />
    public sealed class DoorHandlerState
    {
        #region Fields

        private ImmutableDictionary<Guid, DoorSignalReceivedStatus> _signalReceivedStatus;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorHandlerState"/> class.
        /// </summary>
        internal DoorHandlerState(IEnumerable<DoorSignalReceivedStatusSurrogate> doorSignalListerners)
        {

            this._signalReceivedStatus = (doorSignalListerners ?? EnumerableHelper<DoorSignalReceivedStatusSurrogate>.ReadOnly)
                                                .Select(surrogate => new DoorSignalReceivedStatus(surrogate))
                                                .ToImmutableDictionary(k => k.SignalId);

            this.ListenSignals = EnumerableHelper<Guid>.ReadOnly;
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
        internal IReadOnlyCollection<SignalMessage> GetLastActiveSignalSince(DateTime minTolerateUtcTime)
        {
            if (minTolerateUtcTime.Kind != DateTimeKind.Utc)
                throw new InvalidDataException("Signal handler : Only utc time are toleranted");

            return this._signalReceivedStatus.Where(s => s.Value.LastSignalReceivedNotConsomed != null &&
                                                         s.Value.LastSignalReceivedNotConsomed.SendUtcTime > minTolerateUtcTime)
                                             .Select(s => s.Value.LastSignalReceivedNotConsomed!)
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
        internal bool UpdateSignalStatus(in SignalMessage signal)
        {
            if (this._signalReceivedStatus.TryGetValue(signal.From.SourceDefinitionId, out var doorSignalStatus))
            {
                doorSignalStatus.Update(signal);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Define if the signal have been uses to fire.
        /// </summary>
        internal void UseToFire(in IReadOnlyCollection<SignalMessage> signals)
        {
            foreach (var signal in signals)
            {
                if (this._signalReceivedStatus.TryGetValue(signal.From.SourceDefinitionId, out var doorSignalStatus))
                    doorSignalStatus.UseToFire(signal);
            }
        }

        /// <summary>
        /// Cleans the history past to old base on <paramref name="maxUtcTime"/>
        /// </summary>
        internal void CleanHistory(in DateTime maxUtcTime)
        {
            foreach (var status in this._signalReceivedStatus)
                status.Value.CleanHistory(maxUtcTime);
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

        #endregion
    }
}
