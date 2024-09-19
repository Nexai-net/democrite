// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Bag.DebugTools
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes.MetaData;

    using Orleans.Concurrency;

    /// <summary>
    /// VGrain used to write on the logger the information about an input and execution context
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainMetaData("44F7E66A-CC76-4E60-91C4-15FB63CBF192", 
                    "display",
                    namespaceIdentifier: DebugToolConstants.BAG_NAMESPACE,
                    displayName: "Display", 
                    description: "Used to show in logs the input and/or context information.", 
                    categoryPath: "tools")]
    public interface IDisplayInfoVGrain : IVGrain
    {
        /// <summary>
        /// Displays context information on the logger.
        /// </summary>
        [ReadOnly]
        [OneWay]
        [VGrainMetaDataMethod("execution-context")]
        Task DisplayCallInfoAsync(IExecutionContext ctx);

        /// <summary>
        /// Displays the input and context information on the logger.
        /// </summary>
        [ReadOnly]
        [VGrainMetaDataMethod("input-and-execution-context")]
        Task<TInput> DisplayCallInfoAsync<TInput>(TInput input, IExecutionContext ctx);

        /// <summary>
        /// Displays the input and context information on the logger.
        /// </summary>
        [ReadOnly]
        [VGrainMetaDataMethod("input-and-execution-context-with-config")]
        Task<TInput> DisplayCallInfoAsync<TInput, TConfig>(TInput input, IExecutionContext<TConfig> ctx);
    }
}