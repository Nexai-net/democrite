// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Signals
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Orleans.Concurrency;

    /// <summary>
    /// VGrain in charge to managed output signal of the <see cref="IDoorVGrain"/>
    /// </summary>
    /// <seealso cref="ISignalHandler" />
    /// <seealso cref="IGrainWithGuidKey" />
    internal interface IDoorSignalVGrain : IVGrain, ISignalHandler, IGrainWithGuidKey
    {
        /// <summary>
        /// Fires a signal build by <see cref="DoorBaseVGrain"/>
        /// </summary>
        [OneWay]
        Task<Guid> Fire(SignalMessage signal);
    }
}
