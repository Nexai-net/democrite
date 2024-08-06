// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Extensions.Mongo.Services;

    using MongoDB.Bson.Serialization;

    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Configurator dedicated to mongo db storage
    /// </summary>
    public interface IDemocriteMongoStorageBuilder
    {
        #region Methods

        /// <summary>
        /// Set the connection information state.
        /// </summary>
        IDemocriteMongoStorageBuilder ConnectionString(string connectionString,
                                                             string defaultDatabase,
                                                             string? collectionPrefix = null);

        /// <summary>
        /// Configure storage for <see cref="StorageTypeEnum"/>
        /// </summary>
        IDemocriteMongoStorageBuilder Store(StorageTypeEnum storageType,
                                            string? connectionString = null,
                                            string? customDatabase = null,
                                            string? collectionPrefix = null);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupGrainStateStorage(string key,
                                                             string? connectionString = null,
                                                             string? customDatabase = null,
                                                             string? collectionPrefix = null,
                                                             bool buildRepository = false);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupGrainStateStorage(IReadOnlyCollection<string> keys,
                                                             string? connectionString = null,
                                                             string? customDatabase = null,
                                                             string? collectionPrefix = null,
                                                             bool buildRepository = false);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupRepositoryStorage(string key,
                                                             string? connectionString = null,
                                                             string? customDatabase = null,
                                                             string? collectionPrefix = null);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupRepositoryStorage(IReadOnlyCollection<string> keys,
                                                             string? connectionString = null,
                                                             string? customDatabase = null,
                                                             string? collectionPrefix = null);

        /// <summary>
        /// Adds a bson convert.
        /// </summary>
        IDemocriteMongoStorageBuilder AddConvert<TTarget>(IBsonSerializer<TTarget> bsonSerializer);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupDefaultStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupDefaultRepositoryStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the democrite dynamic definition storage.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupDynamicDefinitionStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the democrite admin system storage.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupDemocriteAdminStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the democrite system storage.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupDemocriteStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupReminderStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IDemocriteMongoStorageBuilder SetupDefinitionStorage(string key, string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the mongo cluster.
        /// </summary>
        void SetupMongoCluster(IDemocriteClusterBuilder cl,
                               string? connectionString = null,
                               MongoDBMembershipTableOptions? option = null,
                               string? serviceId = null,
                               string? clusterId = null);

        #endregion
    }
}
