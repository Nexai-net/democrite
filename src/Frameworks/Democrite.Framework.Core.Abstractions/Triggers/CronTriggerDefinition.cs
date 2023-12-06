// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Triggers
{
    using Democrite.Framework.Core.Abstractions.Inputs;
    using Democrite.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Trigger definition used to setup a timer - cron
    /// </summary>
    /// <seealso cref="TriggerBaseDefinition" />
    public sealed class CronTriggerDefinition : TriggerDefinition, ICronTriggerDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CronTriggerDefinition{TTriggerOutput}"/> class.
        /// </summary>
        public CronTriggerDefinition(Guid uid,
                                     string cronExpression,
                                     IEnumerable<Guid> targetSequenceIds,
                                     IEnumerable<SignalId> targetSignalIds,
                                     bool enabled,
                                     InputSourceDefinition? triggerInputSourceDefinition = null)
            : base(uid,
                   TriggerTypeEnum.Cron,
                   targetSequenceIds,
                   targetSignalIds,
                   enabled,
                   triggerInputSourceDefinition)
        {
            this.CronExpression = cronExpression;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public string CronExpression { get; }

        #endregion
    }
}
