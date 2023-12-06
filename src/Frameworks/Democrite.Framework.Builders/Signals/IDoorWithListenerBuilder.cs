// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Builder of <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="ISignalNetworkBasePartBuilder{ISignalBuilder}" />
    public interface IDoorWithListenerBuilder : IDoorBuilder
    {
        #region Properties

        /// <inheritdoc cref="ISignalNetworkBasePartBuilder{TWizard, TDefinition}.Group(string)"/>
        string? GroupName { get; }

        /// <summary>
        /// Gets unique door name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        Guid Uid { get; }

        /// <summary>
        /// Gets the signal ids.
        /// </summary>
        IReadOnlyCollection<SignalId> SignalIds { get; }

        /// <summary>
        /// Gets the door ids.
        /// </summary>
        IReadOnlyCollection<DoorId> DoorIds { get; }

        #endregion
    }
}
