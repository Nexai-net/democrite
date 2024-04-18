// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;

    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;

    /// <summary>
    /// Handler use to control a controller
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class BlackboardControllerHandler : SafeDisposable
    {
        #region Fields

        private readonly BlackboardTemplateDefinition _blackboardTemplate;
        private readonly ControllerBaseOptions? _option;
        private readonly IGrainFactory _grainFactory;
        private readonly Type _controllerType;
        private readonly Guid _boardId;

        private readonly SemaphoreSlim _locker;

        private IBlackboardBaseControllerGrain? _controller;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardControllerHandler{TController}"/> class.
        /// </summary>
        public BlackboardControllerHandler(Guid boardId,
                                           Type controllerType,
                                           IGrainOrleanFactory grainFactory,
                                           ControllerBaseOptions? option,
                                           BlackboardTemplateDefinition blackboardTemplate)
        {
            this._locker = new SemaphoreSlim(1);

            this._option = option;
            this._boardId = boardId;
            this._grainFactory = grainFactory;
            this._controllerType = controllerType;
            this._blackboardTemplate = blackboardTemplate;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<(TController Controller, IReadOnlyCollection<BlackboardCommand>? InitControllerActions)> GetController<TController>(CancellationToken token)
        {
            List<BlackboardCommand>? iniCommands = null;

            using (this._locker.Lock(token))
            using (var grainTokenSource = token.ToGrainCancellationTokenSource())
            {
                if (this._controller == null)
                {
                    var controller = this._grainFactory.GetGrain(this._controllerType, this._boardId).AsReference<IBlackboardBaseControllerGrain>();
                    var initCmdResuls = await controller.InitializationAsync(this._option, grainTokenSource.Token);

                    this._controller = controller;

                    if (initCmdResuls is not null && initCmdResuls.Any())
                        iniCommands = iniCommands.AddRangeOnNull(initCmdResuls);
                }
            }

            return (this._controller.AsReference<TController>(), (IReadOnlyCollection<BlackboardCommand>?)iniCommands);
        }

        #endregion
    }
}