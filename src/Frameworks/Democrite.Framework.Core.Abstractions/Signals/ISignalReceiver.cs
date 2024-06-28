// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// Describe a signal receiver
    /// </summary>
    public interface ISignalReceiver : IAddressable
    {
        /// <summary>
        /// Receives the signal asynchronous.
        /// </summary>
        [OneWay]
        Task ReceiveSignalAsync(SignalMessage signal);
    }

    /// <summary>
    /// Describe a signal receiver
    /// </summary>
    public interface ISignalReceiverReadOnly : IAddressable
    {
        /// <summary>
        /// Receives the signal asynchronous.
        /// </summary>
        [OneWay]
        [ReadOnly]
        Task ReceiveSignalAsync(SignalMessage signal);
    }
}
