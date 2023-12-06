// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <inheritdoc cref="ISequenceDefinitionProvider"/>
    /// <seealso cref="ISequenceDefinitionProvider" />
    internal sealed class SequenceDefinitionProvider : ProviderStrategyBase<SequenceDefinition, Guid, ISequenceDefinitionSourceProvider>, ISequenceDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinitionProvider"/> class.
        /// </summary>
        public SequenceDefinitionProvider(IEnumerable<ISequenceDefinitionSourceProvider> artifactResourceProviderSources,
                                          ILogger<ISequenceDefinitionSourceProvider> logger)
            : base(artifactResourceProviderSources, logger)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the fetch by key expressions.
        /// </summary>
        protected override Expression<Func<SequenceDefinition, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<Guid> keys)
        {
            return (t => keys.Contains(t.Uid));
        }

        #endregion
    }
}
