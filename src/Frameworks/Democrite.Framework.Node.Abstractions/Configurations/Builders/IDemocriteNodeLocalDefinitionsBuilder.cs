﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using System;

    /// <summary>
    /// Builder used to setup in node local memory all the elements trigger, cron, signals, artefacts, ...
    /// </summary>
    /// <remarks>
    ///     Attention the element define each are mainly define in the local node. <br />
    ///     Except if the other node declare the same definitions they will not be able on cluster
    /// </remarks>
    public interface IDemocriteNodeLocalDefinitionsBuilder
    {
        /// <summary>
        /// Setup callable sequence
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupSequences(Action<IDemocriteNodeSequenceWizard> config);

        /// <summary>
        /// Setup callable sequence
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupSequences(params SequenceDefinition[] sequences);

        /// <summary>
        /// Setups the triggers.
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupTriggers(Action<IDemocriteNodeTriggersWizard> config);

        /// <summary>
        /// Setups the triggers.
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupTriggers(params TriggerDefinition[] triggers);

        /// <summary>
        /// Setups the signals.
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupSignals(Action<IDemocriteNodeSignalsWizard> config);

        /// <summary>
        /// Setups the signals.
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupSignals(params SignalDefinition[] signals);

        /// <summary>
        /// Setups the doors.
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupDoors(Action<IDemocriteNodeDoorsWizard> config);

        /// <summary>
        /// Setups the doors.
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupDoors(params DoorDefinition[] doors);

        /// <summary>
        /// Setups the artifact resource, external source code (python, c++, exe, dll, png, ...)
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupArtifacts(Action<IDemocriteNodeArtifactsWizard> artifactResourceBuilder);

        /// <summary>
        /// Setups artifacts resources, external source code (python, c++, exe, dll, png, ...)
        /// </summary>
        IDemocriteNodeLocalDefinitionsBuilder SetupArtifacts(params ArtifactDefinition[] artifactResourceDefinitions);
    }
}
