// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
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
                                       string displayName,
                                       IEnumerable<TriggerTargetDefinition> targets,
                                       bool enabled,
                                       SignalId? listenSignal,
                                       DoorId? listenDoor,
                                       DefinitionMetaData? metaData,
                                       DataSourceDefinition? triggerGlobalOutputDefinition = null)

            : base(uid, displayName, TriggerTypeEnum.Signal, targets, enabled, metaData, triggerGlobalOutputDefinition)
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

        #region Methods

        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return "[ListenSignal: " + this.ListenSignal + "] [ListenDoor: " + this.ListenDoor + "]";
        }

        #endregion
    }
}
