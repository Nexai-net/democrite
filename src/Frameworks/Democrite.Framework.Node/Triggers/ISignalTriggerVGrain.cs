// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Abstractions.Triggers;

    /// <summary>
    /// Virtual grain in charge to managed one trigger
    /// </summary>
    [DemocriteSystemVGrain]
    internal interface ISignalTriggerVGrain : ITriggerHandlerVGrain, IGrainWithGuidKey, ISignalReceiver, ISignalReceiverReadOnly
    {
    }
}
