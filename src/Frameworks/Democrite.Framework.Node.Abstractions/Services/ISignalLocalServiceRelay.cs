// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Services
{
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Allow local service to catch <see cref="Signal"/> like a VGrain could
    /// </summary>
    public interface ISignalLocalServiceRelay
    {
        /// <summary>
        /// Subscribes the specified action to be called when <paramref name="signalId"/> is emit
        /// </summary>
        ValueTask<IDisposable> SubscribeAsync(Func<SignalMessage, ValueTask> action, SignalId signalId, Predicate<SignalMessage>? predicated = null);
    }
}
