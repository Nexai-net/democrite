﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;

    /// <summary>
    /// Builder used to create <see cref="SignalDefinition"/> simple signal
    /// </summary>
    public static class Signal
    {
        /// <summary>
        /// Start building a new signal
        /// </summary>
        public static ISignalBuilder StartBuilding(string signalName, Guid? uid = null)
        {
            return new SignalBuilder(signalName, uid);
        }

        /// <summary>
        /// Creates the new signal.
        /// </summary>
        public static SignalDefinition Create(string signalName, Guid? uid = null)
        {
            return new SignalBuilder(signalName, uid).Build();
        }

        /// <summary>
        /// Creates the new signal.
        /// </summary>
        public static SignalDefinition Create(in SignalId signalId)
        {
            return new SignalBuilder(signalId.Name, signalId.Uid).Build();
        }
    }
}
