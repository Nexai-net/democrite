// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Node.Signals.Models;

    using System.Collections.Generic;

    /// <summary>
    /// Surrogate struct to help storing <see cref="SignalHandlerState"/>
    /// </summary>
    [Serializable]
    [GenerateSerializer]
    internal struct SignalHandlerStateSurrogate
    {
        /// <summary>
        /// Gets or sets the subscriptions.
        /// </summary>
        [Id(0)]
        public IEnumerable<SignalSubscription> Subscriptions { get; set; }
    }

    /// <summary>
    /// Declare a converter to convert both way <see cref="SignalHandlerState"/> to <see cref="SignalHandlerStateSurrogate"/>
    /// </summary>
    /// <seealso cref="IConverter{SignalHandlerState, SignalHandlerStateSurrogate}" />
    [RegisterConverter]
    internal sealed class SignalHandlerStateSurrogateConverter : IConverter<SignalHandlerState, SignalHandlerStateSurrogate>
    {
        /// <inheritdoc />
        public SignalHandlerState ConvertFromSurrogate(in SignalHandlerStateSurrogate surrogate)
        {
            return new SignalHandlerState(surrogate.Subscriptions);
        }

        /// <inheritdoc />
        public SignalHandlerStateSurrogate ConvertToSurrogate(in SignalHandlerState value)
        {
            return new SignalHandlerStateSurrogate()
            {
                Subscriptions = value.Subscriptions.ToArray()
            };
        }
    }
}
