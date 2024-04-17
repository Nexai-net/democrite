// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Get simple signal
    /// </summary>
    /// <seealso cref="ISignalNetworkBasePartBuilder{ISignalBuilder, SignalDefinition}" />
    public interface ISignalBuilder : ISignalNetworkBasePartBuilder<ISignalBuilder>, IDefinitionBaseBuilder<SignalDefinition>
    {
        /// <summary>
        /// Specific the hierarchy parent signal. All message received by the child will automatically be relay to parent
        /// </summary>
        /// <remarks>This is usefull to filter signal by creating a dedicated sub signal and register only to it.</remarks>
        ISignalBuilder Parent(SignalDefinition parent);
    }
}
