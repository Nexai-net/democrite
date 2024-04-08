// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Builder used to create a <see cref="EventControllerOptions"/>
    /// </summary>
    public interface IEventControllerOptionsBuilder<TWizard>
        where TWizard : IEventControllerOptionsBuilder<TWizard>
    {
        /// <summary>
        /// Listens the signals.
        /// </summary>
        TWizard ListenSignals(params Guid[] signalIds);

        /// <summary>
        /// Listens the signals.
        /// </summary>
        TWizard ListenSignals(params SignalId[] signalIds);

        /// <summary>
        /// Listens the doorss.
        /// </summary>
        TWizard ListenDoors(params Guid[] doorIds);

        /// <summary>
        /// Listens the doorss.
        /// </summary>
        TWizard ListenDoors(params DoorId[] doorIds);

        /// <summary>
        /// Builds <see cref="EventControllerOptions"/>
        /// </summary>
        EventControllerOptions Build();
    }

    /// <summary>
    /// Builder used to create a <see cref="EventControllerOptions"/>
    /// </summary>
    public interface IEventControllerOptionsBuilder : IEventControllerOptionsBuilder<IEventControllerOptionsBuilder>
    {

    }
}
