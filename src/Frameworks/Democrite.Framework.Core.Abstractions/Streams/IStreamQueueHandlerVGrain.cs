// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.StreamQueue
{
    /// <summary>
    /// Define an handler used to keep data provider through the stream connected
    /// </summary>
    public interface IStreamQueueHandlerVGrain : IVGrain, IGrainWithGuidKey
    {
        /// <summary>
        /// Update stream status and ensure information correspond to definitions
        /// </summary>
        Task UpdateAsync();
    }
}