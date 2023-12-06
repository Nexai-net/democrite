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
        public SignalMessage? LastSignalReceivedNotConsomed { get; set; }

        /// <summary>
        /// Gets the signal received history.
        /// </summary>
        [Id(2)]
        public IEnumerable<SignalMessage> SignalReceivedHistory { get; set; }

        #endregion
    }
}
