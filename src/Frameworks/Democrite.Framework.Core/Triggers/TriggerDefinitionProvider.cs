// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Triggers
{
    using Democrite.Framework.Core.Abstractions.Triggers;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Provider in charge to give access to <see cref="TriggerDefinition"/>
    /// </summary>
    /// <seealso cref="ITriggerDefinitionProvider" />
    public sealed class TriggerDefinitionProvider : ProviderStrategyBase<TriggerDefinition, Guid, ITriggerDefinitionProviderSource>, ITriggerDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionProvider"/> class.
        /// </summary>
        public TriggerDefinitionProvider(IEnumerable<ITriggerDefinitionProviderSource> providerSource, ILogger<TriggerDefinitionProvider> logger)
            : base(providerSource, logger)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the fetch by key expressions.
        /// </summary>
        protected override Expression<Func<TriggerDefinition, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<Guid> keys)
        {
            return (t => keys.Contains(t.Uid));
        }

        #endregion
    }
}
