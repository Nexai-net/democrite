// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Core.Abstractions.Streams;

    /// <summary>
    /// Auto configurator used to configure default democrite stream container <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IDemocriteNodeStreamQueueBuilder}" />
    public interface INodeStreamQueueDemocriteDefaultAutoConfigurator : IAutoKeyConfigurator<IDemocriteExtensionBuilderTool>
    {
    }
}
