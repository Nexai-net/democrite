// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue
{
    using Democrite.Framework.Core.Abstractions.Streams;
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Node.Abstractions.Services;
    using Democrite.Framework.Node.Abstractions.Triggers;
    using Democrite.Framework.Node.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;
    using Orleans.Services;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Grain service in charge to handled trigger link to a stream
    /// </summary>
    /// <seealso cref="IGrainService" />
    public sealed class StreamTriggerVGrainService : TriggerBaseGrainService<IStreamTriggerHandlerVGrain>, IStreamTriggerVGrainService
    {
        //#region Fields
        
        //private readonly IStreamQueueDefinitionProvider _streamQueueDefinitionProvider;
        
        //#endregion

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
            //this._streamQueueDefinitionProvider = streamQueueDefinitionProvider;
        }

        #endregion

        //#region Methods

        ///// <summary>
        ///// Gets the grain from triggers.
        ///// </summary>
        //protected override ValueTask<IEnumerable<IStreamTriggerHandlerVGrain>> GetGrainFromTriggersAsync(IReadOnlyCollection<TriggerDefinition> triggers)
        //{
        //    var grains = triggers.OfType<StreamTriggerDefinition>()
        //                         .SelectMany(c => Enumerable.Range(0, (int)Math.Max(1, c.MaxConcurrentProcess))
        //                                                    .Select(indx => this.GrainFactory.GetGrain<IStreamTriggerHandlerVGrain>(c.Uid, keyExtension: indx.ToString(), null)));
        //    return ValueTask.FromResult(grains);
        //}

        //#endregion
    }
}
