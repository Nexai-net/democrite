// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Triggers;
    using Democrite.Framework.Node.Services;

    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class of every GrainService dedicated to triggers 
    /// </summary>
    /// <typeparam name="TTriggerHandlerVGrain">The type of the trigger handler v grain.</typeparam>
    /// <seealso cref="Orleans.Runtime.GrainService" />
    public abstract class TriggerBaseGrainService<TTriggerHandlerVGrain> : DemocriteVGrainService
        where TTriggerHandlerVGrain : ITriggerHandlerVGrain
    {
        #region Fields

        private readonly ITriggerDefinitionProvider _triggerDefinitionProvider;
        private readonly SemaphoreSlim _locker;
        private readonly TriggerTypeEnum _triggerType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CronVGrainService"/> class.
        /// </summary>
        protected TriggerBaseGrainService(GrainId id,
                                          Silo silo,
                                          ILoggerFactory loggerFactory,
                                          IGrainOrleanFactory grainFactory,
                                          ITriggerDefinitionProvider triggerDefinitionProvider,
                                          TriggerTypeEnum triggerType)
            : base(id, silo, loggerFactory)
        {
            this._triggerDefinitionProvider = triggerDefinitionProvider;

            this._locker = new SemaphoreSlim(1);
            this.GrainFactory = grainFactory;

            this._triggerType = triggerType;

            this._triggerDefinitionProvider.DataChanged -= TriggerDefinitionProvider_DataChanged;
            this._triggerDefinitionProvider.DataChanged += TriggerDefinitionProvider_DataChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the grain factory.
        /// </summary>
        public IGrainFactory GrainFactory { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task RefreshInfoAsync()
        {
            await this._locker.WaitAsync();
            try
            {
                await LaunchTriggerHandlersAsync();
            }
            finally
            {
                this._locker.Release();
            }
        }

        #region Tools

        /// <summary>
        /// Raised when the <see cref="ITriggerDefinitionProvider"/> notify a change in the definitions.
        /// </summary>
        private async void TriggerDefinitionProvider_DataChanged(object? sender, IReadOnlyCollection<Guid> definitionThatChanged)
        {
            await this._locker.WaitAsync();
            try
            {
                await LaunchTriggerHandlersAsync(definitionThatChanged);
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Activate the trigger handlers <see cref="ICronTriggerHandlerVGrain"/> if missing on the cluster.
        /// </summary>
        protected virtual async Task LaunchTriggerHandlersAsync(IReadOnlyCollection<Guid>? definitionThatChanged = null)
        {
            try
            {
                using (var timeoutToken = CancellationHelper.DisposableTimeout(TimeSpan.FromMinutes(1)))
                using (var grainCancelToken = timeoutToken.Content.ToGrainCancellationTokenSource())
                {
                    var token = timeoutToken.Content;

                    IEnumerable<TriggerDefinition> triggers = await this._triggerDefinitionProvider.GetValuesAsync(t => t.TriggerType == this._triggerType, token);

                    if (definitionThatChanged is not null && definitionThatChanged.Any())
                        triggers = triggers.Where(t => definitionThatChanged.Contains(t.Uid));

                    var indexedDefinitions = triggers.GroupBy(k => k.Uid)
                                                     .ToDictionary(k => k.Key, v => v.First());

                    var grainFromTriggers = await GetGrainFromTriggersAsync(indexedDefinitions.Keys.Concat(definitionThatChanged ?? EnumerableHelper<Guid>.ReadOnly).Distinct().ToArray(), token);

                    var tasks = new List<Task>();

                    foreach (var tsk in grainFromTriggers)
                    {
                        var id = tsk.GetPrimaryKey();

                        tasks.Add(tsk.UpdateAsync(grainCancelToken.Token).ContinueWith(t =>
                        {
                            token.ThrowIfCancellationRequested();

                            var localDefId = id;
                            if (t is not null && t.Exception is not null && indexedDefinitions.TryGetValue(id, out var trigger))
                            {
                                this.Logger.OptiLog(LogLevel.Error, "[Trigger: {definition}] {exception}", trigger.ToDebugDisplayName(), t.Exception);
                            }
                        }));
                    }

                    if (tasks.Any())
                        await tasks.SafeWhenAllAsync(token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aggr && aggr.InnerExceptions.All(e => e is OperationCanceledException))
                    return;

                this.Logger.OptiLog(LogLevel.Error, "Trigger Service Initialization Failed : {exception}", ex);
            }
        }

        /// <summary>
        /// Gets the grain from triggers.
        /// </summary>
        protected virtual ValueTask<IEnumerable<TTriggerHandlerVGrain>> GetGrainFromTriggersAsync(IReadOnlyCollection<Guid> triggerUids, CancellationToken token)
        {
            var traits = typeof(TTriggerHandlerVGrain);
            var grains = triggerUids.Select(c => this.GrainFactory.GetGrain(traits, c, "").AsReference<TTriggerHandlerVGrain>());
            return ValueTask.FromResult(grains);
        }

        #endregion

        #endregion
    }
}
