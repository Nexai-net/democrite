// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Implementations.Triggers;
    using Democrite.Framework.Builders.Triggers;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Core.Abstractions.Streams;
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
        public static ITriggerDefinitionBuilder<ITriggerDefinitionFinalizeBuilder> Cron(string cronExpressionInUtc, string displayName, Guid? fixUid = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(cronExpressionInUtc);

            return new TriggerDefinitionCronBuilder(cronExpressionInUtc, "TRG:" + displayName, fixUid);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionBuilder<ITriggerDefinitionFinalizeBuilder> Signal(SignalId signalDefinition, string displayName, Guid? fixUid = null)
        {
            if (signalDefinition.Uid == Guid.Empty)
                throw new ArgumentException(nameof(signalDefinition) + " must not be default value");

            return new TriggerDefinitionSignalBuilder(TriggerTypeEnum.Signal, signalDefinition, null, "TRG:" + displayName, fixUid);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionBuilder<ITriggerDefinitionFinalizeBuilder> Signal(SignalDefinition signalDefinition, string displayName, Guid? fixUid = null)
        {
            ArgumentNullException.ThrowIfNull(signalDefinition);
            return Signal(signalDefinition.SignalId, displayName, fixUid);
        }

        /// <summary>
        /// Configure a multiple trigger based on multiple signal fire
        /// </summary>
        public static TriggerDefinition[] Signals(IReadOnlyCollection<SignalId> signalsDefinition, Func<ITriggerDefinitionBuilder<ITriggerDefinitionFinalizeBuilder>, TriggerDefinition> buildTrigger)
        {
            ArgumentNullException.ThrowIfNull(signalsDefinition);
            ArgumentNullException.ThrowIfNull(buildTrigger);

            return signalsDefinition.Select(s =>
                                    {
                                        var fixUid = Guid.NewGuid();
                                        return buildTrigger(Signal(s, fixUid.ToString(), fixUid));
                                    })
                                    .ToArray();
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionBuilder<ITriggerDefinitionFinalizeBuilder> Door(DoorDefinition doorDefinition, string displayName, Guid? fixUid = null)
        {
            ArgumentNullException.ThrowIfNull(doorDefinition);
            return Door(doorDefinition.DoorId, displayName, fixUid);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionBuilder<ITriggerDefinitionFinalizeBuilder> Door(DoorId doorDefinition, string displayName, Guid? fixUid = null)
        {
            if (doorDefinition.Uid == Guid.Empty)
                throw new ArgumentException(nameof(doorDefinition) + " must not be default value");

            return new TriggerDefinitionSignalBuilder(TriggerTypeEnum.Signal, null, doorDefinition, "TRG:" + displayName, fixUid);
        }

        /// <summary>
        /// Configure a multiple trigger based on multiple doors fire
        /// </summary>
        public static TriggerDefinition[] Doors(IReadOnlyCollection<DoorId> doorsDefinition, Func<ITriggerDefinitionBuilder<ITriggerDefinitionFinalizeBuilder>, TriggerDefinition> buildTrigger)
        {
            ArgumentNullException.ThrowIfNull(doorsDefinition);
            ArgumentNullException.ThrowIfNull(buildTrigger);

            return doorsDefinition.Select(s =>
                                  {
                                      var fixUid = Guid.NewGuid();
                                      return buildTrigger(Door(s, fixUid.ToString(), fixUid));
                                  })
                                  .ToArray();
        }

        /// <summary>
        /// Configure a trigger based on a <see cref="Orleans.Stream"/> as input
        /// </summary>
        public static ITriggerDefinitionBuilder<ITriggerDefinitionStreamFinalizeBuilder> Stream(Guid streamSourceDefinitionUid, string displayName, Guid? fixUid = null)
        {
            return new TriggerDefinitionStreamBuilder(streamSourceDefinitionUid, "TRG:" + displayName, fixUid);
        }

        /// <summary>
        /// Configure a trigger based on a <see cref="Orleans.Stream"/> as input
        /// </summary>
        public static ITriggerDefinitionBuilder<ITriggerDefinitionStreamFinalizeBuilder> Stream(StreamQueueDefinition streamDefinition, string? displayName = null, Guid? fixUid = null)
        {
            return Stream(streamDefinition.Uid,
                          !string.IsNullOrEmpty(displayName) ? displayName : "Consumer Trigger : " + streamDefinition.DisplayName,
                          fixUid);
        }
    }
}
