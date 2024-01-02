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
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class SignalTriggerDefinition : TriggerDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTriggerDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public SignalTriggerDefinition(Guid uid,
                                       IEnumerable<Guid> targetSequenceIds,
                                       IEnumerable<SignalId> targetSignalIds,
                                       bool enabled,
                                       SignalId? listenSignal,
                                       DoorId? listenDoor,
                                       InputSourceDefinition? inputSourceDefinition = null)

            : base(uid, TriggerTypeEnum.Signal, targetSequenceIds, targetSignalIds, enabled, inputSourceDefinition)
        {
            this.ListenSignal = listenSignal;
            this.ListenDoor = listenDoor;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public SignalId? ListenSignal { get; }

        /// <inheritdoc />
        [Id(1)]
        [DataMember] 
        public DoorId? ListenDoor { get; }

        #endregion
    }
}
