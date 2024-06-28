// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;
    using Democrite.Framework.Node.Blackboard.Models;
    using Democrite.Framework.Node.Blackboard.Models.Surrogates;

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Collections;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Orleans.Placement;
    using Orleans.Runtime;

    using System;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    /// <summary>
    /// Registry used to stored all the blackboard created
    /// </summary>
    /// <seealso cref="Orleans.Grain" />
    [KeepAlive]
    [DemocriteSystemVGrain]
    internal sealed class BlackboardRegistryGrain : VGrainBase<BlackboardRegistryState, BlackboardRegistryStateSurrogate, BlackboardRegistryStateConverter, IBlackboardRegistryGrain>, IBlackboardRegistryGrain
    {
        #region Fields

        private readonly IBlackboardTemplateDefinitionProvider _blackboardTemplateDefinitionProvider;

        private readonly Dictionary<BlackboardId, GrainId> _grainIdCache;

        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardRegistryGrain"/> class.
        /// </summary>
        public BlackboardRegistryGrain(ILogger<IBlackboardRegistryGrain> logger,
                                       [PersistentState(BlackboardConstants.BlackboardRegistryStorageKey, BlackboardConstants.BlackboardRegistryStorageConfigurationKey)] IPersistentState<BlackboardRegistryStateSurrogate> persistentState,
                                       IGrainOrleanFactory grainFactory,
                                       IBlackboardTemplateDefinitionProvider blackboardTemplateDefinitionProvider)

            : base(logger, persistentState)
        {
            this._blackboardTemplateDefinitionProvider = blackboardTemplateDefinitionProvider;
            this._grainIdCache = new Dictionary<BlackboardId, GrainId>();

            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<Tuple<BlackboardId, GrainId>> GetOrCreateAsync(string boardName, string blackboardTemplateKey, GrainCancellationToken token, Guid? callContextId = null)
        {
            var boardId = this.State!.TryGet(boardName, blackboardTemplateKey);

            // GrainId is cached after BuildFromTemplateAsync is called
            if (boardId is not null && this._grainIdCache.TryGetValue(boardId.Value, out var grainIdCached))
            {
                return Tuple.Create(boardId.Value, grainIdCached);
            }

            boardId = this.State!.CreateNewBlackboardId(boardName, blackboardTemplateKey);
            var blackboardGrain = this._grainFactory.GetGrain<IBlackboardGrain>(boardId!.Value.Uid);
            var grainId = blackboardGrain.GetGrainId();

            this._grainIdCache.Add(boardId.Value, grainId);
            await PushStateAsync(default);

            await InitializaBlackboardGrainAsync(blackboardGrain, boardId.Value, blackboardTemplateKey, token, callContextId);
            return Tuple.Create(boardId.Value, grainId);
        }

        /// <inheritdoc />
        public Task<Tuple<BlackboardId, GrainId>?> TryGetAsync(string boardName, string blackboardTemplateKey)
        {
            Tuple<BlackboardId, GrainId>? result = null;
            var boardId = this.State!.TryGet(boardName, blackboardTemplateKey);

            // GrainId is cached after BuildFromTemplateAsync is called
            if (boardId is not null && this._grainIdCache.TryGetValue(boardId.Value, out var grainIdCached))
            {
                result = Tuple.Create(boardId.Value, grainIdCached);
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<Tuple<BlackboardId, GrainId>?> TryGetAsync(Guid uid)
        {
            Tuple<BlackboardId, GrainId>? result = null;
            var boardId = this.State!.TryGetAsync(uid);

            if (boardId is not null && this._grainIdCache.TryGetValue(boardId.Value, out var grainId))
                result = Tuple.Create(boardId.Value, grainId);

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public async Task Unregister(Guid uid)
        {
            this.State!.Unregister(uid);
            await PushStateAsync(default);
        }

        #region Tools

        /// <summary>
        /// Initializas the blackboard grain.
        /// </summary>
        private async Task InitializaBlackboardGrainAsync(IBlackboardGrain blackboardGrain,
                                                          BlackboardId blackboardId,
                                                          string blackboardTemplateKey,
                                                          GrainCancellationToken token,
                                                          Guid? callContextId = null)
        {
            var tmpl = await this._blackboardTemplateDefinitionProvider.GetFirstValueAsync(f => string.Equals(f.UniqueTemplateName, blackboardTemplateKey), token.CancellationToken);

#pragma warning disable IDE0270 // Use coalesce expression
            if (tmpl == null)
                throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), blackboardTemplateKey);
#pragma warning restore IDE0270 // Use coalesce expression

            await blackboardGrain.BuildFromTemplateAsync(tmpl.Uid, blackboardId, token, callContextId);
        }

        #endregion

        #endregion
    }
}
