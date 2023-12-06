// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Cron
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Services;

    /// <summary>
    /// Virtual Grain Service in charge to managed all cron grain instance.
    /// </summary>
    [DemocriteSystemVGrain]
    public interface ICronVGrainService : IGrainService
    {

    }
}
