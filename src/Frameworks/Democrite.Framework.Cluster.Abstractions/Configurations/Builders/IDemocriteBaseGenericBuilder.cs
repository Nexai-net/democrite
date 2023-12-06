// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Abstractions.Configurations.Builders
{
    /// <summary>
    /// Base builder used to provide support for services building client and node side
    /// </summary>
    public interface IDemocriteBaseGenericBuilder
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is client.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is client; otherwise, <c>false</c>.
        /// </value>
        bool IsClient { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is server node.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is server node; otherwise, <c>false</c>.
        /// </value>
        bool IsServerNode { get; }

        /// <summary>
        /// Gets the source orlean builder. SiloOrClient
        /// </summary>
        object SourceOrleanBuilder { get; }

        #endregion
    }
}
