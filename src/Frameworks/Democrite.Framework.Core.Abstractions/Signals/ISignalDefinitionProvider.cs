// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// <see cref="SignalDefinition"/> provider
    /// </summary>
    /// <seealso cref="IProviderStrategy{SignalDefinition, Guid}" />
    public interface ISignalDefinitionProvider : IProviderStrategy<SignalDefinition, Guid>
    {
    }
}
