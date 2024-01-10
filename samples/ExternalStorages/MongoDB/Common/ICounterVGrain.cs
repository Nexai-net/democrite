// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Common
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Enums;

    using Orleans.Runtime;

    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{input}")]
    public interface ICounterVGrain : IVGrain
    {
        /// <summary>
        /// Increases the counter.
        /// </summary>
        Task<(string, int)> Increase(string counterName, IExecutionContext execution);

        /// <summary>
        /// Gets the current value
        /// </summary>
        Task<int> GetValueAsync(string counterName, IExecutionContext executionContext);
    }
}