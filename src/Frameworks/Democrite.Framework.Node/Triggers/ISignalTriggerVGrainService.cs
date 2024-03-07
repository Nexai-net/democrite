// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core.Abstractions.Attributes;

    using Orleans.Services;

    /// <summary>
    /// FullKeys service use to setup and manager trigger from signal
    /// </summary>
    [DemocriteSystemVGrain]
    internal interface ISignalTriggerVGrainService : IGrainService
    {
    }
}
