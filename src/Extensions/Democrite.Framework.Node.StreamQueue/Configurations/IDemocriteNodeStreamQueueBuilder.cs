// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;

    /// <summary>
    /// Builder in charge to configure stream in the node
    /// </summary>
    public interface IDemocriteNodeStreamQueueBuilder 
    {
        #region Properties

        /// <summary>
        /// Gets the gneric builder tool.
        /// </summary>
        IDemocriteExtensionBuilderTool ConfigurationTools { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Setups a cluster memory stream available only the cluster memory with default container name <see cref="Democrite.Framework.Core.Abstractions.Streams.StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <param name="createMemoryPubSubStorage">Get also the default memory storage need for memory stream with key "PubSubStore"; otherwise you will have to provide the needed storage</param>
        IDemocriteNodeStreamQueueBuilder SetupDefaultDemocriteMemoryStream(bool createMemoryPubSubStorage = true);

        /// <summary>
        /// Setups a cluster memory stream available only the cluster memory
        /// </summary>
        /// <param name="createMemoryPubSubStorage">Get also the default memory storage need for memory stream with key "PubSubStore"; otherwise you will have to provide the needed storage</param>
        IDemocriteNodeStreamQueueBuilder SetupMemoryStream(string streamContainerName, bool createMemoryPubSubStorage = true);

        #endregion
    }
}
