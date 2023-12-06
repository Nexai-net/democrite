// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations
{
    /// <summary>
    /// Define the configuration section name used by democrite
    /// </summary>
    public static class ConfigurationSectionNames
    {
        /// <summary>
        /// The section's name separator
        /// </summary>
        public const string SectionSeparator = ":";

        /// <summary>
        /// The property name used to defined the auto configuration type 
        /// </summary>
        public const string AutoConfigKey = "AutoConfig";

        /// <summary>
        /// The property name used to defined the connection String
        /// </summary>
        public const string ConnectionString = "ConnectionString";

        /// <summary>
        /// The default auto property value
        /// </summary>
        public const string DefaultAutoConfigKey = "Default";

        /// <summary>
        /// The root section containing all the democrite configuration
        /// </summary>
        public const string Root = "Democrite";

        /// <summary>
        /// The section name in charge to define all the dll to load as extensions
        /// </summary>
        public const string ExtensionSection = Root + SectionSeparator + "Extensions";

        /// <summary>
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeStorages = Root + SectionSeparator + "Storages";

        /// <summary>
        /// The section configuring the membership tables storages
        /// </summary>
        public const string NodeStorageMembership = NodeStorages + SectionSeparator + "Memberships";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeVGrainStateMemoryMembership = NodeStorages + SectionSeparator + "VGrainStates";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeVGrainStateMemoryMembershipAutoConfigKey = NodeVGrainStateMemoryMembership + SectionSeparator + AutoConfigKey;

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeReminderStateMemoryMembership = NodeStorages + SectionSeparator + "ReminderStates";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeReminderStateMemoryMembershipAutoConfigKey = NodeReminderStateMemoryMembership + SectionSeparator + AutoConfigKey;

        /// <summary>
        /// The section configuring the membership tables storages
        /// </summary>
        public const string NodeStorageMembershipAutoConfigKey = NodeStorageMembership + SectionSeparator + AutoConfigKey;

        /// <summary>
        /// The section configuring the membership tables storages
        /// </summary>
        public const string NodeStorageMembershipConnectionStringKey = NodeStorageMembership + SectionSeparator + ConnectionString;
    }
}
