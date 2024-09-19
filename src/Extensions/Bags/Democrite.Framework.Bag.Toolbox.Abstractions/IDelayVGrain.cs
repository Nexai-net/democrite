// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.Toolbox.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes.MetaData;

    using Elvex.Toolbox.Abstractions.Models;

    using Orleans.Concurrency;

    using System;

    /// <summary>
    /// Simple grain used to create a Task.Delay in the process
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainMetaData("40157B9A-27A5-402C-9650-4EFDD921DCDC",
                    "delay",
                    namespaceIdentifier: "bag.toolbox",
                    displayName: "Delay", 
                    description: "Used to introduce delay in sequence.", 
                    categoryPath: "tools")]
    public interface IDelayVGrain : IVGrain
    {
        /// <summary>
        /// Delay of a specific time in <see cref="TimeSpan"/> pass in configuration
        /// </summary>
        [ReadOnly]
        Task<TInput> DelayPassInputAsync<TInput>(TInput input, IExecutionContext<TimeSpan> executionContext);

        /// <summary>
        /// Delay of a specific time in <see cref="TimeSpan"/> pass in configuration
        /// </summary>
        [ReadOnly]
        Task DelayAsync(IExecutionContext<TimeSpan> executionContext);

        /// <summary>
        /// Delay of a value in <see cref="TimeSpanRange"/> pass in configuration
        /// </summary>
        [ReadOnly]
        Task<TInput> RandomDelayPassInputAsync<TInput>(TInput input, IExecutionContext<TimeSpanRange> executionContext);

        /// <summary>
        /// Delay of a value in <see cref="TimeSpanRange"/> pass in configuration
        /// </summary>
        [ReadOnly]
        Task RandomDelayAsync(IExecutionContext<TimeSpanRange> executionContext);

    }
}
