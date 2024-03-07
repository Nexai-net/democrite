// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using System;

    /// <summary>
    /// Define the type of controller managed by a blackboard
    /// </summary>
    [Flags]
    public enum BlackboardControllerTypeEnum
    {
        None,

        /// <summary>
        /// Controller in charge to managed issue around storage (data conflict, bad format, historisation, invalid ...)
        /// </summary>
        Storage = 1,

        /// <summary>
        /// Controller in charge to managed reaction to blackboard event (New data, data removed, timer, job done, ...)
        /// </summary>
        Event = 2,

        /// <summary>
        /// Controller in charge to evaluate the blackboard state (In processing, Job done, In failure, missing information, ...)
        /// </summary>
        State = 4
    }
}
