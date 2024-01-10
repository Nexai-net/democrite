// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Configurations;

    /// <summary>
    /// Auto setup reminder state memory container
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IClusterNodeBuilderDemocriteMemoryBuilder}" />
    public interface INodeReminderStateMemoryAutoConfigurator : IAutoConfigurator<IDemocriteNodeMemoryBuilder>
    {
    }
}
