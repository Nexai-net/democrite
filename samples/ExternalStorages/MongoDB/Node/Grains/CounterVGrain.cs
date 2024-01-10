// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Node.Grains
{
    using Common;

    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    public class CounterState
    {
        public int Count { get; set; }
    }

    internal sealed class CounterVGrain : VGrainBase<CounterState, ICounterVGrain>, ICounterVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CounterVGrain"/> class.
        /// </summary>
        public CounterVGrain(ILogger<ICounterVGrain> logger,
                             [PersistentState("Counter")] IPersistentState<CounterState> persistentState) 
            : base(logger, persistentState)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<int> GetValueAsync(string counterName, IExecutionContext executionContext)
        {
            return Task.FromResult(this.State!.Count);
        }

        /// <inheritdoc />
        public async Task<(string, int)> Increase(string counterName, IExecutionContext executionContext)
        {
            this.State!.Count++;

            await PushStateAsync(default);

            return (GetGrainId().ToString(), this.State.Count);
        }

        #endregion
    }
}
