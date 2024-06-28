// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Definition about a trigger based on stream reading
    /// </summary>
    /// <seealso cref="TriggerDefinition" />
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class StreamTriggerDefinition : TriggerDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTriggerDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public StreamTriggerDefinition(Guid uid,
                                       string displayName,
                                       IEnumerable<TriggerTargetDefinition> targets,
                                       bool enabled,
                                       uint maxConcurrentProcess,
                                       uint? relativeMaxConcurrentProcess,
                                       Guid streamSourceDefinitionUid,
                                       DefinitionMetaData? metaData)
            : base(uid, displayName, TriggerTypeEnum.Stream, targets, enabled, metaData)
        {
            this.FixedMaxConcurrentProcess = maxConcurrentProcess;
            this.RelativeMaxConcurrentProcess = relativeMaxConcurrentProcess;

            this.StreamSourceDefinitionUid = streamSourceDefinitionUid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the maximum concurrent process.
        /// </summary>
        [Id(0)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [System.Text.Json.Serialization.JsonInclude]
        public uint FixedMaxConcurrentProcess { get; }

        /// <summary>
        /// Gets the relative maximum concurrent process.
        /// </summary>
        [Id(1)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [System.Text.Json.Serialization.JsonInclude]
        public uint? RelativeMaxConcurrentProcess { get; }

        /// <summary>
        /// Gets the stream source definition uid. <see cref="StreamQueueDefinition"/>
        /// </summary>
        [Id(2)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        [System.Text.Json.Serialization.JsonInclude]
        public Guid StreamSourceDefinitionUid { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return "[StreamSourceId: " + this.StreamSourceDefinitionUid + "]" +
                   " [FixedMaxConcurrentProcess: " + this.FixedMaxConcurrentProcess + "]" +
                   " [RelativeMaxConcurrentProcess: " + (this.RelativeMaxConcurrentProcess?.ToString() ?? "null") + "]";
        }

        #endregion
    }
}
