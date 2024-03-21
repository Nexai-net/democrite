// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Streams
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Streams;

    using Elvex.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider stat store in memory the definition
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{StreamQueueDefinition, System.Guid}" />
    /// <seealso cref="IStreamQueueDefinitionProviderSource" />
    public sealed class InMemoryStreamQueueDefinitionProviderSource : InMemoryBaseDefinitionProvider<StreamQueueDefinition>, IStreamQueueDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryStreamQueueDefinitionProviderSource"/> class.
        /// </summary>
        public InMemoryStreamQueueDefinitionProviderSource(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
