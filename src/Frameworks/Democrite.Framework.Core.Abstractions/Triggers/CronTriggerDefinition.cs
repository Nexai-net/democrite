// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Trigger definition used to setup a timer - cron
    /// </summary>
    /// <seealso cref="TriggerBaseDefinition" />
    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
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
                                     string displayName,
                                     IEnumerable<TriggerTargetDefinition> targets,
                                     bool enabled,
                                     string cronExpression,
                                     bool useSecond,
                                     DefinitionMetaData? metaData,
                                     DataSourceDefinition? triggerGlobalOutputDefinition = null)
            : base(uid,
                   displayName,
                   TriggerTypeEnum.Cron,
                   targets,
                   enabled,
                   metaData,
                   triggerGlobalOutputDefinition)
        {
            this.CronExpression = cronExpression;
            this.UseSecond = useSecond;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public string CronExpression { get; }

        /// <inheritdoc />
        [Id(1)]
        [DataMember]
        public bool UseSecond { get; }

        #endregion


        #region Methods

        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return "[CronExpression: " + this.CronExpression + "] [UseSecond: " + this.UseSecond + "]";
        }

        #endregion
    }
}
