// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders
{
    using Democrite.Framework.Node.Blackboard.Builders.Templates;

    /// <summary>
    /// Entry point to build <see cref="BlackboardTemplateDefinition"/>
    /// </summary>
    public static class BlackboardTemplate
    {
        #region Methods

        /// <summary>
        /// Builds the specified name identifier.
        /// </summary>
        /// <param name="nameIdentifier">A Blackboard template could be identify by is <see cref="BlackboardTemplateDefinition.Uid"/> by also by his <see cref="BlackboardTemplateDefinition.NameIdentifier"/></param>
        public static IBlackboardTemplateBuilder Build(string nameIdentifier, Guid? fixUid = null)
        {
            return new BlackboardTemplateBuilder(nameIdentifier, fixUid);
        }

        #endregion
    }
}