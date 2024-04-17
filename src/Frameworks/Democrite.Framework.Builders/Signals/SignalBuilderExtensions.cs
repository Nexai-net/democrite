// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Builders
namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Signals;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;

    /// <summary>
    /// Extension about signal building
    /// </summary>
    public static class SignalBuilderExtensions
    {
        /// <summary>
        /// Starts the building a sub signal.
        /// </summary>
        public static ISignalBuilder StartBuildSubSignal(this SignalDefinition parentSignal, string signalName, Guid? uid = null, bool contactToParentName = true)
        {
            ArgumentNullException.ThrowIfNull(parentSignal);
            ArgumentNullException.ThrowIfNullOrEmpty(signalName);

            return Signal.StartBuilding(contactToParentName ? parentSignal.Name + ":" + signalName : signalName, uid)
                         .Parent(parentSignal);
        }

        /// <summary>
        /// Starts the building a sub signal.
        /// </summary>
        /// <returns>
        ///     Return sub definition created
        /// </returns>
        public static SignalDefinition CreateSubSignal(this SignalDefinition parentSignal, string signalName, Guid? uid = null, bool contactToParentName = true)
        {
            ArgumentNullException.ThrowIfNull(parentSignal);
            ArgumentNullException.ThrowIfNullOrEmpty(signalName);

            return Signal.StartBuilding(contactToParentName ? parentSignal.Name + ":" + signalName : signalName, uid)
                         .Parent(parentSignal)
                         .Build();
        }

        /// <summary>
        /// Starts the building a sub signal.
        /// </summary>
        /// <returns>
        ///     Root definition
        /// </returns>
        public static SignalDefinition CreateSubSignal(this SignalDefinition parentSignal, string signalName, out SignalDefinition subSignalDefinition, Guid? uid = null, bool contactToParentName = true)
        {
            subSignalDefinition = CreateSubSignal(parentSignal, signalName, uid, contactToParentName);
            return parentSignal;
        }
    }
}
