// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.StreamQueue.Configurations.AutoConfigurators
{
    using Democrite.Framework.Cluster.Abstractions.Configurations.Builders;
    using Democrite.Framework.Core.Abstractions.Streams;
    using Democrite.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using Democrite.Framework.Node.StreamQueue.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Configure memory stream dedicated to default democrite stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"
    /// </summary>
    /// <seealso cref="INodeStreamQueueDemocriteDefaultAutoConfigurator" />
    public sealed class AutoMemoryStreamQueueDemocriteDefaultConfigurator : INodeStreamQueueDemocriteDefaultAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoMemoryStreamQueueDemocriteDefaultConfigurator"/> class.
        /// </summary>
        static AutoMemoryStreamQueueDemocriteDefaultConfigurator()
        {
            Default = new AutoMemoryStreamQueueDemocriteDefaultConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoMemoryStreamQueueDemocriteDefaultConfigurator Default { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Automatics configure democrite section.
        /// </summary>
        public void AutoConfigure(IDemocriteExtensionBuilderTool democriteBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  string configurationKey,
                                  string? key)
        {
            AutoConfigureImpl(democriteBuilderWizard, logger, true);
        }

        /// <summary>
        /// Automatics configure democrite section.
        /// </summary>
        internal void AutoConfigureImpl(IDemocriteExtensionBuilderTool democriteBuilderWizard,
                                        ILogger logger,
                                        bool CreateInMemoryPubSubStorage)
        {
            AutoMemoryStreamQueueConfigurator.Default.AutoConfigureImpl(democriteBuilderWizard,
                                                                        logger,
                                                                        new Models.MemoryStreamQueueOption(StreamQueueDefinition.DEFAULT_STREAM_KEY, CreateInMemoryPubSubStorage));
        }

        #endregion
    }
}
