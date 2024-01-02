// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Node.Configurations
namespace Democrite.Framework.Node.Configurations
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Abstractions.Configurations;
    using Democrite.Framework.VGrains.DebugTools;
    using Democrite.Framework.VGrains.DebugTools.Grains;
    using Democrite.Framework.VGrains.DebugTools.Models;

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
            return AddDebugTools(democriteNodeWizard, new DebugDisplayInfoOptions(logLevel));
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

            var signalSebugSec = Sequence.Build(DebugToolConstants.DisplaySignalSequence)
                                         .RequiredInput<SignalMessage>()
                                         .Use<IDisplaySignalsInfoVGrain>().Call((g, s, ctx) => g.DisplaySignalInfoAsync(s, ctx)).Return
                                         .Build();

            democriteNodeWizard.AddInMemoryMongoDefinitionProvider(s => s.SetupSequences(signalSebugSec));

            return democriteNodeWizard;
        }

        /// <summary>
        /// Create triggers that will display on the logger the signal each time this one is fire
        /// </summary>
        public static IDemocriteNodeWizard ShowSignals(this IDemocriteNodeWizard democriteNodeWizard, params SignalDefinition[] signalDefinitions)
        {
            democriteNodeWizard.AddInMemoryMongoDefinitionProvider(s =>
            {
                var displayTriggers = Trigger.Signals(signalDefinitions.Select(signal => signal.SignalId).ToArray(), 
                                                      b => b.AddTargetSequence(DebugToolConstants.DisplaySignalSequence).Build());

                s.SetupTriggers(displayTriggers);
            });

            return democriteNodeWizard;
        }

        /// <summary>
        /// Create triggers that will display on the logger the door each time this one fire
        /// </summary>
        public static IDemocriteNodeWizard ShowDoors(this IDemocriteNodeWizard democriteNodeWizard, params DoorDefinition[] doorDefinitions)
        {
            democriteNodeWizard.AddInMemoryMongoDefinitionProvider(s =>
            {
                var displayTriggers = Trigger.Doors(doorDefinitions.Select(signal => signal.DoorId).ToArray(),
                                                    b => b.AddTargetSequence(DebugToolConstants.DisplaySignalSequence).Build());

                s.SetupTriggers(displayTriggers);
            });

            return democriteNodeWizard;
        }
    }
}
