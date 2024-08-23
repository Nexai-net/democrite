// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Repositories
{
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Storages;

    using Elvex.Toolbox.Abstractions.Supports;
    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Supports;

    using Orleans.Providers;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Generic repository proxy to support access to data based on specialized access
    /// </summary>
    internal class ReadOnlyRepository<TEntity, TEntityId> : SafeDisposable, IReadOnlyRepository<TEntity, TEntityId>, ISupportInitialization<string>, ISupportDebugDisplayName
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        #region Fields

        private readonly SupportInitializationImplementation<string> _initImpl;
        private readonly IRepositorySpecificFactory _repositorySpecificFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _configurationName;
        private readonly string _storageName;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyRepository{TEntity, TEntityId}"/> class.
        /// </summary>
        public ReadOnlyRepository(IRepositorySpecificFactory repositorySpecificFactory,
                                  IServiceProvider serviceProvider,
                                  RepositoryGetOptions request)
        {
            this._initImpl = new SupportInitializationImplementation<string>(OnInitRepoAsync);

            this._repositorySpecificFactory = repositorySpecificFactory;
            this._configurationName = request.ConfigurationName!;
            this._serviceProvider = serviceProvider;

            if (string.IsNullOrEmpty(request.StorageName))
                request = request.WithStorageName(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            this._storageName = request.StorageName;
            this.Request = request;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the request used to get this repository
        /// </summary>
        protected RepositoryGetOptions Request { get; }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return this.SpecializedRepository?.IsReadOnly ?? true; }
        }

        /// <inheritdoc />
        public bool SupportExpressionFilter
        {
            get { return this.SpecializedRepository?.SupportExpressionFilter ?? false; }
        }

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return this._initImpl.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return this._initImpl.IsInitialized; }
        }

        /// <summary>
        /// Gets the specialized repository.
        /// </summary>
        protected IReadOnlyRepository<TEntity, TEntityId>? SpecializedRepository { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<TEntity?> GetFirstValueAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            CheckSupportExpressionFilter();

            return await this.SpecializedRepository!.GetFirstValueAsync(filterExpression, token);
        }

        /// <inheritdoc />
        public async ValueTask<TProjection?> GetFirstValueAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            CheckSupportExpressionFilter();

            return await this.SpecializedRepository!.GetFirstValueAsync<TProjection>(filterExpression, token);
        }

        /// <inheritdoc />
        public async ValueTask<TEntity?> GetValueByIdAsync([NotNull] TEntityId entityId, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);

            return await this.SpecializedRepository!.GetValueByIdAsync(entityId, token);
        }

        /// <inheritdoc />
        public async ValueTask<TProjection?> GetValueByIdAsync<TProjection>([NotNull] TEntityId entityId, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            return await this.SpecializedRepository!.GetValueByIdAsync<TProjection>(entityId, token);
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TProjection>> GetValueByIdAsync<TProjection>([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            return await this.SpecializedRepository!.GetValueByIdAsync<TProjection>(entityIds, token);
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TEntity>> GetValueByIdAsync([NotNull] IReadOnlyCollection<TEntityId> entityIds, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            return await this.SpecializedRepository!.GetValueByIdAsync(entityIds, token);
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TProjection>> GetValuesAsync<TProjection>([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            CheckSupportExpressionFilter();

            return await this.SpecializedRepository!.GetValuesAsync<TProjection>(filterExpression, token);
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyCollection<TEntity>> GetValuesAsync([AllowNull] Expression<Func<TEntity, bool>> filterExpression, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            CheckSupportExpressionFilter();

            return await this.SpecializedRepository!.GetValuesAsync(filterExpression, token);
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(string? initializationState, CancellationToken token = default)
        {
            return this._initImpl.InitializationAsync(initializationState, token);
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return this._initImpl.InitializationAsync(token);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            var relayName = (this._repositorySpecificFactory as ISupportDebugDisplayName)?.ToDebugDisplayName() ?? this._repositorySpecificFactory?.ToString() ?? "null";
            return "[Repository Proxy] => " + relayName;
        }

        #region Tools

        /// <inheritdoc cref="ISupportInitialization.InitializationAsync(CancellationToken)" />
        protected virtual async ValueTask OnInitRepoAsync(string? storageName, CancellationToken token)
        {
            var localRequest = this.Request;

            if (!string.IsNullOrEmpty(storageName))
                localRequest = localRequest.WithStorageName(storageName);

            if (this is not IRepository<TEntity, TEntityId> && localRequest.IsReadOnly == false)
                localRequest = localRequest.WithIsReadOnly(true);

            this.SpecializedRepository = this._repositorySpecificFactory.Get<IReadOnlyRepository<TEntity, TEntityId>, TEntity, TEntityId>(this._serviceProvider,
                                                                                                                                          localRequest);

            if (this.SpecializedRepository is ISupportInitialization<string> init)
                await init.InitializationAsync(storageName);
            else if (this.SpecializedRepository is ISupportInitialization<IServiceProvider> initServicePro)
                await initServicePro.InitializationAsync(this._serviceProvider);
            else if (this.SpecializedRepository is ISupportInitialization initDefault)
                await initDefault.InitializationAsync();
        }

        /// <summary>
        /// Checks if the repository is initialized.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async ValueTask CheckIsInitializedAsync(CancellationToken token, [CallerMemberName] string? callerName = null)
        {
            if (this.SpecializedRepository is null)
            {
                await InitializationAsync(this._storageName, token);
                if (this.SpecializedRepository is not null)
                    return;

                throw new InvalidOperationException("Repository not initialized, {0} is not allowed".WithArguments(callerName));
            }
        }

        /// <summary>
        /// Checks if the repository is initialized.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckSupportExpressionFilter([CallerMemberName] string? callerName = null)
        {
            if (this.SupportExpressionFilter == false)
                throw new InvalidOperationException("Repository does not support filtering by expression, {0} is not allowed".WithArguments(callerName));
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Repository providing access, push and delete
    /// </summary>
    internal sealed class Repository<TEntity, TEntityId> : ReadOnlyRepository<TEntity, TEntityId>, IRepository<TEntity, TEntityId>
        where TEntity : IEntityWithId<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        #region Fields

        private IRepository<TEntity, TEntityId>? _repo;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity, TEntityId}"/> class.
        /// </summary>
        public Repository(IRepositorySpecificFactory repositorySpecificFactory,
                          IServiceProvider serviceProvider,
                          RepositoryGetOptions request)
            : base(repositorySpecificFactory, serviceProvider, request)
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> DeleteRecordAsync(TEntityId uid, CancellationToken cancellationToken)
        {
            await CheckIsInitializedAsync(cancellationToken);
            CheckReadOnly();

            return await this._repo!.DeleteRecordAsync(uid, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteRecordAsync(IReadOnlyCollection<TEntityId> uids, CancellationToken cancellationToken)
        {
            await CheckIsInitializedAsync(cancellationToken);
            CheckReadOnly();

            return await this._repo!.DeleteRecordAsync(uids, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> PushDataRecordAsync(TEntity entity, bool insertIfNew, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            CheckReadOnly();

            return await this._repo!.PushDataRecordAsync(entity, insertIfNew, token);
        }

        /// <inheritdoc />
        public async Task<int> PushDataRecordAsync(IReadOnlyCollection<TEntity> entities, bool insertIfNew, CancellationToken token)
        {
            await CheckIsInitializedAsync(token);
            CheckReadOnly();

            return await this._repo!.PushDataRecordAsync(entities, insertIfNew, token);
        }

        #region Tools

        /// <inheritdoc />
        protected override async ValueTask OnInitRepoAsync(string? storageName, CancellationToken token)
        {
            await base.OnInitRepoAsync(storageName, token);

            this._repo = this.SpecializedRepository as IRepository<TEntity, TEntityId>;
        }

        /// <summary>
        /// Checks the read only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckReadOnly([CallerMemberName] string? callerName = null)
        {
            if (this.IsReadOnly)
                throw new InvalidOperationException("Repository in readonly access, {0} is not allowed".WithArguments(callerName));
        }

        #endregion

        #endregion
    }
}
