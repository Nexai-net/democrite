// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// Keep : Democrite.Framework.Configurations; to easy configuration use
namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Extensions.Docker;
    using Democrite.Framework.Extensions.Docker.Abstractions;
    using Democrite.Framework.Extensions.Docker.Abstractions.Options;
    using Democrite.Framework.Node.Abstractions.Artifacts;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions use to setup docker artifact environment 
    /// </summary>

    public static class DemocriteDockerBuilderWizardStartExtensions
    {
        #region Methods

        /// <summary>
        /// Setup Mongo db cluster synchronication
        /// </summary>
        public static IDemocriteNodeWizard EnableDockerHost(this IDemocriteNodeWizard wizard)
        {
            wizard.GetServiceCollection().AddSingleton<DockerDemocriteEnvironmentService>();
            wizard.AddService<IDockerClientFactory, DockerClientFactory>();
            wizard.AddService<IArtifactExecutorDedicatedFactory, ArtifactExecutorDockerDedicatedFactory>();
            wizard.AddService<IDockerProcessorFactory, DockerProcessorFactory>();
            return wizard;
        }

        /// <summary>
        /// Adds the docker option.
        /// </summary>
        public static IDemocriteNodeWizard AddDockerOption(this IDemocriteNodeWizard wizard, DockerEnvironementOptions option)
        {
            wizard.AddNodeOption(option);
            return wizard;
        }

        /// <summary>
        /// Adds the docker option.
        /// </summary>
        public static IDemocriteNodeWizard AddDockerOption(this IDemocriteNodeWizard wizard, string optionSectionPath)
        {
            wizard.AddNodeOption<DockerEnvironementOptions>(optionSectionPath);
            return wizard;
        }

        #endregion
    }
}
