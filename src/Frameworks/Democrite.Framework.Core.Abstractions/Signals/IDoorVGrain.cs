// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Signals
{
    /// <summary>
    /// Door virtual grain use to manage signal conditiona and can fire a custom signal.
    /// </summary>
    /// <seealso cref="Orleans.IGrainWithGuidKey" />
    public interface IDoorVGrain : IVGrain, IGrainWithGuidKey
    {
        /// <summary>
        /// Initializes the door.
        /// </summary>
        Task InitializeAsync(DoorDefinition doorDefinition);
    }
}
