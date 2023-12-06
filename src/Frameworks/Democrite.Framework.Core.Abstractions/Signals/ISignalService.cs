// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    /// <summary>
    /// Service in charge to give acces to all signal functionality like fire and/or subscribe
    /// </summary>
    public interface ISignalService
    {
        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        Task<Guid> Fire(string signalName, IVGrainInformationProvider? vgrainInformationProvider = null);

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        Task<Guid> Fire<TData>(string signalName, TData data, IVGrainInformationProvider? vgrainInformationProvider = null)
            where TData : struct;

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        Task<Guid> Fire<TData>(in SignalId signalId, in TData data, IVGrainInformationProvider? vgrainInformationProvider = null)
            where TData : struct;

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        Task<Guid> Fire(string signalName, object? data, IVGrainInformationProvider? vgrainInformationProvider = null);

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        Task<Guid> Fire(in SignalId signalId, object? data, IVGrainInformationProvider? vgrainInformationProvider = null);

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        Task<Guid> Fire(in SignalId signalId, IVGrainInformationProvider? vgrainInformationProvider = null);

        /// <summary>
        /// Subscribes to signal by name
        /// </summary>
        Task<Guid> SubscribeAsync(string signalDoorName, ISignalReceiver receiver);

        /// <summary>
        /// Subscribes to signal by <see cref="SignalId"/>
        /// </summary>
        Task<Guid> SubscribeAsync(in SignalId signalid, ISignalReceiver receiver);

        /// <summary>
        /// Subscribes to signal by <see cref="SignalId"/>
        /// </summary>
        Task<Guid> SubscribeAsync(in DoorId signalid, ISignalReceiver receiver);
    }
}
