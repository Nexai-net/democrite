// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Storages
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;

    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Global repository factory
    /// </summary>
    /// <seealso cref="IRepositoryFactory" />
    internal sealed class RepositoryFactory : IRepositoryFactory
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFactory"/> class.
        /// </summary>
        public RepositoryFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<TTargetRepo> GetAsync<TTargetRepo, TEntity>(string stateName, string? storageName = null, bool blockInitialization = false, CancellationToken token = default)
            where TTargetRepo : IReadOnlyRepository<TEntity>
        {
            var storageConfig = string.IsNullOrEmpty(storageName)
                                            ? DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey
                                            : storageName;

            var specificFactory = this._serviceProvider.GetKeyedService<IRepositorySpecificFactory>(storageConfig)
                                  ?? this._serviceProvider.GetKeyedService<IRepositorySpecificFactory>(DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey)
                                  ?? this._serviceProvider.GetKeyedService<IRepositorySpecificFactory>("Default");

            if (specificFactory is null)
            {
                // TODO : Get Default if null
                throw new Exception();
            }

            var repo = specificFactory.Get<TTargetRepo, TEntity>(this._serviceProvider, stateName);

            if (repo is null)
                throw new KeyNotFoundException($"Could build a dedicated repository with following pair stateName:'{stateName}' storageName:'{storageName}'");

            if (repo is ISupportInitialization initialization && !initialization.IsInitialized)
            {
                if (repo is ISupportInitialization<string> initWithStateName)
                    await initWithStateName.InitializationAsync(storageName, token);
                else
                    await initialization.InitializationAsync(token);
            }

            return repo;
        }

        #endregion
    }
}