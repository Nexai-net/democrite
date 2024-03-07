// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    /// <summary>
    /// 
    /// </summary>
    public enum DataRecordConflictReasonEnum
    {
        None,

        /// <summary>
        /// The already exist with the same Id
        /// </summary>
        AlreadyExist,

        /// <summary>
        /// The maximum limit reach
        /// </summary>
        MaxLimit,

        /// <summary>
        /// Custom reason more info in the detail area
        /// </summary>
        Other,
    }
}
