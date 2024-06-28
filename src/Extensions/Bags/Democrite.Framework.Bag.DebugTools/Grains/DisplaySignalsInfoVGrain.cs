// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.DebugTools.Grains
{
    using Democrite.Framework.Bag.DebugTools;
    using Democrite.Framework.Bag.DebugTools.Models;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Orleans.Concurrency;
    using Orleans.Placement;

    using System.Threading.Tasks;

    /// <summary>
    /// Grain used to display on the logger <see cref="SignalMessage"/>
    /// </summary>
    /// <seealso cref="VGrainBase{IDisplaySignalsInfoVGrain}" />
    /// <seealso cref="IDisplaySignalsInfoVGrain" />
    [Reentrant]
    [StatelessWorker]
    [PreferLocalPlacement]
    internal sealed class DisplaySignalsInfoVGrain : DisplayAllInfoBaseVGrain<IDisplaySignalsInfoVGrain>, IDisplaySignalsInfoVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplaySignalsInfoVGrain"/> class.
        /// </summary>
        public DisplaySignalsInfoVGrain(ILogger<IDisplaySignalsInfoVGrain> logger,
                                    IOptionsMonitor<DebugDisplayInfoOptions>? options = null)
            : base(logger, options)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public Task DisplaySignalInfoAsync(SignalMessage signal, IExecutionContext ctx)
        {
            return DisplayInfoAsync(ctx, nameof(IExecutionContext), signal, nameof(signal)).AsTask();
        }

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public async Task ReceiveSignalAsync(SignalMessage signal)
        {
            await DisplayInfoAsync(signal, nameof(signal));
        }

        #endregion
    }
}
