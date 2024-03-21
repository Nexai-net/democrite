// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    using Elvex.Toolbox.Patterns.Strategy;

    /// <summary>
    /// In memory blackboard template definition provider
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{DoorDefinition, Guid}" />
    /// <seealso cref="IDoorDefinitionProviderSource" />
    internal sealed class InMemoryBlackboardTemplateDefinitionProviderSource : InMemoryBaseDefinitionProvider<BlackboardTemplateDefinition>, IBlackboardTemplateDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryBlackboardTemplateDefinitionProviderSource"/> class.
        /// </summary>
        public InMemoryBlackboardTemplateDefinitionProviderSource()
            : base(null!)
        {
        }

        #endregion
    }
}
