// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// In memory provider source of <see cref="SignalDefinition"/>
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{SignalDefinition, Guid}" />
    /// <seealso cref="ISignalDefinitionProviderSource" />
    public sealed class InMemorySignalDefinitionProviderSource : ProviderStrategyBaseSource<SignalDefinition, Guid>, ISignalDefinitionProviderSource
    {
        #region Methods

        /// <summary>
        /// Adds or update a new artefact
        /// </summary>
        public void AddOrUpdate(SignalDefinition triggerDefinition)
        {
            base.SafeAddOrReplace(triggerDefinition.Uid, triggerDefinition);
        }

        #endregion
    }
}
