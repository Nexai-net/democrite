// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Enums;
    using Democrite.Framework.Core.Abstractions.Models;
    using Democrite.Framework.Core.Abstractions.Models.SignalMessage;
    using Democrite.Framework.Core.Abstractions.Repositories;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Storages;
    using Democrite.Framework.Extensions.Mongo.Models;
    using Democrite.Framework.Node.Models;

    using Elvex.Toolbox.Abstractions.Conditions;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc cref="IDynamicDefinitionHandlerVGrain" />
    [KeepAlive]
    internal sealed class DynamicDefinitionHandlerVGrain : VGrainBase<DynamicDefinitionHandlerState, DynamicDefinitionHandlerStateSurrogate, DynamicDefinitionHandlerConverter, IDynamicDefinitionHandlerVGrain>, IDynamicDefinitionHandlerVGrain
    {
        #region Fields

        private readonly Dictionary<ConcretType, IRepository<EtagDefinitionContainer, Guid>> _repositoryCache;
        private readonly ReaderWriterLockSlim _repositoriyCacheLock;
        private readonly ISignalService _signalService;

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDefinitionHandlerVGrain"/> class.
        /// </summary>
        public DynamicDefinitionHandlerVGrain(ILogger<IDynamicDefinitionHandlerVGrain> logger,
                                              [PersistentState("DynamicDefinitions", DemocriteConstants.DefaultDemocriteStateConfigurationKey)] IPersistentState<DynamicDefinitionHandlerStateSurrogate> persistentState,
                                              IRepositoryFactory repositoryFactory,
                                              ITimeManager timeManager,
                                              ISignalService signalService)
            : base(logger, persistentState)
        {
            this._repositoriyCacheLock = new ReaderWriterLockSlim();
            this._repositoryCache = new Dictionary<ConcretType, IRepository<EtagDefinitionContainer, Guid>>();

            this._signalService = signalService;
            this._timeManager = timeManager;
            this._repositoryFactory = repositoryFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<string> GetHandlerEtagAsync()
        {
            return Task.FromResult(this.State!.Etag);
        }

        /// <inheritdoc />
        public async Task<EtagContainer<IReadOnlyCollection<TDefinition>>> GetDefinitionAsync<TDefinition>(GrainCancellationToken token, IReadOnlyCollection<Guid> uids) where TDefinition : IDefinition
        {
            var fetchTasks = await ApplyActionOn(uids, (r, ids, t) => r.GetByIdsValueAsync(ids, t).AsTask(), token.CancellationToken);

            var results = fetchTasks.Where(t => t.IsCompletedSuccessfully)
                                    .SelectMany(t => t.Result ?? EnumerableHelper<EtagDefinitionContainer>.ReadOnly)
                                    .Select(container => container.GetContainDefinition<TDefinition>())
                                    .Distinct()
                                    .ToArray();

            return new EtagContainer<IReadOnlyCollection<TDefinition>>(this.State!.Etag, results);
        }

        /// <inheritdoc />
        public Task<EtagContainer<IReadOnlyCollection<DynamicDefinitionMetaData>>> GetDynamicDefinitionMetaDatasAsync(ConcretType? typeFilter, string? displayNameRegex, bool onlyEnabled, GrainCancellationToken token)
        {
            IEnumerable<DynamicDefinitionMetaData> metaDataFiltered = this.State!.DefinitionMetaDatas;

            if (typeFilter is not null && typeFilter.ToType() != typeof(IDefinition))
                metaDataFiltered = metaDataFiltered.Where(m => m.MainDefintionType.Equals(typeFilter) || m.RelatedTypes.Any(r => r.Equals(typeFilter)));

            if (!string.IsNullOrEmpty(displayNameRegex))
            {
                var regexFilter = new Regex(displayNameRegex);
                metaDataFiltered = metaDataFiltered.Where(m => regexFilter.IsMatch(m.DisplayName));
            }

            if (onlyEnabled)
                metaDataFiltered = metaDataFiltered.Where(m => m.IsEnabled);

            token.CancellationToken.ThrowIfCancellationRequested();

            // Keep to array instead of toReadonly because ToReadOnly keep the original type if no convertion is needed by performance choice.
            // To prevent sending no serializable collection if no filter is needed we use ToArray
            return Task.FromResult(new EtagContainer<IReadOnlyCollection<DynamicDefinitionMetaData>>(this.State!.Etag, metaDataFiltered.ToArray()));
        }

        /// <inheritdoc />
        public async Task<bool> PushDefinitionAsync<TDefinition>(TDefinition definition, bool @override, IIdentityCard identity, GrainCancellationToken token)
            where TDefinition : class, IDefinition
        {
            // TODO : Check identity
            if (definition is null)
                return false;

            var repo = GetSafeRepository(definition, out var mainType);

            var repository = repo.First().Value;

            return await PushDefinitionImplAsync<TDefinition>(definition, @override, identity, token, repository);
        }

        /// <inheritdoc />
        public async Task<Guid> PushDefinitionAsync<TDefinition>(ConditionExpressionDefinition existFilter, TDefinition definition, IIdentityCard identity, GrainCancellationToken token)
            where TDefinition : class, IDefinition
        {
            // TODO : Check identity
            ArgumentNullException.ThrowIfNull(definition);

            var repo = GetSafeRepository(definition, out var mainType);

            var repository = repo.First().Value;

            var filter = existFilter.ToExpression<TDefinition, bool>().Compile();

            var exist = await repository.GetFirstValueAsync(d => filter(d.GetContainDefinition<TDefinition>()), token.CancellationToken);
            if (exist is not null)
                return exist.Uid;

            await PushDefinitionImplAsync<TDefinition>(definition, true, identity, token, repository);
            return definition.Uid;
        }

        /// <inheritdoc />
        public async Task<bool> ChangeStatus(Guid definitionUid, bool isEnabled, IIdentityCard identity, GrainCancellationToken token)
        {
            // TODO : Check identity

            token.CancellationToken.ThrowIfCancellationRequested();
            var result = this.State!.ChangeDefinitionStatus(definitionUid, isEnabled, this._timeManager, identity);

            if (result)
            {
                await PushStateAsync(token.CancellationToken);

                await this._signalService.Fire(DemocriteSystemDefinitions.Signals.DynamicDefinitionChanged,
                                               new CollectionChangeSignalMessage<Guid>(isEnabled ? CollectionChangeTypeEnum.Added : CollectionChangeTypeEnum.Removed, definitionUid.AsEnumerable().ToArray()),
                                               token.CancellationToken,
                                               this);

            }

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> RemoveDefinitionAsync(IIdentityCard identity, GrainCancellationToken token, IReadOnlyCollection<Guid> definitionIds)
        {
            var tasks = await ApplyActionOn(definitionIds, (r, ids, t) => r.DeleteRecordsAsync(t, ids), token.CancellationToken);

            var deleted = tasks.Where(t => t.IsCompletedSuccessfully)
                               .Select(t => t.Result)
                               .Any();

            if (deleted)
            {
                this.State!.RemoveDefinitionAsync(definitionIds);
                await PushStateAsync(token.CancellationToken);

                await this._signalService.Fire(DemocriteSystemDefinitions.Signals.DynamicDefinitionChanged,
                                               new CollectionChangeSignalMessage<Guid>(CollectionChangeTypeEnum.Removed, definitionIds),
                                               token.CancellationToken,
                                               this);

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public async Task<EtagContainer<IReadOnlyCollection<TDefinition>>> GetDefinitionAsync<TDefinition>(ConditionExpressionDefinition filter, GrainCancellationToken token) where TDefinition : IDefinition
        {
            var filterExpression = filter.ToExpression<TDefinition, bool>().Compile();

            var fetchTasks = await ApplyActionOn(this.State!.MetaDataIds, (r, ids, t) => r.GetValuesAsync(d => d.IsDefinition<TDefinition>() && filterExpression(d.GetContainDefinition<TDefinition>()), t).AsTask(), token.CancellationToken);

            var results = fetchTasks.Where(t => t.IsCompletedSuccessfully)
                                    .SelectMany(t => t.Result ?? EnumerableHelper<EtagDefinitionContainer>.ReadOnly)
                                    .Select(container => container.GetContainDefinition<TDefinition>())
                                    .Distinct()
                                    .ToArray();

            return new EtagContainer<IReadOnlyCollection<TDefinition>>(this.State!.Etag, results);
        }

        #region Tools

        /// <summary>
        /// Apply any action on items
        /// </summary>
        private async Task<IReadOnlyCollection<Task<TResult>>> ApplyActionOn<TResult>(IReadOnlyCollection<Guid> definitionIds,
                                                                                     Func<IRepository<EtagDefinitionContainer, Guid>, IReadOnlyCollection<Guid>, CancellationToken, Task<TResult>> func,
                                                                                     CancellationToken token)
        {
            var definitionMetaData = this.State!.GetMetaDataByIds(definitionIds, false);

            token.ThrowIfCancellationRequested();

            // Used to target the correct repository
            var definitionByMainType = definitionMetaData.GroupBy(d => d.MainDefintionType)
                                                         .ToDictionary(k => k.Key, v => v.ToDictionary(k => k.Uid));

            var repositories = GetRepositoryFromMainType(definitionByMainType.Keys.ToArray());

            var actionTasks = definitionByMainType.Select(kv =>
            {
                IRepository<EtagDefinitionContainer, Guid>? repository = null;
                if (!repositories.TryGetValue(kv.Key, out repository))
                    throw new InvalidOperationException("Missing Repository for " + kv.Key);

                return func(repository, kv.Value.Keys, token);
            }).ToArray();

            await actionTasks.SafeWhenAllAsync();

            return actionTasks;
        }

        /// <summary>
        /// Gets the type of the repository from <paramref name="mainType"/>
        /// </summary>
        private IReadOnlyDictionary<ConcretType, IRepository<EtagDefinitionContainer, Guid>> GetRepositoryFromMainType(params ConcretType[] mainTypes)
        {
            if (mainTypes.Length == 0)
                return DictionaryHelper<ConcretType, IRepository<EtagDefinitionContainer, Guid>>.ReadOnly;

            var result = new Dictionary<ConcretType, IRepository<EtagDefinitionContainer, Guid>>(mainTypes.Length);
            List<ConcretType>? missing = null;

            this._repositoriyCacheLock.EnterReadLock();
            try
            {
                foreach (var type in mainTypes)
                {
                    if (this._repositoryCache.TryGetValue(type, out var repo))
                    {
                        result.Add(type, repo);
                        continue;
                    }

                    missing ??= new List<ConcretType>(mainTypes.Length);
                    missing.Add(type);
                }
            }
            finally
            {
                this._repositoriyCacheLock.ExitReadLock();
            }

            if (missing is not null && missing.Any())
            {
                this._repositoriyCacheLock.EnterWriteLock();
                try
                {
                    foreach (var type in missing)
                    {
                        var repo = this._repositoryFactory.Get<IRepository<EtagDefinitionContainer, Guid>, EtagDefinitionContainer>(type.DisplayName.Replace(".", "").Trim().Replace(" ", "").Replace("-", "").Replace("_", ""),
                                                                                                                                    DemocriteConstants.DefaultDemocriteRepositoryConfigurationKey);

                        result.Add(type, repo);
                        if (!this._repositoryCache.ContainsKey(type))
                        {
                            this._repositoryCache.Add(type, repo);
                            continue;
                        }
                    }
                }
                finally
                {
                    this._repositoriyCacheLock.ExitWriteLock();
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override void DisposeResourcesEnd()
        {
            this._repositoriyCacheLock.Dispose();
            base.DisposeResourcesEnd();
        }

        /// <summary>
        /// Gets the safe repository from definition
        /// </summary>
        private IReadOnlyDictionary<ConcretType, IRepository<EtagDefinitionContainer, Guid>> GetSafeRepository<TDefinition>(TDefinition definition, out ConcretType mainType)
            where TDefinition : class, IDefinition
        {
            mainType = (ConcretType)definition.GetType().GetAbstractType();

            var repo = GetRepositoryFromMainType(mainType);

            if (!repo.Any())
            {
                throw new InvalidOperationException("Repository not founded");
            }

            return repo;
        }

        /// <summary>
        /// Pushes the definition.
        /// </summary>
        private async Task<bool> PushDefinitionImplAsync<TDefinition>(TDefinition definition,
                                                                      bool @override,
                                                                      IIdentityCard identity,
                                                                      GrainCancellationToken token,
                                                                      IRepository<EtagDefinitionContainer, Guid> repository) 
            where TDefinition : class, IDefinition
        {
            if (!@override)
            {
                var existing = await repository.GetByIdValueAsync(definition.Uid, token.CancellationToken);

                if (existing is not null)
                    return false;
            }

            var pushed = await repository.PushDataRecordAsync(EtagDefinitionContainer.Create(definition), true, token.CancellationToken);

            if (pushed)
            {
                this.State!.PushDefinition(definition, @override, this._timeManager, identity, out var metaDataUpdated);

                if (metaDataUpdated)
                {
                    await PushStateAsync(default);

                    await this._signalService.Fire(DemocriteSystemDefinitions.Signals.DynamicDefinitionChanged,
                                                   new CollectionChangeSignalMessage<Guid>(CollectionChangeTypeEnum.Added, definition.Uid.AsEnumerable().ToArray()),
                                                   token.CancellationToken,
                                                   this);
                }
            }

            return pushed;
        }

        #endregion

        #endregion

    }
}
