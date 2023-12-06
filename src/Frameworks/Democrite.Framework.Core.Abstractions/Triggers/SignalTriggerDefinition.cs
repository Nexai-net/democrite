// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [Immutable]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class SignalTriggerDefinition : TriggerDefinition, ISignalTriggerDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTriggerDefinition"/> class.
        /// </summary>
        public SignalTriggerDefinition(Guid uid,
                                       IEnumerable<Guid> targetSequenceIds,
                                       IEnumerable<SignalId> targetSignalIds,
                                       bool enabled,
                                       SignalId? signalId,
                                       DoorId? doorId,
                                       InputSourceDefinition? inputSourceDefinition = null)

            : base(uid, TriggerTypeEnum.Signal, targetSequenceIds, targetSignalIds, enabled, inputSourceDefinition)
        {
            this.ListenSignal = signalId;
            this.ListenDoor = doorId;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        public SignalId? ListenSignal { get; }

        /// <inheritdoc />
        [Id(1)]
        public DoorId? ListenDoor { get; }

        #endregion
    }
}
