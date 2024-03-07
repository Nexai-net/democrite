// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    /// <summary>
    /// <see cref="BlackboardTemplateDefinition"/> build first step
    /// To force the controller to be setup
    /// </summary>
    public interface IBlackboardTemplateBuilder
    {
        /// <summary>
        /// Uses the default controllers.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder UseDefaultControllers(BlackboardControllerTypeEnum controllerTypes = BlackboardControllerTypeEnum.Storage);

        /// <summary>
        /// Setups controllers.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder SetupControllers(Action<IBlackboardTemplateControllerBuilder> builders);
    }
}
