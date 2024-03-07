// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Doors;

    using System;

    /// <summary>
    /// Builder used to create <see cref="SignalDefinition"/>. Signal is a complex Signal
    /// </summary>
    public static class Door
    {
        /// <summary>
        /// Creates <see cref="DoorDefinitions"/>
        /// </summary>
        public static IDoorBuilder Create(string doorName, Guid? uid = null)
        {
            return new DoorStartBuilder(doorName, uid);
        }
    }
}
