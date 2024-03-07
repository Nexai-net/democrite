// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Nexai.Sample.Forex.VGrain.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// VGrain used to suscribe to signal or doord to test 
    /// </summary>
    /// <seealso cref="IVGrain" />
    public interface ISignalDemoTargetVGrain : IVGrain, ISignalReceiver, IGrainWithGuidKey
    {
        Task SubscribeToAsync(SignalDefinition signalDefinition);

        Task SubscribeToAsync(DoorDefinition doorDefinition);
    }
}
