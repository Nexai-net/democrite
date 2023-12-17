// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Mongo.Services
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Cluster.Abstractions.Exceptions;
    using Democrite.Framework.Cluster.Mongo.Converters;
    using Democrite.Framework.Cluster.Mongo.Models;
    using Democrite.Framework.Toolbox;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Serializers;
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

        private static readonly Dictionary<IServiceCollection, MongoConfigurator> s_instances;

        private const string URI_TYPE = "mongodb://";

        private static MongoDBOptions? s_lastOptionSetups;
        private static int s_connectionInfoOrder;
        private static long s_defaultMongoServices;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoConfigurator"/> class.
        /// </summary>
        static MongoConfigurator()
        {
            s_instances = new Dictionary<IServiceCollection, MongoConfigurator>();
        }

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
            if (s_instances.TryGetValue(serviceCollection, out var cfg))
                return cfg;

            var newCfg = new MongoConfigurator();
            s_instances.Add(serviceCollection, newCfg);
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
                option.DatabaseName = s_lastOptionSetups?.DatabaseName ?? nameof(Democrite).ToLower();

            if (string.IsNullOrEmpty(connectionString))
                connectionOption.ConnectionString = configuration.GetSection(connectionStringSectionPath).Get<string>();
            else
                connectionOption.ConnectionString = connectionString;

            s_lastOptionSetups = MongoConfigurator.MapOption<MongoDBOptions>(option);

            var defaultOpt = serviceCollection.FirstOrDefault(s => s.ServiceType == typeof(MongoDBConnectionOptions) &&
                                                                   s.ImplementationInstance is MongoDBConnectionOptions opt &&
                                                                   string.Equals(opt.Key, key));

            if (defaultOpt != null && defaultOpt.ImplementationInstance is MongoDBConnectionOptions saveMongoDBConnectionOptions)
                connectionOption.Order = saveMongoDBConnectionOptions.Order;
            else
                connectionOption.Order = s_connectionInfoOrder++;

            if (defaultOpt != null)
                serviceCollection.Remove(defaultOpt);

            serviceCollection.AddSingleton(connectionOption);

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
        private static void SetupDefaultMongoServices(IServiceCollection serviceCollection)
        {
            if (Interlocked.Increment(ref s_defaultMongoServices) > 1)
                return;

            serviceCollection.AddSingleton<IMongoClientFactory, MultipleMongoClientFactory>();
            serviceCollection.AddSingleton<IGrainStateSerializer, BsonGrainStateSerializer>();

            BsonSerializer.TryRegisterSerializer(NoneTypeStringConverter.Instance);
            BsonSerializer.TryRegisterSerializer(new ObjectSerializer(t => true));
            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));
        }

        #endregion
    }
}
