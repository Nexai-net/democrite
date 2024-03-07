// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Signals
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Signals;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    /// Provider in charge to give access to <see cref="SignalDefinition"/>
    /// </summary>
    /// <seealso cref="ISignalDefinitionProvider" />
    public sealed class SignalDefinitionProvider : DefinitionBaseProvider<SignalDefinition, ISignalDefinitionProviderSource>, ISignalDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalDefinitionProvider"/> class.
        /// </summary>
        public SignalDefinitionProvider(IEnumerable<ISignalDefinitionProviderSource> specificDefinitionProviderSources,
                                        IEnumerable<IDefinitionSourceProvider<SignalDefinition>> genericDefinitionSourceProviders,
                                        ILogger<ISignalDefinitionProviderSource> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion
    }
}
