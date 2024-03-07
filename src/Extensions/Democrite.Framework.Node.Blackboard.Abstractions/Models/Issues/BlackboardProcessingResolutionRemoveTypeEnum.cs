// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues
{
    /// <summary>
    ///  Define the remove behavior wanted
    /// </summary>
    public enum BlackboardProcessingResolutionRemoveTypeEnum
    {
        None,
        Remove,
        Decommission,
        // Archived -> Remove and carry in the event the data, that allow the Trigger Controller to choose to set data in another place
        // Move -> Change the logical type
    }
}
