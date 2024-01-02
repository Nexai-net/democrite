// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Signals;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Trigger definition used to setup a timer - cron
    /// </summary>
    /// <seealso cref="TriggerBaseDefinition" />
    [Immutable]
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class CronTriggerDefinition : TriggerDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CronTriggerDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public CronTriggerDefinition(Guid uid,
                                     IEnumerable<Guid> targetSequenceIds,
                                     IEnumerable<SignalId> targetSignalIds,
                                     bool enabled,
                                     string cronExpression,
                                     InputSourceDefinition? inputSourceDefinition = null)
            : base(uid,
                   TriggerTypeEnum.Cron,
                   targetSequenceIds,
                   targetSignalIds,
                   enabled,
                   inputSourceDefinition)
        {
            this.CronExpression = cronExpression;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public string CronExpression { get; }

        #endregion
    }
}
