// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Node.Abstractions.Models;

    /// <summary>
    /// Specific configuration related to node only
    /// </summary>
    /// <seealso cref="IDemocriteCoreConfigurationWizard" />
    public interface IDemocriteNodeConfigurationWizard : IDemocriteCoreConfigurationWizard<IDemocriteNodeConfigurationWizard>
    {
        /// <summary>
        /// Adds the diagnostic options.
        /// </summary>
        IDemocriteNodeConfigurationWizard AddDiagnostic(ClusterNodeDiagnosticOptions options);

        /// <summary>
        /// Enables the diagnostic log to be relay to classic logger.
        /// </summary>
        IDemocriteNodeConfigurationWizard EnableDiagnosticRelayToLogger();
    }
}
