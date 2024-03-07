// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.DebugTools
{
    using Democrite.Framework.Core.Abstractions;

    /// <summary>
    /// VGrain used to write on the logger the information about an input and execution context
    /// </summary>
    /// <seealso cref="IVGrain" />
    public interface IDisplayInfoVGrain : IVGrain
    {
        /// <summary>
        /// Displays context information on the logger.
        /// </summary>
        Task DisplayCallInfoAsync(IExecutionContext ctx);

        /// <summary>
        /// Displays the input and context information on the logger.
        /// </summary>
        Task<TInput> DisplayCallInfoAsync<TInput>(TInput input, IExecutionContext ctx);


        /// <summary>
        /// Displays the input and context information on the logger.
        /// </summary>
        Task<TInput> DisplayCallInfoAsync<TInput, TConfig>(TInput input, IExecutionContext<TConfig> ctx);
    }
}