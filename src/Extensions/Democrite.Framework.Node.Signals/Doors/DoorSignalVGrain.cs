// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Signals;
    using Democrite.Framework.Node.Signals;

    using Microsoft.Extensions.Logging;

    using Orleans.Placement;
    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Signal virtual grain that is fire Door when activate.
    /// </summary>
    /// <seealso cref="IDoorSignalVGrain" />
    [PreferLocalPlacement] // Try place the signal near it's door
    [DemocriteSystemVGrain]
    internal sealed class DoorSignalVGrain : BaseSignalVGrain<IDoorSignalVGrain>, IDoorSignalVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorSignalVGrain"/> class.
        /// </summary>
        public DoorSignalVGrain(ILogger<IDoorSignalVGrain> logger,
                                [PersistentState("Signals", nameof(Democrite))] IPersistentState<SignalHandlerStateSurrogate> persistentState,
                                IGrainFactory grainFactory,
                                IRemoteGrainServiceFactory remoteGrainServiceFactory)
            : base(logger, persistentState, grainFactory, remoteGrainServiceFactory)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<Guid> Fire(SignalMessage signal)
        {
            return FireSignal(signal);
        }

        #endregion
    }
}
