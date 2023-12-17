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
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeStorages = ConfigurationSectionNames.Root + ConfigurationSectionNames.SectionSeparator + "Storages";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDefaultMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Default";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDefaultMemoryAutoConfigKey = NodeDefaultMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node default memory connection string
        /// </summary>
        public const string NodeDefaultMemoryConnectionString = NodeDefaultMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The section configuring auto key for all memory when misisin
        /// </summary>
        public const string NodeMemoryDefaultAutoConfigKey = NodeDefaultMemory + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The section configuring the custom memory storage
        /// </summary>
        public const string NodeCustomMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Custom";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDemocriteSystemMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "System";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDemocriteSystemMemoryAutoConfigKey = NodeDemocriteSystemMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node democrite system memory connection string
        /// </summary>
        public const string NodeDemocriteSystemMemoryConnectionString = NodeDemocriteSystemMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeReminderStateMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Reminders";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeReminderStateMemoryConnectionString = NodeReminderStateMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeReminderStateMemoryAutoConfigKey = NodeReminderStateMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;
    }
}
