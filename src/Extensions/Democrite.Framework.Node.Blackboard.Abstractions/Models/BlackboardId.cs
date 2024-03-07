// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions.Models
{
    using System;

    /// <summary>
    /// Define a blackboard target information
    /// </summary>
    [GenerateSerializer]
    public record struct BlackboardId(Guid Uid, string BoardName, string BoardTemplateKey);
}
