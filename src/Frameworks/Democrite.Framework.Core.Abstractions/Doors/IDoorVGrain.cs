// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Doors
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Door virtual grain use to manage signal conditiona and can fire a custom signal.
    /// </summary>
    /// <seealso cref="IGrainWithGuidKey" />
    public interface IDoorVGrain : IVGrain, IGrainWithGuidKey, ISignalReceiver
    {
        /// <summary>
        /// Initializes the door.
        /// </summary>
        Task UpdateAsync(DoorDefinition doorDefinition, GrainCancellationToken token);
    }
}
