// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard
{
    using Democrite.Framework.Core.Abstractions.Doors;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Elvex.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// In memory blackboard template definition provider
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{DoorDefinition, Guid}" />
    /// <seealso cref="IDoorDefinitionProviderSource" />
    internal sealed class InMemoryBlackboardTemplateDefinitionProviderSource : ProviderStrategyBaseSource<BlackboardTemplateDefinition, Guid>, IBlackboardTemplateDefinitionProviderSource
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

        #region Methods

        /// <summary>
        /// Adds or update a new artefact
        /// </summary>
        public void AddOrUpdate(BlackboardTemplateDefinition blackboardDefinition)
        {
            base.SafeAddOrReplace(blackboardDefinition.Uid, blackboardDefinition);
        }

        #endregion
    }
}
