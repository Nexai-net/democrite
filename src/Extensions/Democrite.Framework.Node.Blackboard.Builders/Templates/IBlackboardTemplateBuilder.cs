// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    /// <summary>
    /// <see cref="BlackboardTemplateDefinition"/> build first step
    /// To force the controller to be setup
    /// </summary>
    public interface IBlackboardTemplateBuilder
    {
        /// <summary>
        /// Set definition meta data
        /// </summary>
        IBlackboardTemplateBuilder MetaData(Action<IDefinitionMetaDataBuilder> action);

        /// <summary>
        /// Configure blackboard options
        /// </summary>
        IBlackboardTemplateBuilder ConfigureOptions(Action<IBlackboardTemplateOptionsBuilder> optionsBuilder);

        /// <summary>
        /// Setups controllers.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder SetupControllers(Action<IBlackboardTemplateControllerBuilder> builders);
    }
}
