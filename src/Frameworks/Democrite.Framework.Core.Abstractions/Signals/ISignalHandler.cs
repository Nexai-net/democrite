﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Define the virtual grain in charge of a specific signal
    /// </summary>
    public interface ISignalHandler
    {
        /// <summary>
        /// Subscribe a receiver for the signal.
        /// </summary>
        /// <remarks>
        ///     Receiver are identify by his <see cref="GraindId"/> no risque of duplicates if you register multiple times
        /// </remarks>
        /// <returns>
        ///     Return a unique id needed to unsuscribe
        /// </returns>
        Task<Guid> SubscribeAsync(GrainId grainId);

        /// <summary>
        /// Unsuscribes.
        /// </summary>
        /// <param name="subscritionId">The subscrition identifier returned on <see cref="SubscribeAsync(ISignalReceiver)"/>.</param>
        Task UnsuscribeAsync(Guid subscritionId);
    }
}
