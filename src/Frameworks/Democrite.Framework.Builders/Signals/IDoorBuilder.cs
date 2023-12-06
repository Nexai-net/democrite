// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Builder of <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="ISignalNetworkBasePartBuilder{ISignalBuilder}" />
    public interface IDoorBuilder : ISignalNetworkBasePartBuilder<IDoorBuilder>
    {
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
