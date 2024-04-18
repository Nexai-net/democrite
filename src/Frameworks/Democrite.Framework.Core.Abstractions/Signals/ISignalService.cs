// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Core.Abstractions.Doors;

    using Orleans.Concurrency;

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
        [OneWay]
        Task<Guid> Fire(string signalName, CancellationToken token, IVGrainInformationProvider? vgrainInformationProvider = null);

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        [OneWay]
        Task<Guid> Fire<TData>(string signalName, TData data, CancellationToken token, IVGrainInformationProvider? vgrainInformationProvider = null)
            where TData : struct;

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        [OneWay]
        Task<Guid> Fire<TData>(in SignalId signalId, in TData data, CancellationToken token, IVGrainInformationProvider? vgrainInformationProvider = null)
            where TData : struct;

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        [OneWay]
        Task<Guid> Fire(string signalName, object? data, CancellationToken token, IVGrainInformationProvider? vgrainInformationProvider = null);

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        [OneWay]
        Task<Guid> Fire(in SignalId signalId, object? data, CancellationToken token, IVGrainInformationProvider? vgrainInformationProvider = null);

        /// <summary>
        /// Fires the specified signal by his name.
        /// </summary>
        /// <returns>
        ///     Unique id of the fire used to trace the firering results.
        /// </returns>
        /// <exception cref="SignalNotFoundException" />
        [OneWay]
        Task<Guid> Fire(in SignalId signalId, CancellationToken token, IVGrainInformationProvider? vgrainInformationProvider = null);

        /// <summary>
        /// Subscribes to signal by name
        /// </summary>
        Task<SubscriptionId> SubscribeAsync(string signalDoorName, ISignalReceiver receiver, CancellationToken token);

        /// <summary>
        /// Subscribes to signal by <see cref="SignalId"/>
        /// </summary>
        Task<SubscriptionId> SubscribeAsync(SignalId signalid, ISignalReceiver receiver, CancellationToken token);

        /// <summary>
        /// Subscribes to signal by <see cref="SignalId"/>
        /// </summary>
        Task<SubscriptionId> SubscribeAsync(DoorId signalid, ISignalReceiver receiver, CancellationToken token);

        /// <summary>
        /// Unsubscribes the specified subscription identifier.
        /// </summary>
        Task UnsubscribeAsync(SubscriptionId subscriptionId, CancellationToken token);

        /// <summary>
        /// Unsubscribes the specified subscription identifier.
        /// </summary>
        Task UnsubscribeAsync(SignalId signalId, ISignalReceiver receiver, CancellationToken token);

        /// <summary>
        /// Unsubscribes the specified subscription identifier.
        /// </summary>
        Task UnsubscribeAsync(DoorId doorId, ISignalReceiver receiver, CancellationToken token);
    }
}
