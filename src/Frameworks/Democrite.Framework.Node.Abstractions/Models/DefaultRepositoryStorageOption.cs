// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Models
{
    /// <summary>
    /// Option about in memory custom storage
    /// </summary>
    public record class DefaultRepositoryStorageOption(string Key, bool AllowWrite = true, bool TargetGrainState = false);
}
