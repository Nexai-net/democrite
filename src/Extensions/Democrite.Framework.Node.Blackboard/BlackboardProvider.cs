// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard
{
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Exceptions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;
    using Democrite.Framework.Node.Blackboard.VGrains;

    using Elvex.Toolbox.Collections;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.DependencyInjection;

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Get or provide a blackboad based on specific DeferredId or couple Name + Template Name
    /// </summary>
    /// <seealso cref="IBlackboardProvider" />
    internal sealed class BlackboardProvider : IBlackboardProvider
    {
        #region Fields

        private static readonly TimeSpan s_blackboardBuildAndRetreiveTimeout;

        private static readonly Dictionary<Guid, BlackboardHandlerProxy> s_blackboardProxies;
        private static readonly MultiKeyDictionary<string, BlackboardHandlerProxy> s_blackboardProxiesByName;

        private static readonly SemaphoreSlim s_blackboardProxyLocker;

        private readonly IBlackboardTemplateDefinitionProvider _blackboardTemplateDefinitionProvider;
        private readonly IBlackboardRegistryGrain _blackboardRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="BlackboardProvider"/> class.
        /// </summary>
        static BlackboardProvider()
        {
            s_blackboardProxiesByName = new MultiKeyDictionary<string, BlackboardHandlerProxy>();
            s_blackboardProxies = new Dictionary<Guid, BlackboardHandlerProxy>();
            s_blackboardProxyLocker = new SemaphoreSlim(1);

            s_blackboardBuildAndRetreiveTimeout = Debugger.IsAttached ? TimeSpan.FromSeconds(45) : TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardProvider"/> class.
        /// </summary>
        public BlackboardProvider(IBlackboardTemplateDefinitionProvider blackboardTemplateDefinitionProvider,
                                  IServiceProvider serviceProvider,
                                  IGrainFactory grainFactory)
        {
            this._blackboardTemplateDefinitionProvider = blackboardTemplateDefinitionProvider;
            this._serviceProvider = serviceProvider;
            this._grainFactory = grainFactory;
            this._blackboardRegistry = this._grainFactory.GetGrain<IBlackboardRegistryGrain>(IBlackboardRegistryGrain.GrainId);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<IBlackboardRef> GetBlackboardAsync(string boardName,
                                                                  string boardTemplateConfigurationKey,
                                                                  CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(boardName);
            ArgumentNullException.ThrowIfNull(boardTemplateConfigurationKey);

            var tmpl = await this._blackboardTemplateDefinitionProvider.GetValuesAsync(d => d.UniqueTemplateName == boardTemplateConfigurationKey, token);

#pragma warning disable IDE0270 // Use coalesce expression
            if (tmpl is null || !tmpl.Any())
                throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), boardTemplateConfigurationKey);
#pragma warning restore IDE0270 // Use coalesce expression

            if (tmpl.Count > 1)
            {
                throw new DefinitionException(typeof(BlackboardTemplateDefinition),
                                              boardTemplateConfigurationKey,
                                              "Multiple definition are not allowed",
                                              DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Definition,
                                                                        DemocriteErrorCodes.PartType.Identifier,
                                                                        DemocriteErrorCodes.ErrorType.Multiple));
            }

            return await GetBlackboardByNameAsync(boardName, tmpl.First(), token);
        }

        /// <inheritdoc />
        public ValueTask<IBlackboardRef> GetBlackboardAsync(Guid uid, CancellationToken token = default)
        {
            // Method used to put the "ByUid" and "ByName" code side and slit public and private methods
            return GetBlackboardByUidAsync(uid, token);
        }

        /// <inheritdoc />
        public async ValueTask<IBlackboardRef> GetBlackboardAsync(string boardName,
                                                                  Guid boardTemplateConfigurationUid,
                                                                  CancellationToken token = default)
        {
            var tmpl = await this._blackboardTemplateDefinitionProvider.GetFirstValueByIdAsync(boardTemplateConfigurationUid, token);

#pragma warning disable IDE0270 // Use coalesce expression
            if (tmpl is null)
                throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), boardTemplateConfigurationUid.ToString());
#pragma warning restore IDE0270 // Use coalesce expression

            return await GetBlackboardByNameAsync(boardName, tmpl, token);
        }

        #region Tools

        /// <summary>
        /// Gets or create the <see cref="IBlackboardRef"/>
        /// </summary>
        private async ValueTask<IBlackboardRef> GetBlackboardByNameAsync(string boardName, BlackboardTemplateDefinition template, CancellationToken token)
        {
            using (var timeout = CancellationHelper.DisposableTimeout(s_blackboardBuildAndRetreiveTimeout))
            using (var grp = CancellationTokenSource.CreateLinkedTokenSource(timeout.Content, token))
            {
                IBlackboardRef? result = null;
                bool isNew = false;

                await s_blackboardProxyLocker.WaitAsync(grp.Token);
                try
                {
                    var keys = new string[2] { template.UniqueTemplateName, boardName };

                    if (s_blackboardProxiesByName.TryGetValue(keys, out var proxy))
                    {
                        result = proxy;
                    }
                    else
                    {
                        var boardId = await this._blackboardRegistry.GetOrCreateAsync(boardName, template.UniqueTemplateName);

                        var newProxy = ActivatorUtilities.CreateInstance<BlackboardHandlerProxy>(this._serviceProvider, boardId);
                        await newProxy.InitializeAsync(token);
                        s_blackboardProxies[newProxy.Uid] = newProxy;

                        s_blackboardProxiesByName[keys] = newProxy;

                        isNew = true;
                        result = newProxy;
                    }
                }
                finally
                {
                    s_blackboardProxyLocker.Release();
                }

                if (isNew)
                    await CheckBlackboardInitializedAsync(result, token);

                return result;
            }
        }

        /// <inheritdoc />
        private async ValueTask<IBlackboardRef> GetBlackboardByUidAsync(Guid uid, CancellationToken token = default)
        {
            using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(5)))
            using (var grp = CancellationTokenSource.CreateLinkedTokenSource(timeout.Content, token))
            {
                IBlackboardRef? result = null;
                bool isNew = false;

                await s_blackboardProxyLocker.WaitAsync(grp.Token);
                try
                {
                    if (s_blackboardProxies.TryGetValue(uid, out var proxy))
                    {
                        result = proxy;
                    }
                    else
                    {
                        var boardId = await this._blackboardRegistry.TryGetAsync(uid) ?? throw new BlackboardMissingDemocriteException(uid, null, null);

                        var newProxy = ActivatorUtilities.CreateInstance<BlackboardHandlerProxy>(this._serviceProvider, boardId);
                        await newProxy.InitializeAsync(token);

                        s_blackboardProxies.Add(uid, newProxy);

                        var keys = new string[2] { newProxy.TemplateName, newProxy.Name };
                        s_blackboardProxiesByName[keys] = newProxy;

                        isNew = true;
                        result = newProxy;
                    }
                }
                finally
                {
                    s_blackboardProxyLocker.Release();
                }

                if (isNew)
                    await CheckBlackboardInitializedAsync(result, token);

                return result;
            }
        }

        /// <summary>
        /// Checks the blackboard initialized asynchronous.
        /// </summary>
        private async Task CheckBlackboardInitializedAsync(IBlackboardRef result, CancellationToken token)
        {
            var status = await result.GetStatusAsync(token);
            if (status == BlackboardLifeStatusEnum.None)
                await result.InitializeAsync(token);
        }

        #endregion

        #endregion
    }
}
