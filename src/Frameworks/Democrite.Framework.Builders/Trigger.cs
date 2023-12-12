// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Implementations.Triggers;
    using Democrite.Framework.Builders.Triggers;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using System;

    /// <summary>
    /// Tool used to define trigger
    /// </summary>
    public static class Trigger
    {
        /// <summary>
        /// Configure a trigger based on cron expression <see href="https://fr.wikipedia.org/wiki/Cron"/>
        /// </summary>
        public static ITriggerDefinitionBuilder Cron(string cronExpressionInUtc)
        {
            ArgumentException.ThrowIfNullOrEmpty(cronExpressionInUtc);

            return new TriggerDefinitionCronBuilder(cronExpressionInUtc);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionBuilder Signal(SignalId signalDefinition)
        {
            if (signalDefinition.Uid == Guid.Empty)
                throw new ArgumentException(nameof(signalDefinition) + " must not be default value");

            return new TriggerDefinitionSignalBuilder(TriggerTypeEnum.Signal, signalDefinition, null);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionBuilder Signal(SignalDefinition signalDefinition)
        {
            ArgumentNullException.ThrowIfNull(signalDefinition);
            return Signal(signalDefinition.SignalId);
        }

        /// <summary>
        /// Configure a multiple trigger based on multiple signal fire
        /// </summary>
        public static TriggerDefinition[] Signals(IReadOnlyCollection<SignalId> signalsDefinition, Func<ITriggerDefinitionBuilder, TriggerDefinition> buildTrigger)
        {
            ArgumentNullException.ThrowIfNull(signalsDefinition);
            ArgumentNullException.ThrowIfNull(buildTrigger);

            return signalsDefinition.Select(s => buildTrigger(Signal(s)))
                                    .ToArray();
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionBuilder Door(DoorDefinition doorDefinition)
        {
            ArgumentNullException.ThrowIfNull(doorDefinition);
            return Door(doorDefinition.DoorId);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionBuilder Door(DoorId doorDefinition)
        {
            if (doorDefinition.Uid == Guid.Empty)
                throw new ArgumentException(nameof(doorDefinition) + " must not be default value");

            return new TriggerDefinitionSignalBuilder(TriggerTypeEnum.Signal, null, doorDefinition);
        }

        /// <summary>
        /// Configure a multiple trigger based on multiple doors fire
        /// </summary>
        public static TriggerDefinition[] Doors(IReadOnlyCollection<DoorId> doorsDefinition, Func<ITriggerDefinitionBuilder, TriggerDefinition> buildTrigger)
        {
            ArgumentNullException.ThrowIfNull(doorsDefinition);
            ArgumentNullException.ThrowIfNull(buildTrigger);

            return doorsDefinition.Select(s => buildTrigger(Door(s)))
                                  .ToArray();
        }
    }
}
