// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Build simple signal
    /// </summary>
    /// <seealso cref="ISignalNetworkBasePartBuilder{ISignalBuilder, SignalDefinition}" />
    public interface ISignalBuilder : ISignalNetworkBasePartBuilder<ISignalBuilder>, IDefinitionBaseBuilder<SignalDefinition>
    {
    }
}
