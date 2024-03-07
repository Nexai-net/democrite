// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations
{
    using Democrite.Framework.Core.Abstractions.Enums;

    using MongoDB.Bson.Serialization;

    /// <summary>
    /// Configurator dedicated to mongo db storage
    /// </summary>
    public interface IDemocriteMongoStorageConfiguration
    {
        /// <summary>
        /// Set the connection information state.
        /// </summary>
        IDemocriteMongoStorageConfiguration ConnectionString(string connectionString,
                                                             string defaultDatabase,
                                                             string? collectionPrefix = null);

        /// <summary>
        /// Get a storage dedicated to <see cref="StorageTypeEnum"/>.
        /// </summary>
        IDemocriteMongoStorageConfiguration Store(StorageTypeEnum storageType,
                                                  string? customDatabase = null,
                                                  string? collectionPrefix = null);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageConfiguration CustomStorage(string key,
                                                          string? customDatabase = null,
                                                          string? collectionPrefix = null,
                                                          bool buildRepository = false);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageConfiguration CustomStorage(IReadOnlyCollection<string> keys,
                                                          string? customDatabase = null,
                                                          string? collectionPrefix = null,
                                                          bool buildRepository = false);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageConfiguration CustomRepositoryStorage(string key,
                                                                    string? customDatabase = null,
                                                                    string? collectionPrefix = null);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageConfiguration CustomRepositoryStorage(IReadOnlyCollection<string> keys,
                                                                    string? customDatabase = null,
                                                                    string? collectionPrefix = null);

        /// <summary>
        /// Adds a bson convert.
        /// </summary>
        IDemocriteMongoStorageConfiguration AddConvert<TTarget>(IBsonSerializer<TTarget> bsonSerializer);
    }
}
