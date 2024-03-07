// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Targets
{
    /// <summary>
    /// Define the type of action wish throught <see cref="IDataRecordRequest"/>
    /// </summary>
    public enum DataRecordPushRequestTypeEnum
    {
        None,

        /// <summary>
        /// The push create new or update
        /// </summary>
        Push,

        /// <summary>
        /// The update only if the record already exists
        /// </summary>
        UpdateOnly,

        /// <summary>
        /// The push new only if the record don't exist
        /// </summary>
        OnlyNew,
    }
}
