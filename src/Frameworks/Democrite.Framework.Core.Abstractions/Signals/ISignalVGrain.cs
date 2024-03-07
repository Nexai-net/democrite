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
    public interface ISignalVGrain : IGrainWithGuidKey, ISignalHandler
    {
        /// <summary>
        /// Fires the signal configured
        /// </summary>
        [OneWay]
        Task<Guid> Fire(GrainId? sourceId, VGrainMetaData? sourceMetaData, GrainCancellationToken token);

        /// <summary>
        /// Fires the signal configured
        /// </summary>
        [OneWay]
        Task<Guid> Fire<TData>(GrainId? sourceId, TData data, VGrainMetaData? sourceMetaData, GrainCancellationToken token) 
            where TData : struct;
    }
}
