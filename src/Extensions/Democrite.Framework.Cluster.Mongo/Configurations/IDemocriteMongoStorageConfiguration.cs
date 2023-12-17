// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Mongo.Configurations
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
        /// Create a storage dedicated to <see cref="StorageTypeEnum"/>.
        /// </summary>
        IDemocriteMongoStorageConfiguration Store(StorageTypeEnum storageType,
                                                  string? customDatabase = null,
                                                  string? collectionPrefix = null);

        /// <summary>
        /// Create a storage dedicated to specific key.
        /// </summary>
        IDemocriteMongoStorageConfiguration Store(string? customDatabase = null,
                                                  string? collectionPrefix = null,
                                                  params string[] keys);

        /// <summary>
        /// Adds a bson convert.
        /// </summary>
        IDemocriteMongoStorageConfiguration AddConvert<TTarget>(IBsonSerializer<TTarget> bsonSerializer);
    }
}
