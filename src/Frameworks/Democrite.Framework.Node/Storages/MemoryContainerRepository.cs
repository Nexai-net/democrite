// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using Democrite.Framework.Core.Abstractions.Repositories;

    using Elvex.Toolbox.Abstractions.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Orleans.Configuration;

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    /// <seealso cref="IReadOnlyRepository{TEntity}" />
    [DebuggerDisplay("Memory Container Repository {s_entityTraits}[{s_entityIdTraits}] : {_storageName}")]
    internal class MemoryContainerRepository<TContainer, TEntity, TEntityId, TContainerId> : MemoryRepository<TEntity, TEntityId>, IReadOnlyRepository<TContainer, TContainerId>, IRepository<TContainer, TContainerId>
        where TEntity : IEntityWithId<TEntityId>
        where TContainer : IEntityWithId<TContainerId>
        where TContainerId : notnull, IEquatable<TContainerId>
        where TEntityId : notnull, IEquatable<TEntityId>

    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        public MemoryContainerRepository(string storageName,
                                         string configurationName,
                                         IGrainFactory grainFactory,
                                         IOptionsMonitor<MemoryGrainStorageOptions> grainStorageOptions,
                                         IDemocriteSerializer democriteSerializer,
                                         IDedicatedObjectConverter dedicatedObjectConverter,
                                         ILogger<IRepository<TEntity, TEntityId>> logger,
                                         bool readOnly)
            : base(storageName, configurationName, grainFactory, grainStorageOptions, democriteSerializer, dedicatedObjectConverter, logger, readOnly)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<bool> DeleteRecordAsync(TContainerId uid, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> DeleteRecordAsync(IReadOnlyCollection<TContainerId> entityIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<TContainer?> GetFirstValueAsync([AllowNull] Expression<Func<TContainer, bool>> filterExpression, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<TProjection?> GetFirstValueAsync<TProjection>([AllowNull] Expression<Func<TContainer, bool>> filterExpression, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<TContainer?> GetValueByIdAsync([NotNull] TContainerId entityId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<TProjection?> GetValueByIdAsync<TProjection>([NotNull] TContainerId entityId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<TProjection>> GetValueByIdAsync<TProjection>([NotNull] IReadOnlyCollection<TContainerId> entityIds, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<TContainer>> GetValueByIdAsync([NotNull] IReadOnlyCollection<TContainerId> entityIds, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>([AllowNull] Expression<Func<TContainer, bool>> filterExpression, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyCollection<TContainer>> GetValuesAsync([AllowNull] Expression<Func<TContainer, bool>> filterExpression, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> PushDataRecordAsync(TContainer entity, bool insertIfNew, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<int> PushDataRecordAsync(IReadOnlyCollection<TContainer> entity, bool insertIfNew, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        #region Tools

        /// <inheritdoc />
        protected override TProjection OnMap<TProjection, TSource>(in TSource? source, string objJson) where TSource : default
        {
            return base.OnMap<TProjection, TSource>(source, objJson);
        }

        #endregion

        #endregion
    }
}