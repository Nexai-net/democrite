// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Virtual Grain in charge of one signal fire and subsciption
    /// </summary>
    /// <seealso cref="IGrainWithGuidKey" />
    [DemocriteSystemVGrain]
    internal interface ISignalHandlerVGrain : ISignalVGrain, IGrainWithGuidKey, IVGrain
    {
        /// <summary>
        /// Initializes the signal handler
        /// </summary>
        ValueTask InitializeAsync(GrainCancellationToken token);
    }
}
