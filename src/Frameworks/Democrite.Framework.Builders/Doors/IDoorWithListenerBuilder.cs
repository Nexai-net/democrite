// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Doors
{
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Builder of <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="ISignalNetworkBasePartBuilder{ISignalBuilder}" />
    public interface IDoorWithListenerBuilder : IDoorBuilder
    {
        #region Properties

        /// <summary>
        /// Gets unique door name
        /// </summary>
        string? DisplayName { get; }

        /// <summary>
        /// Gets the simple name identifier.
        /// </summary>
        string SimpleNameIdentifier { get; }    

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        Guid Uid { get; }

        /// <summary>
        /// Gets the definition meta data.
        /// </summary>
        DefinitionMetaData? DefinitionMetaData { get; }

        /// <summary>
        /// Gets the global signal retention period (history and not consumed), Default 1 day
        /// </summary>
        TimeSpan? RetentionMaxPeriod { get; }

        /// <summary>
        /// Gets the signal history maximum retention. Default 0 to prevent any signal history
        /// </summary>
        uint? HistoryMaxRetention { get; }

        /// <summary>
        /// Gets the signal "not consumed" maximum retiention. Default null to keep all the incoming signals
        /// </summary>
        uint? NotConsumedMaxRetiention { get; }

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
