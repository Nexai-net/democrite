// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
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
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeRepositoryStorages = ConfigurationSectionNames.Root + ConfigurationSectionNames.SectionSeparator + "Repositories";

        /// <summary>
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeRepositoryStoragesDefaultAutoConfigKey = NodeRepositoryStoragesDefault + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeRepositoryStoragesDefault = NodeRepositoryStorages + ConfigurationSectionNames.SectionSeparator + "Default";

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
        public const string NodeCustomMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Customs";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDemocriteAdminMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Admin";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDemocriteAdminMemoryAutoConfigKey = NodeDemocriteAdminMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node democrite system memory connection string
        /// </summary>
        public const string NodeDemocriteAdminMemoryConnectionString = NodeDemocriteAdminMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDemocriteMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Democrite";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDemocriteMemoryAutoConfigKey = NodeDemocriteMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node democrite system memory connection string
        /// </summary>
        public const string NodeDemocriteMemoryConnectionString = NodeDemocriteMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

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

        /// <summary>
        /// The root section in change to configured the definitions provider
        /// </summary>
        public const string NodeDefinitionProvider = ConfigurationSectionNames.Root + ConfigurationSectionNames.SectionSeparator + "Definitions";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDemocriteDynamicDefinitionsMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "DynamicDefinitions";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDemocriteDynamicDefinitionsMemoryAutoConfigKey = NodeDemocriteDynamicDefinitionsMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node democrite system memory connection string
        /// </summary>
        public const string NodeDemocriteDynamicDefinitionsMemoryConnectionString = NodeDemocriteDynamicDefinitionsMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;
    }
}
