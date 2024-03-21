// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Triggers
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using Elvex.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// In memory provider source of <see cref="ITriggerDefinition"/>
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{TriggerDefinition, Guid}" />
    /// <seealso cref="IStreamQueueDefinitionProviderSource" />
    public sealed class InMemoryTriggerDefinitionProviderSource : InMemoryBaseDefinitionProvider<TriggerDefinition>, ITriggerDefinitionProviderSource, IDefinitionInMemoryFillSourceProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTriggerDefinitionProviderSource"/> class.
        /// </summary>
        public InMemoryTriggerDefinitionProviderSource(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
