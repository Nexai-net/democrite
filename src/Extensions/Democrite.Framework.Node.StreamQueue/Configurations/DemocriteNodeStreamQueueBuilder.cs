// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Node.StreamQueue.Configurations.AutoConfigurators;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Builder in charge to configure stream in the node
    /// </summary>
    internal sealed class DemocriteNodeStreamQueueBuilder : IDemocriteNodeStreamQueueBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DemocriteNodeStreamQueueBuilder"/> class.
        /// </summary>
        public DemocriteNodeStreamQueueBuilder(IDemocriteExtensionBuilderTool tool)
        {
            this.ConfigurationTools = tool;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the gneric builder tool.
        /// </summary>
        public IDemocriteExtensionBuilderTool ConfigurationTools { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Setups a cluster memory stream available only the cluster memory with default container name <see cref="Democrite.Framework.Core.Abstractions.Streams.StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <param name="createMemoryPubSubStorage">Get also the default memory storage need for memory stream with key "PubSubStore"; otherwise you will have to provide the needed storage</param>
        public IDemocriteNodeStreamQueueBuilder SetupDefaultDemocriteMemoryStream(bool createMemoryPubSubStorage = true)
        {
            AutoMemoryStreamQueueDemocriteDefaultConfigurator.Default.AutoConfigureImpl(this.ConfigurationTools, this.ConfigurationTools.Logger, createMemoryPubSubStorage);
            return this;
        }

        /// <summary>
        /// Setups a cluster memory stream available only the cluster memory
        /// </summary>
        /// <param name="createMemoryPubSubStorage">Get also the default memory storage need for memory stream with key "PubSubStore"; otherwise you will have to provide the needed storage</param>
        public IDemocriteNodeStreamQueueBuilder SetupMemoryStream(string streamContainerName, bool createMemoryPubSubStorage = true)
        {
            AutoMemoryStreamQueueConfigurator.Default.AutoConfigureImpl(this.ConfigurationTools, this.ConfigurationTools.Logger, new Models.MemoryStreamQueueOption(streamContainerName, createMemoryPubSubStorage));
            return this;
        }

        #endregion
    }
}
