// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Triggers;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

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
    public abstract class TriggerBaseGrainService<TTriggerHandlerVGrain> : GrainService
        where TTriggerHandlerVGrain : ITriggerHandlerVGrain
    {
        #region Fields

        private readonly ITriggerDefinitionProvider _triggerDefinitionProvider;

        private readonly TriggerTypeEnum _triggerType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CronVGrainService"/> class.
        /// </summary>
        protected TriggerBaseGrainService(GrainId id,
                                          Silo silo,
                                          ILoggerFactory loggerFactory,
                                          IGrainFactory grainFactory,
                                          ITriggerDefinitionProvider triggerDefinitionProvider,
                                          TriggerTypeEnum triggerType)
            : base(id, silo, loggerFactory)
        {
            this._triggerDefinitionProvider = triggerDefinitionProvider;
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
        public override async Task Start()
        {
            await base.Start();
            await LaunchTriggerHandlersAsync();
        }

        #region Tools

        /// <summary>
        /// Raised when the <see cref="ITriggerDefinitionProvider"/> notify a change in the definitions.
        /// </summary>
        private async void TriggerDefinitionProvider_DataChanged(object? sender, IReadOnlyCollection<Guid> definitionThatChanged)
        {
            await LaunchTriggerHandlersAsync();
        }

        /// <summary>
        /// Activate the trigger handlers <see cref="ICronTriggerHandlerVGrain"/> if missing on the cluster.
        /// </summary>
        protected virtual async Task LaunchTriggerHandlersAsync()
        {
            using (var timeoutToken = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(10)))
            {
                var triggers = await this._triggerDefinitionProvider.GetValuesAsync(t => t.TriggerType == this._triggerType, timeoutToken.Content);

                var grainFromTriggers = await GetGrainFromTriggersAsync(triggers, timeoutToken.Content);

                var triggerVGrainTasks = grainFromTriggers.Select(grain => grain.UpdateAsync())
                                                          .ToArray();

                if (triggerVGrainTasks.Any())
                    await triggerVGrainTasks.SafeWhenAllAsync(timeoutToken.Content);
            }
        }

        /// <summary>
        /// Gets the grain from triggers.
        /// </summary>
        protected virtual ValueTask<IEnumerable<TTriggerHandlerVGrain>> GetGrainFromTriggersAsync(IReadOnlyCollection<TriggerDefinition> triggers, CancellationToken token)
        {
            var traits = typeof(TTriggerHandlerVGrain);
            var grains = triggers.Select(c => this.GrainFactory.GetGrain(traits, c.Uid, "").AsReference<TTriggerHandlerVGrain>());
            return ValueTask.FromResult(grains);
        }

        #endregion

        #endregion
    }
}
