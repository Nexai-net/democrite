// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.Toolbox.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Toolbox.Abstractions.Models;

    using System;

    /// <summary>
    /// Simple grain used to create a Task.Delay in the process
    /// </summary>
    /// <seealso cref="IVGrain" />
    public interface IDelayVGrain : IVGrain
    {
        /// <summary>
        /// Delay of a specific time in <see cref="TimeSpan"/> pass in configuration
        /// </summary>
        Task<TInput> DelayPassInputAsync<TInput>(TInput input, IExecutionContext<TimeSpan> executionContext);

        /// <summary>
        /// Delay of a specific time in <see cref="TimeSpan"/> pass in configuration
        /// </summary>
        Task DelayAsync(IExecutionContext<TimeSpan> executionContext);

        /// <summary>
        /// Delay of a value in <see cref="TimeSpanRange"/> pass in configuration
        /// </summary>
        Task<TInput> RandomDelayPassInputAsync<TInput>(TInput input, IExecutionContext<TimeSpanRange> executionContext);

        /// <summary>
        /// Delay of a value in <see cref="TimeSpanRange"/> pass in configuration
        /// </summary>
        Task RandomDelayAsync(IExecutionContext<TimeSpanRange> executionContext);

    }
}
