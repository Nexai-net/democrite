// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.DebugTools
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// VGrain used to write on the logger the information about a signal
    /// </summary>
    /// <seealso cref="IVGrain" />
    /// <seealso cref="ISignalReceiver" />
    public interface IDisplaySignalsInfoVGrain : IVGrain, ISignalReceiver
    {
        /// <summary>
        /// Displays the signal information on the logger.
        /// </summary>
        Task DisplaySignalInfoAsync(SignalMessage signalMessage, IExecutionContext ctx);
    }
}