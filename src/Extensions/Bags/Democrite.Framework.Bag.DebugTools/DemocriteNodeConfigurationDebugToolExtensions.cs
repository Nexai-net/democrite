// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Node.Configurations
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Bag.DebugTools;
    using Democrite.Framework.Bag.DebugTools.Grains;
    using Democrite.Framework.Bag.DebugTools.Models;
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extensions methods used to configure all the debug tools
    /// </summary>
    public static class DemocriteNodeConfigurationDebugToolExtensions
    {
        /// <summary>
        /// Adds the debug tools, (Sequence, vgrins ...).
        /// </summary>
        public static IDemocriteNodeWizard AddDebugTools(this IDemocriteNodeWizard democriteNodeWizard, LogLevel logLevel)
        {
            return democriteNodeWizard.AddDebugTools(new DebugDisplayInfoOptions(logLevel));
        }

        /// <summary>
        /// Adds the debug tools, (Sequence, vgrins ...).
        /// </summary>
        public static IDemocriteNodeWizard AddDebugTools(this IDemocriteNodeWizard democriteNodeWizard, DebugDisplayInfoOptions? option = null)
        {
            if (option != null)
                democriteNodeWizard.AddNodeOption(option);

            democriteNodeWizard.SetupNodeVGrains(g =>
            {
                g.Add<IDisplayInfoVGrain, DisplayAllInfoVGrain>()
                 .Add<IDisplaySignalsInfoVGrain, DisplaySignalsInfoVGrain>();
            });

            var signalSebugSec = Sequence.Build("display-signal",
                                                nameof(Democrite.Framework.Bag) + ":" + nameof(Democrite.Framework.Bag.DebugTools) + ":" + nameof(DebugToolConstants.DisplaySignalSequence), 
                                                DebugToolConstants.DisplaySignalSequence, 
                                                o =>
                                                {
                                                    o.PreventSequenceExecutorStateStorage()
                                                     .MinimalLogLevel(option?.LogLevel ?? DebugDisplayInfoOptions.Default.LogLevel);
                                                },
                                                m =>
                                                {
                                                    m.Namespace(DebugToolConstants.BAG_NAMESPACE)
                                                     .AddTags("debug", "sequence", "display")
                                                     .CategoryChain("utilities", "debug");
                                                })
                                                .RequiredInput<SignalMessage>()
                                                .Use<IDisplaySignalsInfoVGrain>().Call((g, s, ctx) => g.DisplaySignalInfoAsync(s, ctx)).Return
                                                .Build();

            democriteNodeWizard.AddInMemoryDefinitionProvider(s => s.SetupSequences(signalSebugSec));

            return democriteNodeWizard;
        }

        /// <summary>
        /// Get triggers that will display on the logger the signal each time this one is fire
        /// </summary>
        public static IDemocriteNodeWizard ShowSignals(this IDemocriteNodeWizard democriteNodeWizard, params SignalDefinition[] signalDefinitions)
        {
            democriteNodeWizard.AddInMemoryDefinitionProvider(s =>
            {
                var triggers = signalDefinitions.Select(d => Trigger.Signal(d,
                                                                            "auto-signal-trigger-display-" + RefIdHelper.GetSimpleNameIdentification(d.RefId),
                                                                            "AutoTriggerShow:" + d.DisplayName,
                                                                            d.Uid,
                                                                            metadataBuilder : m => m.Namespace(DebugToolConstants.BAG_NAMESPACE))
                                                                    .AddTargetSequence(DebugToolConstants.DisplaySignalSequence)
                                                                    .Build())
                                                .ToArray();

                s.SetupTriggers(triggers);
            });

            return democriteNodeWizard;
        }

        /// <summary>
        /// Get triggers that will display on the logger the door each time this one fire
        /// </summary>
        public static IDemocriteNodeWizard ShowDoors(this IDemocriteNodeWizard democriteNodeWizard, params DoorDefinition[] doorDefinitions)
        {
            democriteNodeWizard.AddInMemoryDefinitionProvider(s =>
            {
                var triggers = doorDefinitions.Select(d => Trigger.Door(d,
                                                                        "auto-door-trigger-display-" + RefIdHelper.GetSimpleNameIdentification(d.RefId),
                                                                        "AutoTriggerShow:" + d.DisplayName,
                                                                        d.Uid,
                                                                        metadataBuilder: m => m.Namespace(DebugToolConstants.BAG_NAMESPACE))
                                                                  .AddTargetSequence(DebugToolConstants.DisplaySignalSequence)
                                                                  .Build())
                                              .ToArray();

                s.SetupTriggers(triggers);
            });

            return democriteNodeWizard;
        }
    }
}
