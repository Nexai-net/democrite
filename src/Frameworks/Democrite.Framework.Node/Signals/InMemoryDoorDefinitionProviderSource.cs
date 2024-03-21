// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Elvex.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// In memory provider source of <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{DoorDefinition, Guid}" />
    /// <seealso cref="IDoorDefinitionProviderSource" />
    public sealed class InMemoryDoorDefinitionProviderSource : InMemoryBaseDefinitionProvider<DoorDefinition>, IDoorDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDoorDefinitionProviderSource"/> class.
        /// </summary>
        public InMemoryDoorDefinitionProviderSource(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
