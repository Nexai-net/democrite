// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Setup local signals
    /// </summary>
    public interface IDemocriteNodeSignalsWizard
    {
        /// <summary>
        /// Registers a signals definition.
        /// </summary>
        IDemocriteNodeSignalsWizard Register(params SignalDefinition[] signalDefinition);
    }
}
