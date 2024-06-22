// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions.Surrogates;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;

    using Elvex.Toolbox.Models;

    using System;
    using System.Reflection;

    /// <summary>
    /// Generic controller builder
    /// </summary>
    public abstract class BlackboardTemplateBaseControllerBuilder<TSpecializedController, TBaseOption, TDefaultOption> : IBlackboardTemplateSpecializedControllerBuilder<TSpecializedController, TBaseOption, TDefaultOption>,
                                                                                                                         IDefinitionBaseBuilder<BlackboardTemplateControllerDefinition>
        where TSpecializedController : IBlackboardBaseControllerGrain
        where TBaseOption : IControllerOptions
        where TDefaultOption : ControllerBaseOptions, TBaseOption
    {
        #region Fields

        //private static readonly ConcretType s_defaultControllerConcretType;
        //private static readonly Type s_defaultController;

        private readonly IBlackboardTemplateControllerBuilder _root;
        private readonly BlackboardControllerTypeEnum _type;

        private ConcretType? _controller;
        private ControllerBaseOptions? _option;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateBaseControllerBuilder{TSpecializedController, TBaseOption, TDefaultOption}"/> class.
        /// </summary>
        public BlackboardTemplateBaseControllerBuilder(IBlackboardTemplateControllerBuilder root,
                                                       BlackboardControllerTypeEnum type)
        {
            this._type = type;
            this._root = root;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IBlackboardTemplateControllerBuilder UseController<TStorageController>() where TStorageController : TSpecializedController
        {
            this._controller = (ConcretType)typeof(TStorageController).GetAbstractType();
            return this._root;
        }

        /// <inheritdoc />
        public IBlackboardTemplateControllerBuilder UseController<TStorageController, TControllerOption>(TControllerOption option)
            where TStorageController : IBlackboardBaseControllerGrain<TControllerOption>, TSpecializedController
            where TControllerOption : ControllerBaseOptions, TBaseOption
        {
            this._controller = (ConcretType)typeof(TStorageController).GetAbstractType();
            this._option = option;
            return this._root;
        }

        /// <inheritdoc />
        public virtual BlackboardTemplateControllerDefinition Build()
        {
            ArgumentNullException.ThrowIfNull(this._controller);

            return new BlackboardTemplateControllerDefinition(Guid.NewGuid(),
                                                              this._type,
                                                              this._controller,
                                                              this._option ?? BuildDefaultOption(),
                                                              null);
        }

        #region Tools

        /// <summary>
        /// Builds the default option
        /// </summary>
        protected virtual ControllerBaseOptions BuildDefaultOption()
        {
            return Activator.CreateInstance<TDefaultOption>();
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Builder dedicated to storage controller
    /// </summary>
    public sealed class BlackboardTemplateStorageControllerBuilder : BlackboardTemplateBaseControllerBuilder<IBlackboardStorageControllerGrain, IControllerStorageOptions, DefaultControllerOptions>, IBlackboardTemplateStorageControllerBuilder, IDefinitionBaseBuilder<BlackboardTemplateControllerDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateStorageControllerBuilder"/> class.
        /// </summary>
        public BlackboardTemplateStorageControllerBuilder(IBlackboardTemplateControllerBuilder root)
            : base(root, BlackboardControllerTypeEnum.Storage)
        {
        }

        /// <inheritdoc />
        public IBlackboardTemplateControllerBuilder UseDefault(DefaultControllerOptions? options = null)
        {
            return UseController<IDefaultBlackboardControllerGrain, DefaultControllerOptions>(options ?? DefaultControllerOptions.Default);
        }
    }

    /// <summary>
    /// Builder dedicated to event controller
    /// </summary>
    public sealed class BlackboardTemplateEventControllerBuilder : BlackboardTemplateBaseControllerBuilder<IBlackboardEventControllerGrain, IControllerEventOptions, EventControllerOptions>, IBlackboardTemplateEventControllerBuilder, IDefinitionBaseBuilder<BlackboardTemplateControllerDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateEventControllerBuilder"/> class.
        /// </summary>
        public BlackboardTemplateEventControllerBuilder(IBlackboardTemplateControllerBuilder root)
            : base(root, BlackboardControllerTypeEnum.Event)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override ControllerBaseOptions BuildDefaultOption()
        {
            return EventControllerOptions.Create(b => { });
        }

        #endregion
    }

    /// <summary>
    /// Builder dedicated to state controller
    /// </summary>
    public sealed class BlackboardTemplateStateControllerBuilder : BlackboardTemplateBaseControllerBuilder<IBlackboardStateControllerGrain, IControllerStateOptions, DefaultControllerOptions>, IBlackboardTemplateStateControllerBuilder, IDefinitionBaseBuilder<BlackboardTemplateControllerDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateStateControllerBuilder"/> class.
        /// </summary>
        public BlackboardTemplateStateControllerBuilder(IBlackboardTemplateControllerBuilder root)
            : base(root, BlackboardControllerTypeEnum.State)
        {
        }
    }

    /// <summary>
    /// Generic controller builder used to setup a controller used to multiple types
    /// </summary>
    public sealed class BlackboardTemplateGenericControllerBuilder : BlackboardTemplateBaseControllerBuilder<IBlackboardBaseControllerGrain, IControllerOptions, DefaultControllerOptions>, IBlackboardTemplateGenericControllerBuilder, IDefinitionBaseBuilder<BlackboardTemplateControllerDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardTemplateGenericControllerBuilder"/> class.
        /// </summary>
        public BlackboardTemplateGenericControllerBuilder(IBlackboardTemplateControllerBuilder root, BlackboardControllerTypeEnum types)
            : base(root, types)
        {
        }

        #endregion
    }
}
