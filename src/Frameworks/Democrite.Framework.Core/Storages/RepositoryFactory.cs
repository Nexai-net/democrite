// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Storages
{
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;

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
        public TTargetRepo Get<TTargetRepo, TEntity>(string stateName, string? storageName = null) where TTargetRepo : IReadOnlyRepository<TEntity>
        {
            var storageConfig = string.IsNullOrEmpty(storageName)
                                            ? DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey
                                            : storageName;

            var specificFactory = this._serviceProvider.GetServiceByKey<string, IRepositorySpecificFactory>(storageConfig)
                                  ?? this._serviceProvider.GetServiceByKey<string, IRepositorySpecificFactory>(DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey)
                                  ?? this._serviceProvider.GetServiceByKey<string, IRepositorySpecificFactory>("Default");

            if (specificFactory is null)
            {
                // TODO : Get Default if null
                throw new Exception();
            }

            return specificFactory.Get<TTargetRepo, TEntity>(this._serviceProvider, stateName);
        }

        #endregion
    }
}