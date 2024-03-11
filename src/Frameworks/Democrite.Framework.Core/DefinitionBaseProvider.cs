// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Elvex.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <inheritdoc cref="ISequenceDefinitionProvider"/>
    /// <seealso cref="ISequenceDefinitionProvider" />
    public abstract class DefinitionBaseProvider<TDefinition, TSpecificProvider> : ProviderStrategyBase<TDefinition, Guid, IDefinitionSourceProvider<TDefinition>>
        where TDefinition : class, IDefinition
        where TSpecificProvider : IDefinitionSourceProvider<TDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinitionProvider"/> class.
        /// </summary>
        public DefinitionBaseProvider(IEnumerable<TSpecificProvider> specificDefinitionProviderSources,
                                      IEnumerable<IDefinitionSourceProvider<TDefinition>> genericDefinitionSourceProviders,
                                      ILogger<TSpecificProvider> logger)

            : base(specificDefinitionProviderSources.Cast<IDefinitionSourceProvider<TDefinition>>()
                                                    .Concat(genericDefinitionSourceProviders).Distinct(), logger)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the fetch by key expressions.
        /// </summary>
        protected override Expression<Func<TDefinition, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<Guid> keys)
        {
            return (t => keys.Contains(t.Uid));
        }

        #endregion
    }
}
