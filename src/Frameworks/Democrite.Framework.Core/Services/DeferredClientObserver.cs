// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Services
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Deferred;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Core.Abstractions.Services;
    using Democrite.Framework.Core.Models;

    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Models;
    using Elvex.Toolbox.Services;
    using Elvex.Toolbox.Supports;
    using Elvex.Toolbox.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Newtonsoft.Json.Linq;

    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Client observer and deferred awaiter provider used to on client side await deferred work
    /// </summary>
    /// <seealso cref="IDeferredAwaiterHandler" />
    /// <seealso cref="IDeferredObserver" />
    internal sealed class DeferredClientObserver : SafeAsyncDisposable, IDeferredAwaiterHandler, IDeferredObserver, IInitService, IFinalizeService
    {
        #region Fields

        private readonly SupportInitializationImplementation<IServiceProvider> _initializer;
        private readonly SupportInitializationImplementation<IServiceProvider> _finalizer;
        private readonly DelayTimer _keepSubscriptionAliveDelayTimer;

        private readonly Dictionary<Guid, DeferredStatusMessage> _deferredStatus;
        private readonly Dictionary<Guid, ITaskCompletionSourceEx> _awaitingTasks;

        private readonly IVGrainDemocriteSystemProvider _grainDemocriteSystemProvider;
        private readonly IGrainOrleanFactory _grainOrleanFactory;
        private readonly ILogger<IDeferredObserver> _logger;

        private static readonly MethodInfo s_fetchGenericTaskResponse;

        private readonly SemaphoreSlim _subscriptionLocker;
        private readonly SemaphoreSlim _deferredLocker;

        private IDeferredObserver? _objRefObserver;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DeferredClientObserver"/> class.
        /// </summary>
        static DeferredClientObserver()
        {
            s_fetchGenericTaskResponse = ((MethodCallExpression)((LambdaExpression)((DeferredClientObserver c) => c.FetchTaskResponseAsync<int>(default, null))).Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredClientObserver"/> class.
        /// </summary>
        public DeferredClientObserver(ILogger<IDeferredObserver> logger,
                                      IGrainOrleanFactory grainOrleanFactory,
                                      IVGrainDemocriteSystemProvider grainDemocriteSystemProvider)
        {
            this._initializer = new SupportInitializationImplementation<IServiceProvider>(InitObserver);
            this._finalizer = new SupportInitializationImplementation<IServiceProvider>(FinalizeObserver);

            this._logger = logger ?? NullLogger<IDeferredObserver>.Instance;

            this._deferredStatus = new Dictionary<Guid, DeferredStatusMessage>();
            this._awaitingTasks = new Dictionary<Guid, ITaskCompletionSourceEx>();

            this._deferredLocker = new SemaphoreSlim(1);
            this._subscriptionLocker = new SemaphoreSlim(1);

            this._grainDemocriteSystemProvider = grainDemocriteSystemProvider;
            this._grainOrleanFactory = grainOrleanFactory;

            var refreshTimer = TimeSpan.FromMicroseconds(DemocriteConstants.DefaultObserverTimeout.TotalMicroseconds * 0.95);
            this._keepSubscriptionAliveDelayTimer = DelayTimer.Create(UpdateSubscription,
                                                                      tickDelay: refreshTimer,
                                                                      startDelay: refreshTimer,
                                                                      waitExecutionEnd: true);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool ExpectOrleanStarted
        {
            get { return true; }
        }

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return this._initializer.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return this._initializer.IsInitialized; }
        }

        /// <inheritdoc />
        public bool IsFinalizing
        {
            get { return this._finalizer.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsFinalized
        {
            get { return this._finalizer.IsInitialized; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task DeferredStatusChangedNotification(DeferredStatusMessage statusChangeMessage)
        {
            var deferredId = statusChangeMessage.DeferredId.Uid;
            ITaskCompletionSourceEx? requiredResponse = null;

            using (await this._deferredLocker.LockAsync())
            {
                this._deferredStatus[deferredId] = statusChangeMessage;

                if (this._awaitingTasks.TryGetValue(deferredId, out var pendingTask))
                {
                    if (statusChangeMessage.Status == DeferredStatusEnum.Cancelled || statusChangeMessage.Status == DeferredStatusEnum.Failed || statusChangeMessage.Status == DeferredStatusEnum.Finished)
                    {
                        requiredResponse = pendingTask;
                    }
                    else if (statusChangeMessage.Status == DeferredStatusEnum.Cleanup)
                    {
                        this._awaitingTasks.Remove(deferredId);
                        this._deferredStatus.Remove(deferredId);
                        return;
                    }
                }
            }

            if (requiredResponse is not null)
            {
                s_fetchGenericTaskResponse.MakeGenericMethod((Type)requiredResponse.State!).Invoke(this, new object?[] { statusChangeMessage, requiredResponse });
            }
        }

        /// <inheritdoc />
        public Task<IExecutionResult<TResponse>> GetDeferredWorkAwaiterAsync<TResponse>(DeferredId id, CancellationToken token)
        {
            return GetDeferredWorkAwaiterAsync<TResponse>(id.Uid, token);
        }

        /// <inheritdoc />
        public async Task<IExecutionResult<TResponse>> GetDeferredWorkAwaiterAsync<TResponse>(Guid id, CancellationToken token)
        {
            var task = await ReservedDeferredWorkSlotImpl<TResponse>(id, token);
            return await task;
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(IServiceProvider? initializationState, CancellationToken token = default)
        {
            return this._initializer.InitializationAsync(initializationState, token);
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return this._initializer.InitializationAsync(token);
        }

        /// <inheritdoc />
        public ValueTask FinalizeAsync(CancellationToken token = default)
        {
            return this._finalizer.InitializationAsync(token);
        }

        /// <inheritdoc />
        public async Task<Guid> ReservedDeferredWorkSlot<TResponse>(Guid? sourceId = null)
        {
            var handler = await this._grainDemocriteSystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this._logger);
            var workId = await handler.CreateDeferredWorkAsync(sourceId, null, (ConcretBaseType)typeof(TResponse).GetAbstractType());

            await ReservedDeferredWorkSlotImpl<TResponse>(workId);
            return workId;
        }

        /// <inheritdoc />
        public async Task CleanUpDeferredWork(Guid deferredId)
        {
            var handler = await this._grainDemocriteSystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this._logger);
            await handler.CleanUpDeferredWork(deferredId);
        }

        #region Tools

        /// <inheritdoc />
        public async Task<Task<IExecutionResult<TResponse>>> ReservedDeferredWorkSlotImpl<TResponse>(Guid id, CancellationToken? token = null)
        {
            Task<IExecutionResult<TResponse>>? task = null;

            using (await this._deferredLocker.LockAsync())
            {
                token?.ThrowIfCancellationRequested();
                if (this._awaitingTasks.TryGetValue(id, out var pendingTask))
                {
                    task = (Task<IExecutionResult<TResponse>>)pendingTask.GetTask();
                }
                else
                {
                    if (this._deferredStatus.TryGetValue(id, out var deferredStatusMessage))
                    {
                        if (deferredStatusMessage.Status == DeferredStatusEnum.Cancelled)
                            throw new OperationCanceledException();

                        // TODO : managed failed status
                    }

                    var taskCompletion = new TaskCompletionSourceEx<IExecutionResult<TResponse>>(typeof(TResponse));
                    task = taskCompletion.Task;
                    this._awaitingTasks.Add(id, taskCompletion);
                }
            }

            token?.Register(uid =>
            {
                var deferredTaskUid = (Guid)uid!;

                this._deferredLocker.Lock();
                try
                {
                    if (this._awaitingTasks.TryGetValue(deferredTaskUid, out var pendingTask))
                    {
                        this._awaitingTasks.Remove(deferredTaskUid);
                        pendingTask.TrySetCanceled();
                    }
                }
                finally
                {
                    this._deferredLocker.Release();
                }
            }, id, true);

            return task;
        }

        /// <summary>
        /// Fetches pending <see cref="ITaskCompletionSourceEx"/> response
        /// </summary>
        private async Task FetchTaskResponseAsync<TExpectedResponse>(DeferredStatusMessage statusChangeMessage, ITaskCompletionSourceEx requiredResponse)
        {
            if (requiredResponse.GetTask().IsCompleted)
                return;

            var deferredId = statusChangeMessage.DeferredId.Uid;

            var handler = await this._grainDemocriteSystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this._logger);

            TExpectedResponse? result = default;
            DemocriteInternalException? internalException = null;

            if (statusChangeMessage.Status == DeferredStatusEnum.Failed)
            {
                internalException = await handler.ConsumeDeferredResponseAsync<DemocriteInternalException>(deferredId);
                internalException ??= new DemocriteInternalException("no exception",
                                                                     null!,
                                                                     DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Execution,
                                                                                               DemocriteErrorCodes.PartType.Deferred,
                                                                                               DemocriteErrorCodes.ErrorType.Failed),
                                                                     null);
            }
            else if (statusChangeMessage.Status == DeferredStatusEnum.Finished)
            {
                result = await handler.ConsumeDeferredResponseAsync<TExpectedResponse>(deferredId);
            }

            var executionResult = new ExecutionResult<TExpectedResponse>(statusChangeMessage.DeferredId.SourceId,
                                                                         internalException is null,
                                                                         statusChangeMessage.Status == DeferredStatusEnum.Cancelled,
                                                                         internalException is not null ? DemocriteErrorCodes.ToDecryptErrorCode(internalException.ErrorCode) : string.Empty,
                                                                         internalException?.GetFullString(),
                                                                         result);

            requiredResponse.TrySetResultObject(executionResult);
        }

        /// <summary>
        /// 
        /// </summary>
        private async ValueTask InitObserver(IServiceProvider? provider, CancellationToken token)
        {
            await UpdateSubscription(token);
            await this._keepSubscriptionAliveDelayTimer.StartAsync(token);
        }

        /// <summary>
        /// 
        /// </summary>
        private async ValueTask FinalizeObserver(IServiceProvider? provider, CancellationToken token)
        {
            var timer = this._keepSubscriptionAliveDelayTimer;

            if (timer is not null && !this.IsDisposed)
                await this._keepSubscriptionAliveDelayTimer.StopAsync(token);
        }

        /// <summary>
        /// Update subscription to ObserverManager that is reset every <see cref="DemocriteConstants.DefaultObserverTimeout"/>
        /// </summary>
        private async Task UpdateSubscription(CancellationToken token)
        {
            await this._subscriptionLocker.WaitAsync(token);

            try
            {
                var handler = await this._grainDemocriteSystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this._logger);

                this._objRefObserver ??= this._grainOrleanFactory.CreateObjectReference<IDeferredObserver>(this);
                await handler.Subscribe(this._objRefObserver);
            }
            finally
            {
                this._subscriptionLocker.Release();
            }
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            if (!this.IsFinalized)
                await FinalizeAsync(default);

            await base.DisposeBeginAsync();
        }

        #endregion

        #endregion
    }
}
