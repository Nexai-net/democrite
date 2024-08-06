// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Core.Abstractions.Enums;

    /// <summary>
    /// Builder used to configure all storage in cluster memory
    /// </summary>
    public interface IDemocriteMemoryInLocalBuilder
    {
        #region Methods

        /// <summary>
        /// Configure storage for <see cref="StorageTypeEnum"/>
        /// </summary>
        IDemocriteMemoryInLocalBuilder Store(StorageTypeEnum storageType);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupGrainStateStorage(string key,
                                                              bool buildRepository = false);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupGrainStateStorage(IReadOnlyCollection<string> keys,
                                                              bool buildRepository = false);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupRepositoryStorage(string key);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupRepositoryStorage(IReadOnlyCollection<string> keys);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupDefaultStorage();

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupDefaultRepositoryStorage();

        /// <summary>
        /// Setups the democrite dynamic definition storage.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupDynamicDefinitionStorage();

        /// <summary>
        /// Setups the democrite admin system storage.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupDemocriteAdminStorage();

        /// <summary>
        /// Setups the democrite system storage.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupDemocriteStorage();

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IDemocriteMemoryInLocalBuilder SetupReminderStorage();

        #endregion
    }
}
