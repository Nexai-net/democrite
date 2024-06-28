// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Configurations; to easy configuration use
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Configurations;

    using Microsoft.Extensions.DependencyInjection;

    using System.Diagnostics;

    public static class DemocriteNodeWizardBlackboardExtensions
    {
        #region Fields

        private static readonly string s_blackboardBuilderKey = nameof(DemocriteNodeBlackboardsBuilder) + Guid.NewGuid().ToString();

        #endregion

        #region Methods

        /// <summary>
        /// Setups blackboards
        /// </summary>
        public static IDemocriteNodeWizard UseBlackboards(this IDemocriteNodeWizard wizard, Action<IDemocriteNodeBlackboardsBuilder>? builder = null)
        {
            var blackboardAnalyzeFeedbackSignalDef = Signal.Create(BlackboardConstants.BlackboardAnalyzeFeedbackSignal, m => m.Description("Signal used as root for blackboard to get analyze result")
                                                                                                                              .AddTags("Feedback", "Blackboard")
                                                                                                                              .CategoryChain("Blackboard", "Signals", "Feedback"));

            wizard.AddInMemoryDefinitionProvider(p =>
            {
                p.SetupSignals(blackboardAnalyzeFeedbackSignalDef);
            });

            var bbBuilder = GetBlackboardBuilder(wizard.ConfigurationTools);
            builder?.Invoke(bbBuilder);
            return wizard;
        }

        /// <summary>
        /// Setups blackboards definitions
        /// </summary>
        public static IDemocriteNodeLocalDefinitionsBuilder SetupBlackboardTemplates(this IDemocriteNodeLocalDefinitionsBuilder wizard, params BlackboardTemplateDefinition[] templateDefinitions)
        {
            var bbBuilder = GetBlackboardBuilder(wizard.ConfigurationTools);
            bbBuilder.AddInMemoryDefinitionProvider(m => m.SetupTemplates(templateDefinitions));
            return wizard;
        }

        /// <summary>
        /// Gets the blackboard builder.
        /// </summary>
        private static DemocriteNodeBlackboardsBuilder GetBlackboardBuilder(IDemocriteExtensionBuilderTool wizard)
        {
            var blackboardBuilder = wizard.GetServiceCollection().GetServiceByKey<string, DemocriteNodeBlackboardsBuilder>(s_blackboardBuilderKey);

            if (blackboardBuilder is null)
            {
                var newBuilder = new DemocriteNodeBlackboardsBuilder(wizard);
                wizard.GetServiceCollection().AddKeyedSingleton(s_blackboardBuilderKey, newBuilder);
                newBuilder.BuildServices();
                return newBuilder;
            }

            Debug.Assert(blackboardBuilder.KeyedImplementationInstance is not null);
            return (DemocriteNodeBlackboardsBuilder)blackboardBuilder.KeyedImplementationInstance;
        }

        #endregion
    }
}