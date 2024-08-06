// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Microsoft.Extensions.Logging;

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

        /// <summary>
        /// Gets the build logger.
        /// </summary>
        ILogger Logger { get; } 

        /// <summary>
        /// Gets the build tools.
        /// </summary>
        IClusterBuilderTools Tools { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the cluster option to setup the communition info.
        /// </summary>
        TOptions AddExtensionOption<TOptions>(string configurationSection, TOptions fallbackOption) where TOptions : class;

        /// <summary>
        /// Adds the cluster option to setup the communition info.
        /// </summary>
        void AddExtensionOption<TOptions>(TOptions instances) where TOptions : class;

        /// <summary>
        /// Adds the cluster option to setup the communition info.
        /// </summary>
        void AddExtensionOption<TOptions>(Action<TOptions> options) where TOptions : class;

        /// <summary>
        /// Adds the cluster option to setup the communition info.
        /// </summary>
        TOptions AddExtensionOption<TOptions, TKey>(string configurationSection, TOptions fallbackOption, TKey key) where TOptions : class;

        /// <summary>
        /// Adds the cluster option to setup the communition info.
        /// </summary>
        void AddExtensionOption<TOptions, TKey>(TOptions instances, TKey key) where TOptions : class;

        /// <summary>
        /// Adds the cluster option to setup the communition info.
        /// </summary>
        void AddExtensionOption<TOptions>(Action<TOptions> options, string name) where TOptions : class;

        #endregion
    }
}
