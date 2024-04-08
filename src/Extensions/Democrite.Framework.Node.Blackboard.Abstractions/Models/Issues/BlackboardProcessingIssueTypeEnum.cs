// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models.Issues
{
    /// <summary>
    /// Define the main issue categories
    /// </summary>
    [Flags]
    public enum BlackboardProcessingIssueTypeEnum
    {
        None,
        Storage = 1,
        Rule = 2,
        Type = 4,
        Aggregate = 8,
        NotSupported = 16,
    }

    /// <summary>
    /// Define the issue type related to storage process
    /// </summary>
    public enum BlackboardProcessingIssueStorageTypeEnum
    {
        None,
        Limits,
        Conflict,
        Format
    }
}
