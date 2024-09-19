// Copyright (c) Nexai.
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
        public static ISignalBuilder StartBuilding(string simpleNameIdentifier, string? displayName = null, Guid? fixUid = null, Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            return new SignalBuilder(simpleNameIdentifier, displayName, fixUid, metadataBuilder);
        }

        /// <summary>
        /// Creates the new signal.
        /// </summary>
        public static SignalDefinition Create(string simpleNameIdentifier, string? displayName = null, Guid? fixUid = null, Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            return new SignalBuilder(simpleNameIdentifier, displayName, fixUid, metadataBuilder).Build();
        }

        /// <summary>
        /// Creates the new signal.
        /// </summary>
        public static SignalDefinition Create(in SignalId signalId, Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(signalId.Name);
            
            return new SignalBuilder(signalId.Name!, signalId.Name ?? "Signal:" + signalId.Uid, signalId.Uid, metadataBuilder).Build();
        }
    }
}
