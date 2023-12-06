// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals.Doors
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Services;

    /// <summary>
    /// Virtual Grain Service used to managed door grain through the cluster.
    /// </summary>
    /// <seealso cref="IGrainService" />
    [DemocriteSystemVGrain]
    internal interface IDoorVGrainService : IGrainService
    {
    }
}
