﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations.Builders
{
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;

    /// <summary>
    /// Wizard used to setup memory configuration, vgrain state, reminder, triggers, ...
    /// </summary>
    /// <seealso cref="IBuilderDemocriteBaseWizard" />
    public interface IDemocriteNodeMemoryBuilder : IBuilderDemocriteBaseWizard, IDemocriteBaseGenericBuilder
    {
        /// <summary>
        /// Uses InMemory storage to save vgrain states.
        /// </summary>
        IDemocriteNodeMemoryBuilder UseInMemoryVGrainStateMemory();

        /// <summary>
        /// Uses InMemory storage to save trigger/reminder information.
        /// </summary>
        IDemocriteNodeMemoryBuilder UseInMemoryTriggerReminderMemory();
    }
}
