// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Node.Abstractions.Models;

    /// <summary>
    /// Define the configuration section name used by democrite's node
    /// </summary>
    public static class ConfigurationNodeSectionNames
    {
        /// <summary>
        /// The node specific section
        /// </summary>
        public const string NodeSpecificSection = "Node";

        /// <summary>
        /// The diagnostics section name configuration
        /// </summary>
        public const string Diagnostics = ConfigurationSectionNames.Root +
                                          ConfigurationSectionNames.SectionSeparator +
                                          NodeSpecificSection +
                                          ConfigurationSectionNames.SectionSeparator +
                                          ClusterNodeDiagnosticOptions.ConfiguratioName;

        /// <summary>
        /// The endpoints section name configuration
        /// </summary>
        public const string Endpoints = ConfigurationSectionNames.Root +
                                        ConfigurationSectionNames.SectionSeparator +
                                        NodeSpecificSection +
                                        ConfigurationSectionNames.SectionSeparator +
                                        ClusterNodeEndPointOptions.ConfiguratioName;
    }
}
