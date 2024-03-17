// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Administrations
{
    using Democrite.Framework.Node.Abstractions.Models.Administrations;

    using System.Threading.Tasks;

    /// <summary>
    /// Define an <see cref="IAddressable"/> able to received administration event
    /// </summary>
    public interface IAdminEventReceiver
    {
        /// <summary>
        /// Receives the event information.
        /// </summary>
        /// <remarks>
        ///     To ensure a maximum of synchronization the emitter will wait every receiver treatment maximum 2 sec to prevent deadlock
        /// </remarks>
        Task ReceiveAsync<TEvent>(TEvent adminEvent, GrainCancellationToken cancellationToken) where TEvent : AdminEventArg;
    }
}
