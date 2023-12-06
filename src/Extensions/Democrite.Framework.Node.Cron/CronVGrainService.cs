// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Cron
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// Virtual Grain Service instanciate and launch on all silo start used to start <see cref="ICronTriggerHandlerVGrain"/> by cron setups
    /// </summary>
    [DemocriteSystemVGrain]
    internal sealed class CronVGrainService : GrainService, ICronVGrainService
    {
        #region Fields

        private readonly ITriggerDefinitionProvider _triggerDefinitionProvider;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CronVGrainService"/> class.
        /// </summary>
        public CronVGrainService(GrainId id,
                                 Silo silo,
                                 ILoggerFactory loggerFactory,
                                 IGrainFactory grainFactory,
                                 ITriggerDefinitionProvider triggerDefinitionProvider)
            : base(id, silo, loggerFactory)
        {
            this._triggerDefinitionProvider = triggerDefinitionProvider;
            this._grainFactory = grainFactory;

            this._triggerDefinitionProvider.DataChanged -= TriggerDefinitionProvider_DataChanged;
            this._triggerDefinitionProvider.DataChanged += TriggerDefinitionProvider_DataChanged;
        }

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
        private async void TriggerDefinitionProvider_DataChanged(object? sender, EventArgs e)
        {
            await LaunchTriggerHandlersAsync();
        }

        /// <summary>
        /// Activate the trigger handlers <see cref="ICronTriggerHandlerVGrain"/> if missing on the cluster.
        /// </summary>
        private async Task LaunchTriggerHandlersAsync()
        {
            var crons = await this._triggerDefinitionProvider.GetValuesAsync(t => t.TriggerType == TriggerTypeEnum.Cron);

            var cronTriggerVGrainTasks = crons.Select(c => this._grainFactory.GetGrain<ICronTriggerHandlerVGrain>(c.Uid))
                                              .Select(grain => grain.UpdateAsync())
                                              .ToArray();

            if (cronTriggerVGrainTasks.Any())
                await Task.WhenAll(cronTriggerVGrainTasks);
        }

        #endregion

        #endregion
    }
}
