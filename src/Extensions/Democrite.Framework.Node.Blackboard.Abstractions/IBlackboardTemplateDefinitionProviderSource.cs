// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;

    /// <summary>
    /// Source provider of <see cref="BlackboardTemplateDefinition"/>
    /// </summary>
    /// <seealso cref="IProviderStrategySource{BlackboardTemplateDefinition, Guid}" />
    /// <seealso cref="IInitService" />
    public interface IBlackboardTemplateDefinitionProviderSource : IDefinitionSourceProvider<BlackboardTemplateDefinition>
    {
    }
}
