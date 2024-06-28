// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Cron
{
    using Democrite.Framework.Node.Abstractions.Triggers;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Cron Reminder state used to store information and cache to help <see cref="CronTriggerHandlerVGrain"/> to perform
    /// </summary>
    [GenerateSerializer]
    public sealed class CronReminderState : TriggerState
    {
    }
}
