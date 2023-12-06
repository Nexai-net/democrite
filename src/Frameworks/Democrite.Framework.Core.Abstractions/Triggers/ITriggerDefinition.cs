// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Define trigger information
    /// </summary>
    public interface ITriggerDefinition
    {
        /// <summary>
        /// Gets the uid.
        /// </summary>
        Guid Uid { get; }

        /// <summary>
        /// Gets the type of the trigger.
        /// </summary>
        TriggerTypeEnum TriggerType { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ITriggerDefinition"/> is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets the trigger target sequence ids.
        /// </summary>
        IReadOnlyCollection<Guid> TargetSequenceIds { get; }

        /// <summary>
        /// Gets the trigger target signal ids.
        /// </summary>
        IReadOnlyCollection<SignalId> TargetSignalIds { get; }

        /// <summary>
        /// Gets the input provider definition send when trigger fire.
        /// </summary>
        InputSourceDefinition? InputSourceDefinition { get; }
    }
}
