// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations
{
    using Democrite.Framework.Core.Abstractions.Triggers;

    /// <summary>
    /// Setup local triggers
    /// </summary>
    public interface IDemocriteNodeTriggersWizard
    {
        /// <summary>
        /// Registers a trigger definition.
        /// </summary>
        IDemocriteNodeTriggersWizard Register(ITriggerDefinition triggerDefinition);
    }
}
