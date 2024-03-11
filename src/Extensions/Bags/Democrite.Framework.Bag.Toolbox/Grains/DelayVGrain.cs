// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.Toolbox.Grains
{
    using Democrite.Framework.Bag.Toolbox.Abstractions;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Elvex.Toolbox.Abstractions.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Placement;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Grain dedicated to delay the process <see cref="Task.Delay(TimeSpan)"/>
    /// </summary>
    /// <seealso cref="VGrainBase{IDelayVGrain}" />
    /// <seealso cref="IDelayVGrain" />
    [StatelessWorker]
    [PreferLocalPlacement]
    internal sealed class DelayVGrain : VGrainBase<IDelayVGrain>, IDelayVGrain
    {
        #region Fields
        
        private readonly Random _random;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayVGrain"/> class.
        /// </summary>
        public DelayVGrain(ILogger<IDelayVGrain> logger) 
            : base(logger)
        {
            this._random = new Random();
        }

        #endregion

        /// <inheritdoc />
        public async Task<TInput> DelayPassInputAsync<TInput>(TInput input, IExecutionContext<TimeSpan> executionContext)
        {
            await DelayAsync(executionContext);
            return input;
        }

        /// <inheritdoc />
        public async Task DelayAsync(IExecutionContext<TimeSpan> executionContext)
        {
            var time = executionContext.Configuration;

            if (time.TotalSeconds < 0)
                return;

            await Task.Delay(time, executionContext.CancellationToken);
        }

        /// <inheritdoc />
        public async Task<TInput> RandomDelayPassInputAsync<TInput>(TInput input, IExecutionContext<TimeSpanRange> executionContext)
        {
            await RandomDelayAsync(executionContext);
            return input;
        }

        /// <inheritdoc />
        public async Task RandomDelayAsync(IExecutionContext<TimeSpanRange> executionContext)
        {
            var time = executionContext.Configuration.GetRandomValue(this._random);

            if (time.TotalSeconds < 0)
                return;

            await Task.Delay(time, executionContext.CancellationToken);
        }
    }
}
