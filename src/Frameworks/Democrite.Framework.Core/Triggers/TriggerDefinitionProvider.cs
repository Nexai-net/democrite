// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Triggers
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Triggers;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    /// Provider in charge to give access to <see cref="TriggerDefinition"/>
    /// </summary>
    /// <seealso cref="ITriggerDefinitionProvider" />
    public sealed class TriggerDefinitionProvider : DefinitionBaseProvider<TriggerDefinition, ITriggerDefinitionProviderSource>, ITriggerDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionProvider"/> class.
        /// </summary>
        public TriggerDefinitionProvider(IEnumerable<ITriggerDefinitionProviderSource> specificDefinitionProviderSources,
                                         IEnumerable<IDefinitionSourceProvider<TriggerDefinition>> genericDefinitionSourceProviders,
                                         ILogger<ITriggerDefinitionProviderSource> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion
    }
}
