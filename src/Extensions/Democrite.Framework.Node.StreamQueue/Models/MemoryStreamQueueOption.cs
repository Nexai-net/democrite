// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue.Models
{
    /// <summary>
    /// Option used to create a memory steam queue
    /// </summary>
    public record class MemoryStreamQueueOption(string ConfigContainerName, bool CreateInMemoryPubSubStorage);
}
