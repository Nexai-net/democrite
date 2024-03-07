// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Sequence;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <inheritdoc cref="ISequenceDefinitionProvider"/>
    /// <seealso cref="ISequenceDefinitionProvider" />
    internal sealed class SequenceDefinitionProvider : DefinitionBaseProvider<SequenceDefinition, ISequenceDefinitionSourceProvider>, ISequenceDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinitionProvider"/> class.
        /// </summary>
        public SequenceDefinitionProvider(IEnumerable<ISequenceDefinitionSourceProvider> specificDefinitionProviderSources,
                                          IEnumerable<IDefinitionSourceProvider<SequenceDefinition>> genericDefinitionSourceProviders,
                                          ILogger<ISequenceDefinitionSourceProvider> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion
    }
}
