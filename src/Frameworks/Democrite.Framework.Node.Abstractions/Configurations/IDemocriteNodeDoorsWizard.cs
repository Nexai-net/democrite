// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Core.Abstractions.Doors;

    /// <summary>
    /// Setup local doors
    /// </summary>
    public interface IDemocriteNodeDoorsWizard
    {
        /// <summary>
        /// Registers a signals definition.
        /// </summary>
        IDemocriteNodeDoorsWizard Register(params DoorDefinition[] signalDefinition);
    }
}
