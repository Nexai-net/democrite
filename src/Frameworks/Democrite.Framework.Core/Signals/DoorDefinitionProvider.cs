// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Signals
{
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Provider in charge to give access to <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="IDoorDefinitionProvider" />
    public sealed class DoorDefinitionProvider : ProviderStrategyBase<DoorDefinition, Guid, IDoorDefinitionProviderSource>, IDoorDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorDefinitionProvider"/> class.
        /// </summary>
        public DoorDefinitionProvider(IEnumerable<IDoorDefinitionProviderSource> providerSource, ILogger<DoorDefinitionProvider> logger)
            : base(providerSource, logger)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the fetch by key expressions.
        /// </summary>
        protected override Expression<Func<DoorDefinition, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<Guid> keys)
        {
            return (t => keys.Contains(t.Uid));
        }

        #endregion
    }
}
