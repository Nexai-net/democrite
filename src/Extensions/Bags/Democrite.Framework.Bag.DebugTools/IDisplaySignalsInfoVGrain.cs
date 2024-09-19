// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.DebugTools
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes.MetaData;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Orleans.Concurrency;

    /// <summary>
    /// VGrain used to write on the logger the information about a signal
    /// </summary>
    /// <seealso cref="IVGrain" />
    /// <seealso cref="ISignalReceiver" />
    [VGrainMetaData("48AB5776-0D89-445B-871A-4EE1FA0E4293",
                    "display-signal",
                    namespaceIdentifier: DebugToolConstants.BAG_NAMESPACE,
                    displayName: "Display",
                    description: "Used to show in logs signals when emit.",
                    categoryPath: "tools")]
    public interface IDisplaySignalsInfoVGrain : IVGrain, ISignalReceiver, ISignalReceiverReadOnly
    {
        /// <summary>
        /// Displays the signal information on the logger.
        /// </summary>
        [OneWay]
        [ReadOnly]
        [VGrainMetaDataMethod("from-message")]
        Task DisplaySignalInfoAsync(SignalMessage signalMessage, IExecutionContext ctx);
    }
}