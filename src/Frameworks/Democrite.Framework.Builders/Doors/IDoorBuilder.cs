// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Doors
{
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Builder of <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="ISignalNetworkBasePartBuilder{ISignalBuilder}" />
    public interface IDoorBuilder : ISignalNetworkBasePartBuilder<IDoorBuilder>
    {
        /// <summary>
        /// Sets the time a signal is maintain and interpret in history and "not consumed" queue.
        /// </summary>
        /// <param name="signalRetentionPeriod">period a message is valid and could be keep in history; default 1 days <see cref="DoorDefinition.DEFAULT_RETENTION_MAX_DELAY"/> to prevent any memory overload.</param>
        /// <remarks>
        ///     Null value could produce a memory overflow if other history clean way is not provided.
        /// </remarks>
        IDoorBuilder SetSignalRetention(TimeSpan? signalRetentionPeriod);

        /// <summary>
        /// Sets the signal retention maximum number on to be analyzed on door.
        /// </summary>
        /// <param name="history">The number of signal consumed on the past; default value 0 (<see cref="DoorDefinition.DEFAULT_HISTORY_RETENTION"/>) that also desactivate the history collect.</param>
        /// <param name="notConsumed">The number od signal not consumed. default null (<see cref="DoorDefinition.DEFAULT_NOT_CONSUMED_RETENTION"/>) to collect prevent signal lost.</param>
        IDoorBuilder SetSignalRetention(uint? history, uint? notConsumed);

        /// <summary>
        /// Configure a signal as input listen by the door
        /// </summary>
        IDoorWithListenerBuilder Listen(params SignalDefinition[] signalDefinition);

        /// <summary>
        /// Configure a signal as input listen by the door
        /// </summary>
        IDoorWithListenerBuilder Listen(params SignalId[] signalId);

        /// <summary>
        /// Configure a door output as input listen by the door
        /// </summary>
        IDoorWithListenerBuilder Listen(params DoorDefinition[] signalDefinition);

        /// <summary>
        /// Configure a door output as input listen by the door
        /// </summary>
        IDoorWithListenerBuilder Listen(params DoorId[] signalId);
    }
}
