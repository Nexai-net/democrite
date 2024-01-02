// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider source of <see cref="SignalDefinition"/>
    /// </summary>
    public interface ISignalDefinitionProviderSource : IProviderStrategySource<SignalDefinition, Guid>, INodeInitService
    {
    }
}
