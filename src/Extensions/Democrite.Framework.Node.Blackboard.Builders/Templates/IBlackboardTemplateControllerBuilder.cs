// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;

    /// <summary>
    /// Setup all template controllers
    /// </summary>
    /// <remarks>
    ///     All controller not specified will be replaced by the default democrite ones
    /// </remarks>
    public interface IBlackboardTemplateControllerBuilder
    {
        /// <summary>
        /// Gets the storage configurator.
        /// </summary>
        IBlackboardTemplateStorageControllerBuilder Storage { get; }

        /// <summary>
        /// Gets the Event configurator.
        /// </summary>
        IBlackboardTemplateEventControllerBuilder Event { get; }

        /// <summary>
        /// Gets the state controller configurator.
        /// </summary>
        IBlackboardTemplateStateControllerBuilder State { get; }

        /// <summary>
        /// Specify a controller grain in charge multiple controller type
        /// </summary>
        IBlackboardTemplateControllerBuilder Multi(BlackboardControllerTypeEnum types, Action<IBlackboardTemplateGenericControllerBuilder> builder);
    }

    /// <summary>
    /// Specialised base controller builder
    /// </summary>
    public interface IBlackboardTemplateSpecializedControllerBuilder<TSpecializedController, TBaseOption, TDefaultOption>
        where TSpecializedController : IBlackboardBaseControllerGrain
        where TBaseOption : IControllerOptions
        where TDefaultOption : ControllerBaseOptions, TBaseOption
    {
        /// <summary>
        /// Configurations a storage controller.
        /// </summary>
        IBlackboardTemplateControllerBuilder UseController<TController>()
            where TController : TSpecializedController;

        IBlackboardTemplateControllerBuilder UseController<TController, TControllerOption>(TControllerOption option)
            where TController : TSpecializedController, IBlackboardBaseControllerGrain<TControllerOption>
            where TControllerOption : ControllerBaseOptions, TBaseOption;
    }

    /// <summary>
    /// Builder specialized in storage controller
    /// </summary>
    public interface IBlackboardTemplateStorageControllerBuilder : IBlackboardTemplateSpecializedControllerBuilder<IBlackboardStorageControllerGrain, IControllerStorageOptions, DefaultControllerOptions>
    {
        /// <summary>
        /// Configurations the default storage controller.
        /// </summary>
        IBlackboardTemplateControllerBuilder UseDefault(DefaultControllerOptions? options = null);
    }

    /// <summary>
    /// Builder specialized in Event controller
    /// </summary>
    public interface IBlackboardTemplateEventControllerBuilder : IBlackboardTemplateSpecializedControllerBuilder<IBlackboardEventControllerGrain, IControllerEventOptions, EventControllerOptions>
    {
    }

    /// <summary>
    /// Builder specialized in Event controller
    /// </summary>
    public interface IBlackboardTemplateStateControllerBuilder : IBlackboardTemplateSpecializedControllerBuilder<IBlackboardStateControllerGrain, IControllerStateOptions, DefaultControllerOptions>
    {
    }

    /// <summary>
    /// Builder specialized in Generic controller
    /// </summary>
    public interface IBlackboardTemplateGenericControllerBuilder : IBlackboardTemplateSpecializedControllerBuilder<IBlackboardBaseControllerGrain, IControllerOptions, DefaultControllerOptions>
    {
    }
}
