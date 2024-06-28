// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    using Democrite.Framework.Core.Abstractions.Attributes;
    using Democrite.Framework.Core.Abstractions.Models;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Define the virtual grain in charge of a specific signal
    /// </summary>
    /// <seealso cref="IGrainWithGuidKey" />
    [DemocriteSystemVGrain]
    internal interface ISignalVGrain : IGrainWithGuidKey, ISignalHandler
    {
        /// <summary>
        /// Fires the signal configured
        /// </summary>
        [OneWay]
        [ReadOnly]
        Task<Guid> Fire(Guid fireId, GrainId? sourceId, VGrainMetaData? sourceMetaData, GrainCancellationToken token);

        /// <summary>
        /// Fires the signal configured
        /// </summary>
        [OneWay]
        [ReadOnly]
        Task<Guid> Fire<TData>(Guid fireId, GrainId? sourceId, TData data, VGrainMetaData? sourceMetaData, GrainCancellationToken token)
            where TData : struct;
    }
}
