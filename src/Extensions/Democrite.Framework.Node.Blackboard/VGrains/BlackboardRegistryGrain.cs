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

    using Microsoft.Extensions.Logging;

    using Orleans.Placement;
    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Registry used to stored all the blackboard created
    /// </summary>
    /// <seealso cref="Orleans.Grain" />
    [KeepAlive]
    [PreferLocalPlacement]
    [DemocriteSystemVGrain]
    internal sealed class BlackboardRegistryGrain : VGrainBase<BlackboardRegistryState, BlackboardRegistryStateSurrogate, BlackboardRegistryStateConverter, IBlackboardRegistryGrain>, IBlackboardRegistryGrain
    {
        #region Fields

        private readonly IBlackboardTemplateDefinitionProvider _blackboardTemplateDefinitionProvider;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardRegistryGrain"/> class.
        /// </summary>
        public BlackboardRegistryGrain(ILogger<IBlackboardRegistryGrain> logger,
                                       [PersistentState(BlackboardConstants.BlackboardStorageStateKey, BlackboardConstants.BlackboardRegistryStorageConfigurationKey)] IPersistentState<BlackboardRegistryStateSurrogate> persistentState,
                                       IGrainOrleanFactory grainFactory,
                                       IBlackboardTemplateDefinitionProvider blackboardTemplateDefinitionProvider)
            : base(logger, persistentState)
        {
            this._grainFactory = grainFactory;
            this._blackboardTemplateDefinitionProvider = blackboardTemplateDefinitionProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<GrainId> GetOrCreateAsync(string boardName, string blackboardTemplateKey)
        {
            Guid? templateUid = null;

            var boardId = this.State!.TryGetAsync(boardName, blackboardTemplateKey);
            if (boardId is null)
            {
                var tmpl = await this._blackboardTemplateDefinitionProvider.GetFirstValueAsync(f => string.Equals(f.UniqueTemplateName, blackboardTemplateKey), default);

#pragma warning disable IDE0270 // Use coalesce expression
                if (tmpl == null)
                    throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), blackboardTemplateKey);
#pragma warning restore IDE0270 // Use coalesce expression

                templateUid = tmpl.Uid;
                boardId = this.State!.CreateNewBlackboardId(boardName, blackboardTemplateKey);
                await PushStateAsync(default);
            }

            var blackboardGrain = this._grainFactory.GetGrain<IBlackboardGrain>(boardId!.Value.Uid);
            var grainId = blackboardGrain.GetGrainId();

            // This ensure a blackboard is initialized before any usage
            if (templateUid is not null && templateUid.Value != Guid.Empty)
                await blackboardGrain.BuildFromTemplateAsync(templateUid.Value, boardId.Value);

            return grainId;
        }

        /// <inheritdoc />
        public Task<GrainId?> TryGetAsync(Guid uid)
        {
            var boardId = this.State!.TryGetAsync(uid);
            if (boardId is null)
                return Task.FromResult<GrainId?>(null);

            var addressable = this._grainFactory.GetGrain<IBlackboardGrain>(boardId.Value.Uid);
            var grainId = addressable.GetGrainId();
            return Task.FromResult<GrainId?>(grainId);
        }

        /// <inheritdoc />
        public async Task Unregister(Guid uid)
        {
            this.State!.Unregister(uid);
            await PushStateAsync(default);
        }

        #endregion
    }
}
