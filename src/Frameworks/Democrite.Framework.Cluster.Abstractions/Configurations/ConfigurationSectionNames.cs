// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
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
        public const string AutoConfigKey = "AutoKey";

        /// <summary>
        /// The property name used to defined if repository must be build from a grain storage
        /// </summary>
        public const string BuildRepository = "BuildRepository";

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
        /// The section configuring the cluster meeting point db
        /// </summary>
        public const string ClusterMembership = Root + SectionSeparator + "Cluster";

        /// <summary>
        /// The section configuring thhe cluster meeting point db auto config key
        /// </summary>
        public const string ClusterMembershipAutoConfigKey = ClusterMembership + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The section configuring thhe cluster meeting point db connection string
        /// </summary>
        public const string ClusterMembershipConnectionStringKey = ClusterMembership + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The endpoints section name configuration
        /// </summary>
        public const string Endpoints = Root + SectionSeparator + "Endpoint";
    }
}
