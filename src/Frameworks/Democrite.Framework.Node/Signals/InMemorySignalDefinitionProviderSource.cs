// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Elvex.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// In memory provider source of <see cref="SignalDefinition"/>
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{SignalDefinition, Guid}" />
    /// <seealso cref="ISignalDefinitionProviderSource" />
    public sealed class InMemorySignalDefinitionProviderSource : InMemoryBaseDefinitionProvider<SignalDefinition>, ISignalDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySignalDefinitionProviderSource"/> class.
        /// </summary>
        public InMemorySignalDefinitionProviderSource(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
