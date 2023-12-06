// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Node.Abstractions.Configurations;
    using Democrite.Framework.Node.Abstractions.Models;

    /// <summary>
    /// Extensions method about node configuration
    /// </summary>
    public static class IClusterNodeBuilderDemocriteWizardExtensions
    {
        /// <summary>
        /// Adds the diagnostic options.
        /// </summary>
        public static IDemocriteNodeWizard AddDiagnosticOptions(this IDemocriteNodeWizard wizard, ClusterNodeDiagnosticOptions option)
        {
            wizard.AddNodeOption<ClusterNodeDiagnosticOptions>(option);
            return wizard;
        }

        /// <summary>
        /// Adds the diagnostic options.
        /// </summary>
        public static IDemocriteNodeWizard AddDiagnosticOptions(this IDemocriteNodeWizard wizard, string configuratioSection)
        {
            wizard.AddNodeOption<ClusterNodeDiagnosticOptions>(configuratioSection);
            return wizard;
        }

        /// <summary>
        /// Adds the endpoint options used to define how node open through network.
        /// </summary>
        public static IDemocriteNodeWizard AddEndpointOptions(this IDemocriteNodeWizard wizard, ClusterNodeEndPointOptions option)
        {
            wizard.AddNodeOption<ClusterNodeEndPointOptions>(option);
            return wizard;
        }

        /// <summary>
        /// Adds the endpoint options used to define how node open through network.
        /// </summary>
        public static IDemocriteNodeWizard AddEndpointOptions(this IDemocriteNodeWizard wizard, string configuratioSection)
        {
            wizard.AddNodeOption<ClusterNodeEndPointOptions>(configuratioSection);
            return wizard;
        }
    }
}
