// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Configurations
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    /// <summary>
    /// Builder use to configure in memory blackboard template definition
    /// </summary>
    public interface IDemocriteNodeBlackboardsLocalDefinitionBuilder
    {
        /// <summary>
        /// Adds the templates.
        /// </summary>
        IDemocriteNodeBlackboardsLocalDefinitionBuilder SetupTemplates(params BlackboardTemplateDefinition[] templates);
    }
}
