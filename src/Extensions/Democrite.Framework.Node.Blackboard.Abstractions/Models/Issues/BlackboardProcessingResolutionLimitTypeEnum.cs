// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues
{
    /// <summary>
    /// Define the classic limit resolution 
    /// </summary>
    public enum BlackboardProcessingResolutionLimitTypeEnum
    {
        None,

        /// <summary>
        /// The reject the add if limit is reach
        /// </summary>
        Reject,

        /// <summary>
        /// The keep newest element created and remove the other to let a slot for the new entry
        /// </summary>
        KeepNewest,

        /// <summary>
        /// The keep newest element updated and remove the other to let a slot for the new entry
        /// </summary>
        KeepNewestUpdated,

        /// <summary>
        /// The keep oldest element created and remove the other to let a slot for the new entry
        /// </summary>
        KeepOldest,

        /// <summary>
        /// The keep oldest element update and remove the other to let a slot for the new entry
        /// </summary>
        KeepOldestUpdated,

        /// <summary>
        /// When limit it reach remove all the past value
        /// </summary>
        ClearAll,
    }
}
