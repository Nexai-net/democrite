// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.UnitTests
{
    using Democrite.Framework.Node.Configurations;

    using NFluent;

    /// <summary>
    /// Unit test about <see cref="DemocriteNode"/>
    /// </summary>
    public sealed class ClusterNodeUnitTests
    {
        /// <summary>
        /// ClusterNode ctor.
        /// </summary>
        [Fact]
        public async Task ClusterNode_Ctor()
        {
            await using (var node = DemocriteNode.Create(cfg => cfg.SetupClusterOption(opt => opt.BlockAutoConfig())
                                                                   .WizardConfig()
                                                                   .NoCluster()))
            {
            }
        }

        /// <summary>
        /// ClusterNode manually config
        /// </summary>
        [Fact]
        public async Task ClusterNode_Manually()
        {
            bool haveBeenConfig = false;

            var node = DemocriteNode.Create(cfg =>
            {
                cfg.SetupClusterOption(opt => opt.BlockAutoConfig())
                   .WizardConfig()
                   .NoCluster();

                var siloConfig = cfg.ManualyAdvancedConfig();
                Check.That(siloConfig).IsNotNull();

                haveBeenConfig = true;
            });

            await using (node)
            {
            }

            Check.That(haveBeenConfig).IsTrue();
        }
    }
}