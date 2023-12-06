// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Cron
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Node.Abstractions.Triggers;

    /// <summary>
    /// Handler in charge to manager trigger using con expression as source
    /// </summary>
    /// <remarks>
    ///     By default the grain id match the trigger definition.
    /// </remarks>
    [DemocriteSystemVGrain]
    public interface ICronTriggerHandlerVGrain : ITriggerHandlerVGrain, IGrainWithGuidKey
    {

    }
}
