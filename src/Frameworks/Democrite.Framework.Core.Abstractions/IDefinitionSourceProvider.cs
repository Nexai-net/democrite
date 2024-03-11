// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider used to provide any definition
    /// </summary>
    public interface IDefinitionSourceProvider<TDefinition> : IProviderStrategySource<TDefinition, Guid>
        where TDefinition : class, IDefinition

    {
    }
}
