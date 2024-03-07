// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    /// <summary>
    /// Define the type of trigger
    /// </summary>
    public enum TriggerTypeEnum
    {
        None,
        Manual,
        Cron,
        Signal,
        Other,
        Stream
    }
}
