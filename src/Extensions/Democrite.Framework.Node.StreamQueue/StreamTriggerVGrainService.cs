// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Services;

    /// <summary>
    /// Grain service in charge to handled trigger link to a stream
    /// </summary>
    /// <seealso cref="IGrainService" />
    public sealed class StreamTriggerVGrainService : TriggerBaseGrainService<IStreamTriggerHandlerVGrain>, IStreamTriggerVGrainService
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTriggerVGrainService"/> class.
        /// </summary>
        public StreamTriggerVGrainService(GrainId id,
                                          Silo silo,
                                          ILoggerFactory loggerFactory,
                                          IGrainOrleanFactory grainFactory,
                                          ITriggerDefinitionProvider triggerDefinitionProvider)
            : base(id, silo, loggerFactory, grainFactory, triggerDefinitionProvider, TriggerTypeEnum.Stream)
        {
        }

        #endregion
    }
}
