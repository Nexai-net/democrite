// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Setup local doors
    /// </summary>
    public interface IDemocriteNodeDoorsWizard
    {
        /// <summary>
        /// Registers a signals definition.
        /// </summary>
        IDemocriteNodeDoorsWizard Register(DoorDefinition signalDefinition);
    }
}
