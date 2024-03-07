// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue
{
    using Democrite.Framework.Node.Abstractions.Triggers;

    /// <summary>
    /// Handler of trigger based on stream information
    /// </summary>
    /// <seealso cref="ITriggerHandlerVGrain" />
    /// <seealso cref="IGrainWithGuidCompoundKey" />
    public interface IStreamTriggerHandlerVGrain : ITriggerHandlerVGrain, IGrainWithGuidCompoundKey
    {
    }
}
