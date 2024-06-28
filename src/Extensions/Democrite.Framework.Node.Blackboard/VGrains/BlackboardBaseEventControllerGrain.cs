// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.VGrains
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions.Signals;
    using Democrite.Framework.Node.Blackboard.Abstractions;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Commands;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Events;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models.Queries;
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
    public abstract class BlackboardBaseEventControllerGrain<TController> : BlackboardBaseControllerGrain<TController>, IBlackboardEventControllerGrain
        where TController : IBlackboardEventControllerGrain
    {
        #region Fields

        private static readonly IReadOnlyCollection<BlackboardCommand> s_defaultRejectRequest = BlackboardCommandRejectAction.Default.AsEnumerable().ToArray();
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardBaseControllerGrain_tmpl{TController}"/> class.
        /// </summary>
        protected BlackboardBaseEventControllerGrain(ILogger<TController> logger,
                                                     IBlackboardProvider blackboardProvider) 
            : base(logger, blackboardProvider)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ProcessRequestAsync<TResponseRequested>(BlackboardBaseQuery request, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnProcessRequestAsync<TResponseRequested>(request, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnProcessRequestAsync<TResponseRequested>(BlackboardBaseQuery request, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(s_defaultRejectRequest);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ReactToEventsAsync(BlackboardEventBook eventBook, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnReactToEventsAsync(eventBook, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnReactToEventsAsync(BlackboardEventBook eventBook, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ManagedSignalMessageAsync(SignalMessage message, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnManagedSignalMessageAsync(message, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnManagedSignalMessageAsync(SignalMessage message, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }

        #endregion
    }

    /// <summary>
    /// Base class of controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller.</typeparam>
    /// <seealso cref="VGrainBase{TController}" />
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public abstract class BlackboardBaseEventControllerGrain<TState, TController> : BlackboardBaseControllerGrain<TState, TController>, IBlackboardEventControllerGrain
        where TController : IBlackboardEventControllerGrain
        where TState : class
    {
        #region Fields

        private static readonly IReadOnlyCollection<BlackboardCommand> s_defaultRejectRequest = BlackboardCommandRejectAction.Default.AsEnumerable().ToArray();
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardBaseControllerGrain_tmpl{TController}"/> class.
        /// </summary>
        protected BlackboardBaseEventControllerGrain(ILogger<TController> logger,
                                                     IBlackboardProvider blackboardProvider,
                                                     IPersistentState<TState> persistentState) 
            : base(logger, blackboardProvider, persistentState)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ProcessRequestAsync<TResponseRequested>(BlackboardBaseQuery request, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnProcessRequestAsync<TResponseRequested>(request, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnProcessRequestAsync<TResponseRequested>(BlackboardBaseQuery request, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(s_defaultRejectRequest);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ReactToEventsAsync(BlackboardEventBook eventBook, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnReactToEventsAsync(eventBook, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnReactToEventsAsync(BlackboardEventBook eventBook, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ManagedSignalMessageAsync(SignalMessage message, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnManagedSignalMessageAsync(message, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnManagedSignalMessageAsync(SignalMessage message, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }

        #endregion
    }

    /// <summary>
    /// Base class of controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller.</typeparam>
    /// <seealso cref="VGrainBase{TController}" />
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public abstract class BlackboardBaseEventControllerGrain<TState, TSurrogate, TConverter, TController> : BlackboardBaseControllerGrain<TState, TSurrogate, TConverter, TController>, IBlackboardEventControllerGrain
        where TController : IBlackboardEventControllerGrain
        where TState : class
        where TSurrogate : struct
        where TConverter : class, IConverter<TState, TSurrogate>
    {
        #region Fields

        private static readonly IReadOnlyCollection<BlackboardCommand> s_defaultRejectRequest = BlackboardCommandRejectAction.Default.AsEnumerable().ToArray();
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardBaseControllerGrain_tmpl{TController}"/> class.
        /// </summary>
        protected BlackboardBaseEventControllerGrain(ILogger<TController> logger,
                                                     IBlackboardProvider blackboardProvider,
                                                     IPersistentState<TSurrogate> persistentState) 
            : base(logger, blackboardProvider, persistentState)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ProcessRequestAsync<TResponseRequested>(BlackboardBaseQuery request, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnProcessRequestAsync<TResponseRequested>(request, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnProcessRequestAsync<TResponseRequested>(BlackboardBaseQuery request, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(s_defaultRejectRequest);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ReactToEventsAsync(BlackboardEventBook eventBook, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnReactToEventsAsync(eventBook, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnReactToEventsAsync(BlackboardEventBook eventBook, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<BlackboardCommand>?> ManagedSignalMessageAsync(SignalMessage message, GrainCancellationToken token)
        {
            await InitblackboardAccessAsync(token.CancellationToken);
            return await OnManagedSignalMessageAsync(message, token);
        }

        /// <inheritdoc />
        protected virtual Task<IReadOnlyCollection<BlackboardCommand>?> OnManagedSignalMessageAsync(SignalMessage message, GrainCancellationToken token)
        {
            return Task.FromResult<IReadOnlyCollection<BlackboardCommand>?>(null);
        }

        #endregion
    }
}