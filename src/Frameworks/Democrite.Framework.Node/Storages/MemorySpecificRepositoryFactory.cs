// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;

    using Elvex.Toolbox.Abstractions.Models;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    using Orleans.Configuration;
    using Orleans.Runtime;

    using System;

    public sealed class MemorySpecificRepositoryFactory : IRepositorySpecificFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemorySpecificRepositoryFactory"/> class.
        /// </summary>
        public MemorySpecificRepositoryFactory(string? configurationName = null)
        {

        }

        /// <inheritdoc />
        public IReadOnlyRepository<TEntity, TEntityId> Get<TTargetRepo, TEntity, TEntityId>(IServiceProvider serviceProvider, RepositoryGetOptions request)
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            return new MemoryRepository<TEntity, TEntityId>(request.StorageName,
                                                            request.ConfigurationName!,
                                                            serviceProvider.GetRequiredService<IGrainFactory>(),
                                                            serviceProvider.GetRequiredService<IOptionsMonitor<MemoryGrainStorageOptions>>(),
                                                            serviceProvider.GetRequiredService<IDemocriteSerializer>(),
                                                            serviceProvider.GetRequiredService<IDedicatedObjectConverter>(),
                                                            serviceProvider.GetService<ILogger<IRepository<TEntity, TEntityId>>>() ?? NullLogger<IRepository<TEntity, TEntityId>>.Instance,
                                                            request.IsReadOnly);
        }
    }

    public sealed class MemorySpecificStateContainerFactory : IRepositorySpecificFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemorySpecificStateContainerFactory"/> class.
        /// </summary>
        public MemorySpecificStateContainerFactory(string? configurationName = null)
        {

        }

        /// <inheritdoc />
        public IReadOnlyRepository<TEntity, TEntityId> Get<TTargetRepo, TEntity, TEntityId>(IServiceProvider serviceProvider, RepositoryGetOptions request)
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : IEquatable<TEntityId>
        {
            /*
             * In Memory storage the principe use registry to found where the data are.
             * The registry master correctly dissocate repository registry and Grain state registry
             * 
             * The repository behavior is the same except Grain state repository can only read data and not update them
             */
            return new MemoryRepository<TEntity, TEntityId>(request.StorageName,
                                                            request.ConfigurationName!,
                                                            serviceProvider.GetRequiredService<IGrainFactory>(),
                                                            serviceProvider.GetRequiredService<IOptionsMonitor<MemoryGrainStorageOptions>>(),
                                                            serviceProvider.GetRequiredService<IDemocriteSerializer>(),
                                                            serviceProvider.GetRequiredService<IDedicatedObjectConverter>(),
                                                            serviceProvider.GetService<ILogger<IRepository<TEntity, TEntityId>>>() ?? NullLogger<IRepository<TEntity, TEntityId>>.Instance,
                                                            true);
        }
    }
}
