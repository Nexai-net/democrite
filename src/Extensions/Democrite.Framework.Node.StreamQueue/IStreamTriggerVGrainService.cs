// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Services;

    /// <summary>
    /// Grain service in charge to handled trigger link to a stream
    /// </summary>
    /// <seealso cref="IGrainService" />
    [DemocriteSystemVGrain]
    public interface IStreamTriggerVGrainService : IGrainService
    {
    }
}
