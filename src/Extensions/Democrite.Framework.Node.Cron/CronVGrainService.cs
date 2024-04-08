// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Cron
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    /// <summary>
    /// Virtual Grain Service instanciate and launch on all silo start used to start <see cref="ICronTriggerHandlerVGrain"/> by cron setups
    /// </summary>
    [DemocriteSystemVGrain]
    internal sealed class CronVGrainService : TriggerBaseGrainService<ICronTriggerHandlerVGrain>, ICronVGrainService
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CronVGrainService"/> class.
        /// </summary>
        public CronVGrainService(GrainId id,
                                 Silo silo,
                                 ILoggerFactory loggerFactory,
                                 IGrainOrleanFactory grainFactory,
                                 ITriggerDefinitionProvider triggerDefinitionProvider) 
            : base(id, silo, loggerFactory, grainFactory, triggerDefinitionProvider, TriggerTypeEnum.Cron)
        {
        }

        #endregion
    }
}
