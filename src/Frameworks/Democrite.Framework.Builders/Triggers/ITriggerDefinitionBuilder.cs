// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Common part of trigger definition
    /// </summary>
    public interface ITriggerDefinitionBuilder
    {
        /// <summary>
        /// Adds the targe id. Sample:  Sequence.Uid
        /// </summary>
        ITriggerDefinitionFinalizeBuilder AddTargetSequence(Guid targetSequenceId);

        /// <summary>
        /// Adds the targe id. Sample:  Sequence.Uid
        /// </summary>
        ITriggerDefinitionFinalizeBuilder AddTarget(SequenceDefinition sequenceDefinition);

        /// <summary>
        /// Adds a signal as target
        /// </summary>
        ITriggerDefinitionFinalizeBuilder AddTargetSignal(SignalId signalId);

    }
}
