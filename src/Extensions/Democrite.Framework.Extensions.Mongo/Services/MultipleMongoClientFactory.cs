// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Mongo.Services
{
    using Amazon.Runtime.Internal.Util;

    using Democrite.Framework.Extensions.Mongo.Models;

    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using MongoDB.Driver;

    using Orleans.Providers;
    using Orleans.Providers.MongoDB.Utils;

    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// <see cref="IMongoClientFactory"/> use to allow storage to use different db
    /// </summary>
    /// <seealso cref="IMongoClientFactory" />
    internal class MultipleMongoClientFactory : SafeDisposable, IMongoClientFactory
    {
        #region Fields

        private readonly IReadOnlyDictionary<string, MongoDBConnectionOptions> _configuredConnections;
        private readonly IMongoClient? _defaultClient;

        private readonly Dictionary<string, IMongoClient> _connectionCacheFromConnectionString;
        private readonly Dictionary<string, IMongoClient> _connectionCacheFromRequestName;
        private readonly MongoDBConnectionOptions _rootDefaultOptions;

        private readonly ReaderWriterLockSlim _locker;
        private readonly ILogger<MultipleMongoClientFactory> _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleMongoClientFactory"/> class.
        /// </summary>
        public MultipleMongoClientFactory(IEnumerable<MongoDBConnectionOptions> configuredConnections,
                                          IMongoClient? defaultClient = null,
                                          ILogger<MultipleMongoClientFactory>? logger = null)
        {
            this._connectionCacheFromRequestName = new Dictionary<string, IMongoClient>(StringComparer.OrdinalIgnoreCase);
            this._connectionCacheFromConnectionString = new Dictionary<string, IMongoClient>();
            this._locker = new ReaderWriterLockSlim();

            this._logger = logger ?? NullLogger<MultipleMongoClientFactory>.Instance;

            this._defaultClient = defaultClient;
            this._configuredConnections = configuredConnections.GroupBy(c => c.Key?.ToLower() ?? string.Empty)
                                                               .ToDictionary(k => k.Key, v => v.Select(vv => vv).Last(), StringComparer.OrdinalIgnoreCase);

            this._rootDefaultOptions = ExtractDefaultOption(this._configuredConnections) ?? MongoDBConnectionOptions.Default;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IMongoClient Create(string name)
        {
            try
            {
                this._locker.EnterReadLock();
                try
                {
                    if (this._connectionCacheFromRequestName.TryGetValue(name, out var cachedClient))
                        return cachedClient;
                }
                finally
                {
                    this._locker.ExitReadLock();
                }

                string? connectionString = null;

                if (this._configuredConnections.TryGetValue(name, out var connection))
                {
                    connectionString = connection.ConnectionString;
                }

                if (string.IsNullOrEmpty(connectionString))
                    connectionString = this._rootDefaultOptions?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidDataException("Connection is empty for storage '" + name + "'");

                var client = GetConnectionFromConfigString(connectionString);

                this._locker.EnterWriteLock();
                try
                {
                    if (this._connectionCacheFromRequestName.TryGetValue(name, out var cachedClient))
                        return cachedClient;

                    this._connectionCacheFromRequestName.Add(name, client);
                    return client;
                }
                finally
                {
                    this._locker.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Error, "Mongo Client failed config '{configurationName}' => {exception}", name, ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the connection associate configuration string from cache or build new one
        /// </summary>
        private IMongoClient GetConnectionFromConfigString(string configurationString)
        {
            this._locker.EnterReadLock();
            try
            {
                if (this._connectionCacheFromConnectionString.TryGetValue(configurationString, out var cachedClient))
                    return cachedClient;
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            this._locker.EnterWriteLock();
            try
            {
                if (this._connectionCacheFromConnectionString.TryGetValue(configurationString, out var cachedClient))
                    return cachedClient;

                var newClient = InstanciateMongoClient(configurationString);
                this._connectionCacheFromConnectionString.Add(configurationString, newClient);

                return newClient;
            }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Instanciates the mongo client.
        /// </summary>
        protected virtual MongoClient InstanciateMongoClient(string configurationString)
        {
            configurationString = MongoConfigurator.CheckAndNormalizeConnectionString(configurationString);

            var settings = MongoClientSettings.FromUrl(MongoUrl.Create(configurationString));
            settings.MaxConnecting = settings.MaxConnectionPoolSize / 3;
            settings.MinConnectionPoolSize = settings.MaxConnectionPoolSize / 3;

            return new MongoClient(settings);
        }

        /// <summary>
        /// Extracts the default option with a connection string.
        /// </summary>
        private static MongoDBConnectionOptions? ExtractDefaultOption(IReadOnlyDictionary<string, MongoDBConnectionOptions> configuredConnections)
        {
            if (configuredConnections.TryGetValue(MongoDBConnectionOptions.DEMOCRITE_DEFAULT, out var defaultDemocriteOpt) &
                defaultDemocriteOpt != null &&
                !string.IsNullOrEmpty(defaultDemocriteOpt!.ConnectionString))
            {
                return defaultDemocriteOpt;
            }
            else if (configuredConnections.TryGetValue(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, out var defaultOpt) &&
                     defaultOpt != null &&
                     !string.IsNullOrEmpty(defaultOpt.ConnectionString))
            {
                return defaultOpt;
            }

            if (configuredConnections.TryGetValue(MongoDBConnectionOptions.DEMOCRITE_CLUSTER, out var defaultClusterOpt) &&
                defaultClusterOpt != null &&
                !string.IsNullOrEmpty(defaultClusterOpt.ConnectionString))
            {
                return defaultClusterOpt;
            }

            // Search for first connections string recorded
            return configuredConnections.Where(c => !string.IsNullOrEmpty(c.Value.ConnectionString))
                                        .OrderBy(c => c.Value.Order)
                                        .FirstOrDefault().Value;
        }

        #endregion
    }
}
