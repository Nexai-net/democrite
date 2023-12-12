// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Models
{
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Surrogate structure used to store <see cref="DoorSignalReceivedStatus"/> instances.
    /// </summary>
    [GenerateSerializer]
    public struct DoorSignalReceivedStatusSurrogate
    {
        #region Properties

        /// <summary>
        /// Gets the signal identifier.
        /// </summary>
        [Id(0)]
        public Guid SignalId { get; set; }

        /// <summary>
        /// Gets the last signal received.
        /// </summary>
        [Id(1)]
        public IEnumerable<SignalMessage> LastSignalReceivedNotConsumed { get; set; }

        /// <summary>
        /// Gets the signal received history.
        /// </summary>
        [Id(2)]
        public IEnumerable<SignalMessage> SignalReceivedHistory { get; set; }

        /// <summary>
        /// Gets or sets the last signal received.
        /// </summary>
        [Id(3)]
        public SignalMessage? LastSignalReceived { get; set; }  

        #endregion

        #region Methods

        /// <summary>
        /// Creates <see cref="DoorSignalReceivedStatusSurrogate"/> from <see cref="DoorSignalReceivedStatus"/>
        /// </summary>
        public static DoorSignalReceivedStatusSurrogate CreateFrom(DoorSignalReceivedStatus status)
        {
            return new DoorSignalReceivedStatusSurrogate()
            {
                SignalId = status.SignalId,
                LastSignalReceivedNotConsumed = status.SignalsReceivedNotConsumed.ToArray(),
                SignalReceivedHistory = status.SignalsReceivedHistory.ToArray(),
                LastSignalReceived = status.LastSignalReceived
            };
        }

        #endregion
    }
}
