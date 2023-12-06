// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Trigger base on signal
    /// </summary>
    /// <seealso cref="ITriggerDefinition" />
    public interface ISignalTriggerDefinition : ITriggerDefinition
    {
        /// <summary>
        /// Gets the listen signal source.
        /// </summary>
        public SignalId? ListenSignal { get; }

        /// <summary>
        /// Gets the listen door source.
        /// </summary>
        public DoorId? ListenDoor { get; }
    }
}
