// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Doors
{
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// <see cref="DoorDefinition"/> provider
    /// </summary>
    /// <seealso cref="IProviderStrategy{DoorDefinition, Guid}" />
    public interface IDoorDefinitionProvider : IProviderStrategy<DoorDefinition, Guid>
    {
    }
}
