// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Configurations; to easy configuration use
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Node.StreamQueue;
    using Democrite.Framework.Node.StreamQueue.Configurations;

    public static class DemocriteNodeWizardStreamQueueExtensions
    {
        /// <summary>
        /// Setup all services to enabled Streaming/Queuing management on this node.
        /// </summary>
        public static IDemocriteNodeWizard UseStreamQueues(this IDemocriteNodeWizard democriteNodeWizard, bool configureDemocriteDefaultStream = true)
        {
            Action<IDemocriteNodeStreamQueueBuilder>? cfg = null;

            if (configureDemocriteDefaultStream)
                cfg = b => b.SetupDefaultDemocriteMemoryStream();

            return UseStreamQueues(democriteNodeWizard, cfg);
        }

        /// <summary>
        /// Setup all services to enabled Streaming/Queuing management on this node.
        /// </summary>
        public static IDemocriteNodeWizard UseStreamQueues(this IDemocriteNodeWizard democriteNodeWizard, Action<IDemocriteNodeStreamQueueBuilder>? cfg)
        {
            democriteNodeWizard.ConfigureServices(s => s.AddGrainService<StreamTriggerVGrainService>());

            if (cfg is not null)
            {
                var builder = new DemocriteNodeStreamQueueBuilder(democriteNodeWizard.ConfigurationTools);
                cfg(builder);
            }

            return democriteNodeWizard;
        }
    }
}