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
        public static ITriggerDefinitionCronBuilder Cron(string cronExpressionInUtc,
                                                         string simpleNameIdentifier,
                                                         string? displayName = null,
                                                         Guid? fixUid = null,
                                                         Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(cronExpressionInUtc);

            return new TriggerDefinitionCronBuilder(cronExpressionInUtc,
                                                    simpleNameIdentifier,       
                                                    displayName ?? simpleNameIdentifier,
                                                    fixUid,
                                                    metadataBuilder);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionSignalBuilder Signal(SignalId signalDefinition,
                                                             string simpleNameIdentifier,
                                                             string? displayName = null,
                                                             Guid? fixUid = null,
                                                             Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            if (signalDefinition.Uid == Guid.Empty)
                throw new ArgumentException(nameof(signalDefinition) + " must not be default value");

            return new TriggerDefinitionSignalBuilder(TriggerTypeEnum.Signal,
                                                      signalDefinition,
                                                      null,
                                                      simpleNameIdentifier,
                                                      displayName ?? simpleNameIdentifier,
                                                      fixUid,
                                                      metadataBuilder);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionSignalBuilder Signal(SignalDefinition signalDefinition,
                                                             string simpleNameIdentifier,
                                                             string? displayName,
                                                             Guid? fixUid = null,
                                                             Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            ArgumentNullException.ThrowIfNull(signalDefinition);
            return Signal(signalDefinition.SignalId, simpleNameIdentifier, displayName, fixUid, metadataBuilder);
        }

        /// <summary>
        /// Configure a multiple trigger based on multiple signal fire
        /// </summary>
        public static TriggerDefinition[] Signals(IReadOnlyCollection<SignalId> signalsDefinition, Func<ITriggerDefinitionBuilder, TriggerDefinition> buildTrigger)
        {
            ArgumentNullException.ThrowIfNull(signalsDefinition);
            ArgumentNullException.ThrowIfNull(buildTrigger);

            return signalsDefinition.Select(s =>
                                    {
                                        var fixUid = Guid.NewGuid();
                                        return buildTrigger(Signal(s, fixUid.ToString(), fixUid.ToString(), fixUid));
                                    })
                                    .ToArray();
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionDoorBuilder Door(DoorDefinition doorDefinition,
                                                         string simpleNameIdentifier,
                                                         string? displayName = null,
                                                         Guid? fixUid = null,
                                                         Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            ArgumentNullException.ThrowIfNull(doorDefinition);
            return Door(doorDefinition.DoorId, simpleNameIdentifier, displayName, fixUid, metadataBuilder);
        }

        /// <summary>
        /// Configure a trigger based on signal fire
        /// </summary>
        public static ITriggerDefinitionDoorBuilder Door(DoorId doorDefinition,
                                                         string simpleNameIdentifier,
                                                         string? displayName = null,
                                                         Guid? fixUid = null,
                                                         Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            if (doorDefinition.Uid == Guid.Empty)
                throw new ArgumentException(nameof(doorDefinition) + " must not be default value");

            return new TriggerDefinitionSignalBuilder(TriggerTypeEnum.Signal,
                                                      null,
                                                      doorDefinition,
                                                      simpleNameIdentifier,
                                                      displayName ?? simpleNameIdentifier,
                                                      fixUid,
                                                      metadataBuilder);
        }

        /// <summary>
        /// Configure a multiple trigger based on multiple doors fire
        /// </summary>
        public static TriggerDefinition[] Doors(IReadOnlyCollection<DoorId> doorsDefinition,
                                                Func<ITriggerDefinitionBuilder, TriggerDefinition> buildTrigger)
        {
            ArgumentNullException.ThrowIfNull(doorsDefinition);
            ArgumentNullException.ThrowIfNull(buildTrigger);

            return doorsDefinition.Select(s =>
                                  {
                                      var fixUid = Guid.NewGuid();
                                      return buildTrigger(Door(s, fixUid.ToString(), fixUid.ToString(), fixUid));
                                  })
                                  .ToArray();
        }

        /// <summary>
        /// Configure a trigger based on a <see cref="Orleans.Stream"/> as input
        /// </summary>
        public static ITriggerDefinitionStreamBuilder Stream(Guid streamSourceDefinitionUid,
                                                             string simpleNameIdentifier,
                                                             string? displayName = null,
                                                             Guid? fixUid = null,
                                                             Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            return new TriggerDefinitionStreamBuilder(streamSourceDefinitionUid,
                                                      simpleNameIdentifier,
                                                      displayName ?? simpleNameIdentifier,
                                                      fixUid,
                                                      metadataBuilder);
        }

        /// <summary>
        /// Configure a trigger based on a <see cref="Orleans.Stream"/> as input
        /// </summary>
        public static ITriggerDefinitionStreamBuilder Stream(StreamQueueDefinition streamDefinition,
                                                             string simpleNameIdentifier,
                                                             string? displayName = null,
                                                             Guid? fixUid = null,
                                                             Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
        {
            return Stream(streamDefinition.Uid,
                          simpleNameIdentifier,
                          !string.IsNullOrEmpty(displayName) ? displayName : "Consumer Trigger : " + streamDefinition.DisplayName,
                          fixUid,
                          metadataBuilder);
        }
    }
}
