// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    /// <inheritdoc />
    /// <seealso cref="GrainService" />
    /// <seealso cref="ISignalTriggerVGrainService" />
    internal sealed class SignalTriggerVGrainService : TriggerBaseGrainService<ISignalTriggerVGrain>, ISignalTriggerVGrainService
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTriggerVGrainService"/> class.
        /// </summary>
        public SignalTriggerVGrainService(GrainId grainId,
                                          Silo silo,
                                          ILoggerFactory loggerFactory,
                                          ITriggerDefinitionProvider triggerDefinitionProvider,
                                          IGrainOrleanFactory grainFactory)
            : base(grainId, silo, loggerFactory, grainFactory, triggerDefinitionProvider, TriggerTypeEnum.Signal)
        {
        }

        #endregion
    }
}
