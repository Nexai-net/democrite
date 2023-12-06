// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Node.Signals.Doors;
    using Democrite.Framework.Node.Signals.Models;

    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Surrogate struct to serialize <see cref="DoorHandlerState"/>
    /// </summary>
    [GenerateSerializer]
    public struct DoorHandlerStateSurrogate
    {
        /// <summary>
        /// Gets or sets the signal states.
        /// </summary>
        [Id(0)]
        public IEnumerable<DoorSignalReceivedStatusSurrogate> SignalStates { get; set; }
    }

    /// <summary>
    /// Declare a converter to convert both way <see cref="DoorHandlerState"/> to <see cref="DoorHandlerStateSurrogate"/>
    /// </summary>
    /// <seealso cref="IConverter{DoorHandlerState, DoorHandlerStateSurrogate}" />
    [RegisterConverter]
    public sealed class DoorHandlerStateSurrogateConverter : IConverter<DoorHandlerState, DoorHandlerStateSurrogate>
    {
        /// <inheritdoc />
        public DoorHandlerState ConvertFromSurrogate(in DoorHandlerStateSurrogate surrogate)
        {
            return new DoorHandlerState(surrogate.SignalStates);
        }

        /// <inheritdoc />
        public DoorHandlerStateSurrogate ConvertToSurrogate(in DoorHandlerState value)
        {
            return new DoorHandlerStateSurrogate()
            {
                SignalStates = value.SignalStatus
                                    .Select(s => new DoorSignalReceivedStatusSurrogate()
                                    {
                                        SignalId = s.SignalId,
                                        LastSignalReceivedNotConsomed = s.LastSignalReceivedNotConsomed,
                                        SignalReceivedHistory = s.SignalReceivedHistory.ToArray()
                                    }).ToArray()
            };
        }
    }
}
