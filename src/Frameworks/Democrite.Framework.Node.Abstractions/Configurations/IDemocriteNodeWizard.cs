// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations;
    using Democrite.Framework.Node.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.Abstractions.Models;

    using System;

    /// <summary>
    ///  Cluster node configuration wizard tools
    /// </summary>
    public interface IDemocriteNodeWizard : IDemocriteWizard<IDemocriteNodeWizard, IDemocriteNodeConfigurationWizard>
    {
        /// <summary>
        /// Add a <see cref="INodeOptions"/>.
        /// </summary>
        IDemocriteNodeWizard AddNodeOption<TOption>(TOption option) where TOption : class, INodeOptions;

        /// <summary>
        /// Add a <see cref="INodeOptions"/> using configuration section.
        /// </summary>
        IDemocriteNodeWizard AddNodeOption<TOption>(string configurationSection) where TOption : class, INodeOptions;

        /// <summary>
        /// Manually setup the cluster vgrains
        /// </summary>
        IDemocriteNodeWizard SetupNodeVGrains(Action<IDemocriteNodeVGrainWizard> config);

        /// <summary>
        /// Setups in local memory definitions
        /// </summary>
        IDemocriteNodeWizard SetupInMemoryDefintions(Action<IDemocriteNodeLocalDefinitionsBuilder> config);

        /// <summary>
        /// Setups the nodes memories, how to same state, reminder, trigger ...
        /// </summary>
        IDemocriteNodeWizard SetupNodeMemories(Action<IDemocriteNodeMemoryBuilder> memoryBuilder);
    }
}
