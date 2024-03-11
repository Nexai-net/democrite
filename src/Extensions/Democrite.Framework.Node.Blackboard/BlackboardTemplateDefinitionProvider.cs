// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Elvex.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    /// Default provider with caching system the provide <see cref="BlackboardTemplateDefinition"/> using <see cref="IBlackboardTemplateDefinitionProviderSource"/> configured in the system
    /// </summary>
    /// <seealso cref="ProviderStrategyBase{BlackboardTemplateDefinition, Guid, IBlackboardTemplateDefinitionProviderSource}" />
    /// <seealso cref="IBlackboardTemplateDefinitionProvider" />
    internal sealed class BlackboardTemplateDefinitionProvider : DefinitionBaseProvider<BlackboardTemplateDefinition, IBlackboardTemplateDefinitionProviderSource>, IBlackboardTemplateDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateDefinitionProvider"/> class.
        /// </summary>
        public BlackboardTemplateDefinitionProvider(IEnumerable<IBlackboardTemplateDefinitionProviderSource> specificDefinitionProviderSources,
                                                    IEnumerable<IDefinitionSourceProvider<BlackboardTemplateDefinition>> genericDefinitionSourceProviders,
                                                    ILogger<IBlackboardTemplateDefinitionProviderSource> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion
    }
}
