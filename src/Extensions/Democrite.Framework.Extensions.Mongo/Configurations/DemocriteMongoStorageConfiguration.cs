// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Configurations
{
    using Democrite.Framework.Configurations;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Extensions.Mongo.Configurations.AutoConfigurator;
    using Democrite.Framework.Extensions.Mongo.Services;
    using Democrite.Framework.Node.Abstractions.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using MongoDB.Bson.Serialization;

    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Configurator dedicated to mongo db storage
    /// </summary>
    internal sealed class DemocriteMongoStorageConfiguration : IDemocriteMongoStorageConfiguration
    {
        #region Fields

        private readonly IDemocriteNodeMemoryBuilder _nodeBuilder;
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        private MongoDBOptions? _connectionOption;
        private string? _currentConnectionString;

        #endregion

        #region Ctor

        public DemocriteMongoStorageConfiguration(IDemocriteNodeMemoryBuilder nodeBuilder,
                                                  IServiceCollection serviceCollection,
                                                  IConfiguration configuration)
        {
            this._nodeBuilder = nodeBuilder;
            this._serviceCollection = serviceCollection;
            this._configuration = configuration;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the connection information state.
        /// </summary>
        public IDemocriteMongoStorageConfiguration ConnectionString(string connectionString,
                                                                    string defaultDatabase,
                                                                    string? collectionPrefix = null)
        {
            this._currentConnectionString = connectionString;
            this._connectionOption = new MongoDBOptions()
            {
                CollectionPrefix = collectionPrefix,
                DatabaseName = defaultDatabase
            };

            return this;
        }

        /// <summary>
        /// Get a storage dedicated to <see cref="StorageTypeEnum"/>.
        /// </summary>
        public IDemocriteMongoStorageConfiguration Store(StorageTypeEnum storageType,
                                                         string? customDatabase = null,
                                                         string? collectionPrefix = null)
        {
            if ((storageType & StorageTypeEnum.Default) == StorageTypeEnum.Default)
            {
                var option = GenerateOptions<MongoDBRemindersOptions>(customDatabase, collectionPrefix);

                AutoDefaultMemoryMongoConfigurator.Default
                                                  .ConfigureDefaultMongoStorage(this._nodeBuilder,
                                                                                this._configuration,
                                                                                this._serviceCollection,
                                                                                this._nodeBuilder.Logger,
                                                                                this._currentConnectionString,
                                                                                option);
            }

            if ((storageType & StorageTypeEnum.Democrite) == StorageTypeEnum.Democrite)
            {
                var option = GenerateOptions<MongoDBOptions>(customDatabase, collectionPrefix);

                AutoDemocriteMongoConfigurator.Default
                                              .ConfigureMongoStorage(this._nodeBuilder,
                                                                     this._configuration,
                                                                     this._serviceCollection,
                                                                     this._nodeBuilder.Logger,
                                                                     this._currentConnectionString,
                                                                     option);
            }

            if ((storageType & StorageTypeEnum.Reminders) == StorageTypeEnum.Reminders)
            {
                var option = GenerateOptions<MongoDBRemindersOptions>(customDatabase, collectionPrefix);

                AutoReminderMongoConfigurator.ConfigureMongoReminderStorage(this._nodeBuilder,
                                                                            this._configuration,
                                                                            this._serviceCollection,
                                                                            this._nodeBuilder.Logger,
                                                                            this._currentConnectionString,
                                                                            option);
            }

            if ((storageType & StorageTypeEnum.Repositories) == StorageTypeEnum.Repositories)
            {
                var option = GenerateOptions<MongoDBOptions>(customDatabase, collectionPrefix);

                AutoCustomRepositoryMongoConfigurator.Default.ConfigureMongoStorages(this._nodeBuilder,
                                                                                     this._configuration,
                                                                                     this._serviceCollection,
                                                                                     this._nodeBuilder.Logger,
                                                                                     this._currentConnectionString,
                                                                                     option,
                                                                                     DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey.AsEnumerable().ToReadOnly());
            }

            return this;
        }

        private TOptionType GenerateOptions<TOptionType>(string? customDatabase, string? collectionPrefix)
            where TOptionType : MongoDBOptions, new()
        {
            var option = MongoConfigurator.MapOption<TOptionType>(this._connectionOption);

            option.DatabaseName = string.IsNullOrEmpty(customDatabase)
                                            ? option.DatabaseName
                                            : customDatabase;

            option.CollectionPrefix = string.IsNullOrEmpty(collectionPrefix)
                                            ? option.CollectionPrefix
                                            : collectionPrefix;
            return option;
        }

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        public IDemocriteMongoStorageConfiguration CustomStorage(string key,
                                                                 string? customDatabase = null,
                                                                 string? collectionPrefix = null,
                                                                 bool buildRepository = false)
        {
            return CustomStorage(key.AsEnumerable().ToArray(), customDatabase, collectionPrefix, buildRepository);
        }

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        public IDemocriteMongoStorageConfiguration CustomStorage(IReadOnlyCollection<string> keys,
                                                                 string? customDatabase = null,
                                                                 string? collectionPrefix = null,
                                                                 bool buildRepository = false)
        {
            var option = GenerateOptions<MongoDBOptions>(customDatabase, collectionPrefix);

            AutoCustomGrainMemoryMongoConfigurator.Default.ConfigureMongoStorages(this._nodeBuilder,
                                                                                  this._configuration,
                                                                                  this._serviceCollection,
                                                                                  this._nodeBuilder.Logger,
                                                                                  this._currentConnectionString,
                                                                                  option,
                                                                                  buildRepository,
                                                                                  keys);

            return this;
        }

        /// <inheritdoc />
        public IDemocriteMongoStorageConfiguration CustomRepositoryStorage(string key,
                                                                           string? customDatabase = null,
                                                                           string? collectionPrefix = null)
        {
            return CustomRepositoryStorage(key.AsEnumerable().ToArray(), customDatabase, collectionPrefix);
        }

        /// <inheritdoc />
        public IDemocriteMongoStorageConfiguration CustomRepositoryStorage(IReadOnlyCollection<string> keys,
                                                                           string? customDatabase = null,
                                                                           string? collectionPrefix = null)
        {
            var option = GenerateOptions<MongoDBOptions>(customDatabase, collectionPrefix);

            AutoCustomRepositoryMongoConfigurator.Default.ConfigureMongoStorages(this._nodeBuilder,
                                                                                 this._configuration,
                                                                                 this._serviceCollection,
                                                                                 this._nodeBuilder.Logger,
                                                                                 this._currentConnectionString,
                                                                                 option,
                                                                                 keys);

            return this;
        }

        /// <summary>
        /// Adds a bson convert.
        /// </summary>
        public IDemocriteMongoStorageConfiguration AddConvert<TTarget>(IBsonSerializer<TTarget> bsonSerializer)
        {
            BsonSerializer.RegisterSerializer<TTarget>(bsonSerializer);
            return this;
        }

        #endregion
    }
}
