// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Abstractions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider of <see cref="BlackboardTemplateDefinition"/> using all the <see cref="IBlackboardTemplateDefinitionProviderSource"/>
    /// </summary>
    /// <seealso cref="IProviderStrategy{BlackboardTemplateDefinition, Guid}" />
    /// <seealso cref="IInitService" />
    public interface IBlackboardTemplateDefinitionProvider : IProviderStrategy<BlackboardTemplateDefinition, Guid>
    {
    }
}
