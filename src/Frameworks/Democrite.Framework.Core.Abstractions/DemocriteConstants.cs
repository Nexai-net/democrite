// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite
{
    /// <summary>
    /// Define constants use by Blackboard extensions
    /// </summary>
    public static class DemocriteConstants
    {
        #region Fields

        public const string CSHARP_SYSTEM_NAMESPACE = "system";
        public const string SYSTEM_VGRAIN_NAMESPACE = "democrite.system";

        #endregion

        #region ctor

        /// <summary>
        /// Initializes the <see cref="DemocriteConstants"/> class.
        /// </summary>
        static DemocriteConstants()
        {
            DefaultObserverTimeout = TimeSpan.FromMinutes(5);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The default dmocrite state storage configuration
        /// </summary>
        public const string DefaultDefinitionStorageName = "Definitions";

        /// <summary>
        /// The default dmocrite state storage configuration
        /// </summary>
        public const string DefaultDemocriteStateConfigurationKey = nameof(Democrite);

        /// <summary>
        /// The default dmocrite dynamic definitions
        /// </summary>
        public const string DefaultDemocriteDynamicDefinitionsConfigurationKey = nameof(Democrite) + "DynamicDefinitions";

        /// <summary>
        /// The default dmocrite dynamic definitions
        /// </summary>
        public const string DefaultDemocriteDynamicDefinitionsRepositoryConfigurationKey = DefaultDemocriteDynamicDefinitionsConfigurationKey + "Repository";

        /// <summary>
        /// The default dmocrite System state storage configuration
        /// </summary>
        public const string DefaultDemocriteAdminStateConfigurationKey = nameof(Democrite) + "Admin";

        /// <summary>
        /// The default dmocrite repository storage configuration
        /// </summary>
        public const string DefaultDemocriteRepositoryConfigurationKey = nameof(Democrite) + "Repository";

        /// <summary>
        /// Gets the default observer timeout.
        /// </summary>
        public static TimeSpan DefaultObserverTimeout { get; }

        #endregion
    }
}
