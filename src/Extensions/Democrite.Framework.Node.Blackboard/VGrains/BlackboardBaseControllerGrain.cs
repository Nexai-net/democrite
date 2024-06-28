﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// Base class of controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller.</typeparam>
    /// <seealso cref="VGrainBase{TController}" />
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public abstract class BlackboardBaseControllerGrain<TController> : VGrainBase<TController>, IBlackboardBaseControllerGrain
        where TController : IBlackboardBaseControllerGrain
    {
        #region Fields
        
        private readonly IBlackboardProvider _blackboardProvider;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardBaseControllerGrain_tmpl{TController}"/> class.
        /// </summary>
        protected BlackboardBaseControllerGrain(ILogger<TController> logger,
                                                IBlackboardProvider blackboardProvider) 
            : base(logger)
        {
            this._blackboardProvider = blackboardProvider;

            // Before any use the grain WILL be activated where the origin blackboard is retreive
            this.Blackboard = null!;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the blackboard.
        /// </summary>
        protected IBlackboardRef Blackboard { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> InitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            await InitblackboardAccessAsync(cancellationToken.CancellationToken);
            return await OnInitializationAsync(option, cancellationToken);
        }

        #region Tools

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnInitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }
        
        /// <summary>
        /// Initialize blackboards the access.
        /// </summary>
        protected async Task InitblackboardAccessAsync(CancellationToken token)
        {
            if (this.Blackboard is null)
            {
                var bbuid = base.GetGrainId().GetGuidKey();
                this.Blackboard = await this._blackboardProvider.GetBlackboardAsync(bbuid, token);
            }
        }

        #endregion
    
        #endregion
    }

    /// <summary>
    /// Base class of controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller.</typeparam>
    /// <seealso cref="VGrainBase{TController}" />
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public abstract class BlackboardBaseControllerGrain<TState, TController> : VGrainBase<TState, TController>, IBlackboardBaseControllerGrain
        where TController : IBlackboardBaseControllerGrain
        where TState : class
    {
        #region Fields
        
        private readonly IBlackboardProvider _blackboardProvider;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardBaseControllerGrain_tmpl{TController}"/> class.
        /// </summary>
        protected BlackboardBaseControllerGrain(ILogger<TController> logger,
                                                IBlackboardProvider blackboardProvider,
                                                IPersistentState<TState> persistentState) 
            : base(logger, persistentState)
        {
            this._blackboardProvider = blackboardProvider;

            // Before any use the grain WILL be activated where the origin blackboard is retreive
            this.Blackboard = null!;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the blackboard.
        /// </summary>
        protected IBlackboardRef Blackboard { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> InitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            await InitblackboardAccessAsync(cancellationToken.CancellationToken);
            return await OnInitializationAsync(option, cancellationToken);
        }

        #region Tools

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnInitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }
        
        /// <summary>
        /// Initialize blackboards the access.
        /// </summary>
        protected async Task InitblackboardAccessAsync(CancellationToken token)
        {
            if (this.Blackboard is null)
            {
                var bbuid = base.GetGrainId().GetGuidKey();
                this.Blackboard = await this._blackboardProvider.GetBlackboardAsync(bbuid, token);
            }
        }

        #endregion
    
        #endregion
    }

    /// <summary>
    /// Base class of controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller.</typeparam>
    /// <seealso cref="VGrainBase{TController}" />
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public abstract class BlackboardBaseControllerGrain<TState, TSurrogate, TConverter, TController> : VGrainBase<TState, TSurrogate, TConverter, TController>, IBlackboardBaseControllerGrain
        where TController : IBlackboardBaseControllerGrain
        where TState : class
        where TSurrogate : struct
        where TConverter : class, IConverter<TState, TSurrogate>
    {
        #region Fields
        
        private readonly IBlackboardProvider _blackboardProvider;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardBaseControllerGrain_tmpl{TController}"/> class.
        /// </summary>
        protected BlackboardBaseControllerGrain(ILogger<TController> logger,
                                                IBlackboardProvider blackboardProvider,
                                                IPersistentState<TSurrogate> persistentState) 
            : base(logger, persistentState)
        {
            this._blackboardProvider = blackboardProvider;

            // Before any use the grain WILL be activated where the origin blackboard is retreive
            this.Blackboard = null!;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the blackboard.
        /// </summary>
        protected IBlackboardRef Blackboard { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> InitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            await InitblackboardAccessAsync(cancellationToken.CancellationToken);
            return await OnInitializationAsync(option, cancellationToken);
        }

        #region Tools

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnInitializationAsync(ControllerBaseOptions? option, GrainCancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }
        
        /// <summary>
        /// Initialize blackboards the access.
        /// </summary>
        protected async Task InitblackboardAccessAsync(CancellationToken token)
        {
            if (this.Blackboard is null)
            {
                var bbuid = base.GetGrainId().GetGuidKey();
                this.Blackboard = await this._blackboardProvider.GetBlackboardAsync(bbuid, token);
            }
        }

        #endregion
    
        #endregion
    }
}