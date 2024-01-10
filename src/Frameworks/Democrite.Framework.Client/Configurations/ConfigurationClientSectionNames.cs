// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Client.Configurations
{
    using Democrite.Framework.Client.Abstractions.Models;
    using Democrite.Framework.Configurations;

    /// <summary>
    /// Define the configuration section name used by democrite's node
    /// </summary>
    public static class ConfigurationClientSectionNames
    {
        /// <summary>
        /// The endpoints section name configuration
        /// </summary>
        public const string Cluster = ConfigurationSectionNames.Root +
                                      ConfigurationSectionNames.SectionSeparator +
                                      ClusterClientOptions.ConfiguratioName;
    }
}
