// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Services
{
    using Democrite.Framework.Cluster.Abstractions.Exceptions;
    using Democrite.Framework.Extensions.Mongo.Abstractions;
    using Democrite.Framework.Extensions.Mongo.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using MongoDB.Driver;

    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.StorageProviders.Serializers;
    using Orleans.Providers.MongoDB.Utils;

    using System;

    /// <summary>
    /// Define configuration used by all mongo configuration
    /// </summary>
    internal sealed class MongoConfigurator
    {
        #region Fields

        private const string URI_TYPE = "mongodb://";

        private MongoDBOptions? _lastOptionSetups;
        private int _connectionInfoOrder;
        private long _defaultMongoServices;

        #endregion

        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="MongoConfigurator"/> class from being created.
        /// </summary>
        private MongoConfigurator()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the instance deficated to <paramref name="serviceCollection"/>
        /// </summary>
        /// <remarks>
        ///     This allow unit test isolation context
        /// </remarks>
        public static MongoConfigurator GetInstance(IServiceCollection serviceCollection)
        {
            var mongoCfg = serviceCollection.GetServiceByKey<string, MongoConfigurator>(nameof(MongoConfigurator));

            if (mongoCfg is not null && mongoCfg.KeyedImplementationInstance is MongoConfigurator cfg)
                return cfg;

            var newCfg = new MongoConfigurator();
            serviceCollection.TryAddKeyedSingleton<MongoConfigurator>(nameof(MongoConfigurator), newCfg);
            return newCfg;
        }

        /// <summary>
        /// Setups the mongo connection information for the management of multiple targets
        /// </summary>
        public void SetupMongoConnectionInformation(IServiceCollection serviceCollection,
                                                    IConfiguration configuration,
                                                    MongoDBOptions option,
                                                    string key,
                                                    string connectionStringSectionPath,
                                                    string? connectionString = null)
        {
            var connectionOption = MongoConfigurator.MapOption<MongoDBConnectionOptions>(option);
            connectionOption.Key = key;

            if (string.IsNullOrEmpty(option.DatabaseName))
                option.DatabaseName = _lastOptionSetups?.DatabaseName ?? nameof(Democrite).ToLower();

            if (string.IsNullOrEmpty(connectionString))
                connectionOption.ConnectionString = configuration.GetSection(connectionStringSectionPath).Get<string>();
            else
                connectionOption.ConnectionString = connectionString;

            _lastOptionSetups = MongoConfigurator.MapOption<MongoDBOptions>(option);

            var defaultOpt = serviceCollection.FirstOrDefault(s => s.IsKeyedService == false &&
                                                                   s.ServiceType == typeof(MongoDBConnectionOptions) &&
                                                                   s.ImplementationInstance is MongoDBConnectionOptions opt &&
                                                                   string.Equals(opt.Key, key));

            if (defaultOpt != null && defaultOpt.ImplementationInstance is MongoDBConnectionOptions saveMongoDBConnectionOptions)
                connectionOption.Order = saveMongoDBConnectionOptions.Order;
            else
                connectionOption.Order = _connectionInfoOrder++;

            if (defaultOpt != null)
                serviceCollection.Remove(defaultOpt);

            serviceCollection.AddSingleton(connectionOption);
            serviceCollection.AddKeyedSingleton<MongoDBConnectionOptions>(key, connectionOption);// (p, n) => connectionOption);

            SetupDefaultMongoServices(serviceCollection);
        }

        /// <summary>
        /// Normalizes the connection string.
        /// </summary>
        public static string CheckAndNormalizeConnectionString(string connectionString)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(connectionString);

            if (!connectionString.StartsWith(URI_TYPE))
                connectionString = URI_TYPE + connectionString;

            if (!Uri.TryCreate(connectionString, UriKind.Absolute, out _))
            {
                throw new InvalidFormatDemocriteConfigurationException("Mongo." + nameof(connectionString),
                                                                       connectionString,
                                                                       URI_TYPE + "...@HOST:PORT?OPTIONS");
            }

            return connectionString;
        }

        /// <summary>
        /// Maps option to another one, if null try using the default
        /// </summary>
        internal static TOption MapOption<TOption>(MongoDBOptions? source = null)
            where TOption : MongoDBOptions, new()
        {
            return new TOption()
            {
                ClientName = source?.ClientName,
                CollectionConfigurator = source?.CollectionConfigurator,
                CollectionPrefix = source?.CollectionPrefix,
                CreateShardKeyForCosmos = source?.CreateShardKeyForCosmos ?? false,
                DatabaseName = source?.DatabaseName,
            };
        }

        /// <summary>
        /// Setups the default mongo services.
        /// </summary>
        private void SetupDefaultMongoServices(IServiceCollection serviceCollection)
        {
            if (Interlocked.Increment(ref _defaultMongoServices) > 1)
                return;

            serviceCollection.AddSingleton<IMongoClientFactory, MultipleMongoClientFactory>();
            serviceCollection.AddSingleton<IGrainStateSerializer, BsonGrainStateSerializer>();

            DemocriteMongoDefaultSerializerConfig.SetupSerializationConfiguration();

            //BsonSerializer.TryRegisterSerializer(NoneTypeStringConverter.Instance);
            //BsonSerializer.TryRegisterSerializer(new ObjectSerializer(t => true));
            //BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));
            //BsonSerializer.TryRegisterSerializer(new TypeSerializer());
        }

        #endregion
    }
}
