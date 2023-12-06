// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// In memory provider source of <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{DoorDefinition, Guid}" />
    /// <seealso cref="IDoorDefinitionProviderSource" />
    public sealed class InMemoryDoorDefinitionProviderSource : ProviderStrategyBaseSource<DoorDefinition, Guid>, IDoorDefinitionProviderSource
    {
        #region Methods

        /// <summary>
        /// Adds or update a new artefact
        /// </summary>
        public void AddOrUpdate(DoorDefinition doorDefinition)
        {
            base.SafeAddOrReplace(doorDefinition.Uid, doorDefinition);
        }

        #endregion
    }
}
