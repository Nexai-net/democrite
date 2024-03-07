// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Patterns.Workers
{
    using Democrite.Framework.Toolbox.Abstractions.Commands;
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Workers;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.VisualBasic;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Task scheduler allowing <see cref="MaximumConcurrencyLevel"/> of processing at same time with some blocking functionality
    /// </summary>
    /// <seealso cref="TaskScheduler" />
    /// <seealso cref="IWorkerTaskScheduler" />
    public sealed class WorkerTaskScheduler : SafeAsyncDisposable, IWorkerTaskScheduler
    {
        #region Fields

        private readonly Queue<LinkedListNode<TaskWorkerContainerAsync>> _containerPool;

        private readonly LinkedList<TaskWorkerContainerAsync> _waitingTasks;
        private readonly CancellationTokenSource _lifeToken;
        private readonly SemaphoreSlim _locker;
        private readonly ILogger _logger;

        private uint _maximumConcurrencyLevel;
        private bool _flushing;

        private long _taskPending;
        private long _taskProcessing;

        private readonly EventWaitHandle _newElementToProcessEventHandler;
        private readonly EventWaitHandle _elementProcessEventHandler;

        private TaskCompletionSource? _flushingTaskCompletionSource;
        private Task? _processLoopTask;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerTaskScheduler"/> class.
        /// </summary>
        public WorkerTaskScheduler(uint initialMaxConcurrecy = 0, Guid? schedulerUid = null, ILogger? logger = null)
        {
            this._lifeToken = new CancellationTokenSource();

            this._logger = logger ?? NullLogger.Instance;

            this._locker = new SemaphoreSlim(1);
            this._newElementToProcessEventHandler = new EventWaitHandle(false, EventResetMode.AutoReset);
            this._elementProcessEventHandler = new EventWaitHandle(false, EventResetMode.AutoReset);

            this._waitingTasks = new LinkedList<TaskWorkerContainerAsync>();

            ChangeMaximumConcurrencyLevel(initialMaxConcurrecy);

            this._containerPool = new Queue<LinkedListNode<TaskWorkerContainerAsync>>(Enumerable.Range(0, (int)this.MaximumConcurrencyLevel + 1).Select(m => new LinkedListNode<TaskWorkerContainerAsync>(new TaskWorkerContainerAsync())));

            this.SchedulerUId = schedulerUid ?? Guid.NewGuid();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public uint MaximumConcurrencyLevel
        {
            get
            {
                this._locker.Wait();
                try
                {
                    return _maximumConcurrencyLevel;
                }
                finally
                {
                    this._locker.Release();
                }
            }
        }

        /// <inheritdoc />
        public Guid SchedulerUId { get; }

        /// <inheritdoc />
        public ulong TaskPending
        {
            get { return (ulong)Interlocked.Read(ref this._taskPending); }
        }

        /// <inheritdoc />
        public ulong TaskProcessing
        {
            get { return (ulong)Interlocked.Read(ref this._taskProcessing); }
        }

        #endregion

        #region Nested

        /// <summary>
        /// Pool item that contain all the relative information to execute a func
        /// </summary>
        private sealed class TaskWorkerContainerAsync
        {
            #region Fields

            private Func<CancellationToken, Task>? _func;
            private TaskCompletionSource? _completionSource;
            private TaskScheduler? _scheduler;
            private CancellationToken? _waitingCancellationToken;

            #endregion

            #region Properties

            public Task? ExecTask { get; private set; }

            #endregion

            #region Methods

            /// <summary>
            /// Initializes the container.
            /// </summary>
            public void Initialize(Func<CancellationToken, Task> func,
                                   TaskCompletionSource? completionSource,
                                   TaskScheduler scheduler,
                                   CancellationToken? waitingCancellationToken)
            {
                this._func = func;
                this._completionSource = completionSource;
                this._scheduler = scheduler;
                this._waitingCancellationToken = waitingCancellationToken;
            }

            /// <summary>
            /// Cleans up.
            /// </summary>
            public void CleanUp()
            {
                this._func = null;
                this._completionSource = null;
                this.ExecTask = null;
                this._waitingCancellationToken = null;
            }

            public Task ExecuteAsync(CancellationToken token)
            {
                if (this.ExecTask == null)
                {
                    this.ExecTask = Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            using (var tokens = CancellationTokenSource.CreateLinkedTokenSource(this._waitingCancellationToken ?? default, token))
                            {
                                await this._func!(tokens.Token);
                            }
                            this._completionSource?.TrySetResult();
                        }
                        catch (OperationCanceledException)
                        {
                            this._completionSource?.TrySetCanceled();
                        }
                        catch (Exception ex)
                        {
                            this._completionSource?.TrySetException(ex);
                        }
                    }, token, Task.Factory.CreationOptions, this._scheduler!).Unwrap();
                }
                return this.ExecTask;
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void ChangeMaximumConcurrencyLevel(uint maximumConcurrencyLevel)
        {
            this._locker.Wait();
            try
            {
                if (maximumConcurrencyLevel < 1)
                    maximumConcurrencyLevel = (uint)(Environment.ProcessorCount / 3.0);

                this._maximumConcurrencyLevel = maximumConcurrencyLevel;
            }
            finally
            {
                this._locker.Release();
            }

            // Unlock the processing loop if more work can be done
            this._newElementToProcessEventHandler.Set();
        }

        /// <inheritdoc />
        public async Task FlushAsync(CancellationToken token)
        {
            Task waitTask = null!;

            this._locker.Wait();
            try
            {
                if (this._processLoopTask is null)
                    return;

                if (this._flushingTaskCompletionSource is null)
                    this._flushingTaskCompletionSource = new TaskCompletionSource();

                this._flushing = true;

                waitTask = this._flushingTaskCompletionSource.Task;
            }
            finally
            {
                this._locker.Release();
                this._newElementToProcessEventHandler.Set();
            }

            await Task.Run(async () => await waitTask);
        }

        /// <inheritdoc />
        public void PushTask(Func<CancellationToken, Task> task, TaskScheduler? taskScheduler = null)
        {
            PushTaskImpl(task, null, false, scheduler: taskScheduler).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public void PushTask<TInput>(Func<TInput?, CancellationToken, Task> func, TInput? input, TaskScheduler? taskScheduler = null)
        {
            PushTaskImpl((token) => func(input, token), null, false, scheduler: taskScheduler).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Enqueues the func to process and return a func to wait the result h.
        /// </summary>
        public void ExecTask(Func<CancellationToken, Task> func, TaskCompletionSource? resultTask, CancellationToken? waitingCancellationToken = default, TaskScheduler? taskScheduler = null)
        {
            PushTaskImpl(func, resultTask, false, scheduler: taskScheduler, waitingCancellationToken).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Enqueues the func to process and return a func to wait the result h.
        /// </summary>
        public void ExecTask<TInput>(Func<TInput?, CancellationToken, Task> func, TInput? input, TaskCompletionSource? resultTask, CancellationToken? waitingCancellationToken = default, TaskScheduler? taskScheduler = null)
        {
            PushTaskImpl((token) => func(input, token), resultTask, false, scheduler: taskScheduler, waitingCancellationToken).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task PushTaskWhenAvailableSlotAsync<TInput>(Func<TInput?, CancellationToken, Task> func, TInput? input, CancellationToken token, TaskScheduler? taskScheduler = null)
        {
            await PushTaskImpl((token) => func(input, token), null, true, scheduler: taskScheduler, token);
        }

        /// <inheritdoc />
        public async Task PushTaskWhenAvailableSlotAsync(Func<CancellationToken, Task> task, CancellationToken token, TaskScheduler? taskScheduler = null)
        {
            await PushTaskImpl(task, null, true, scheduler: taskScheduler, token);
        }

        #region Tools

        private async Task PushTaskImpl(Func<CancellationToken, Task> execTask,
                                        TaskCompletionSource? completionSource,
                                        bool waitSlotIsAvailable,
                                        TaskScheduler? scheduler,
                                        CancellationToken? waitingCancellationToken = default)
        {
            var solvedScheduler = scheduler ?? TaskScheduler.Current;
            if (waitSlotIsAvailable)
            {
                using (var token = CancellationTokenSource.CreateLinkedTokenSource(waitingCancellationToken ?? default, this._lifeToken.Token))
                {
                    while (this._lifeToken.IsCancellationRequested == false)
                    {
                        var added = false;

                        await this._locker.WaitAsync(token.Token);
                        try
                        {
                            if (this.TaskPending < this._maximumConcurrencyLevel)
                            {
                                ThreadSafePushTaskImpl(execTask, completionSource, solvedScheduler, waitingCancellationToken);
                                added = true;
                            }

                            token.Token.ThrowIfCancellationRequested();
                        }
                        finally
                        {
                            this._locker.Release();

                            if (added)
                                this._newElementToProcessEventHandler.Set();
                        }

                        if (added)
                            break;

                        this._elementProcessEventHandler.WaitOne(10000);
                        token.Token.ThrowIfCancellationRequested();
                    }
                }
                return;
            }

            await this._locker.WaitAsync(this._lifeToken.Token);
            try
            {
                ThreadSafePushTaskImpl(execTask, completionSource, solvedScheduler, waitingCancellationToken);
            }
            finally
            {
                this._locker.Release();
                this._newElementToProcessEventHandler.Set();
            }
        }

        private void ThreadSafePushTaskImpl(Func<CancellationToken, Task> execTask,
                                            TaskCompletionSource? completionSource,
                                            TaskScheduler scheduler,
                                            CancellationToken? waitingCancellationToken)
        {
            if (this._flushing)
                throw new InvalidOperationException("Worker in flushing process can't add tasks");

            LinkedListNode<TaskWorkerContainerAsync> taskWorker;

            if (this._containerPool.Count > 0)
                taskWorker = this._containerPool.Dequeue();
            else
                taskWorker = new LinkedListNode<TaskWorkerContainerAsync>(new TaskWorkerContainerAsync());

            taskWorker.ValueRef.Initialize(execTask, completionSource, scheduler, waitingCancellationToken);

            if (this._processLoopTask is null)
                this._processLoopTask = Task.Run(ProcessLoopTaskAsync);

            this._waitingTasks.AddLast(taskWorker);
            Interlocked.Exchange(ref this._taskPending, this._waitingTasks.Count);
        }

        /// <summary>
        /// Loop in charge to process the tasks
        /// </summary>
        private async Task ProcessLoopTaskAsync()
        {
            var workToDo = new List<LinkedListNode<TaskWorkerContainerAsync>>();
            this._newElementToProcessEventHandler.WaitOne(1000);

            try
            {
                // Build processing group from _waitingTasks
                while (this._lifeToken.IsCancellationRequested == false)
                {
                    var guid = Guid.NewGuid();
                    await ThreadSafeBuildRemainWorkTodo(workToDo);

                    if (workToDo.Count == 0)
                    {
                        this._newElementToProcessEventHandler.WaitOne(1000);
                        continue;
                    }

                    try
                    {
                        await workToDo.Select(w => w.ValueRef.ExecuteAsync(this._lifeToken.Token))
                                      .ToArray()
                                      .SafeWhenAllAsync(this._lifeToken.Token);
                    }
                    catch
                    {
                    }

                    await CleanAndRecycle(workToDo);

                    this._elementProcessEventHandler.Set();
                }
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Error, "{exception}", ex);
            }
            finally
            {
                await ThreadSafeCleanUpAfterExit(workToDo);
            }
        }

        private async Task CleanAndRecycle(List<LinkedListNode<TaskWorkerContainerAsync>> workToDo)
        {
            TaskCompletionSource? flusingTaskCompletionSource = null;
            await this._locker.WaitAsync(this._lifeToken.Token);
            try
            {

                for (int i = 0; i < workToDo.Count; i++)
                {
                    var worker = workToDo[i];

                    var task = worker.ValueRef.ExecTask;
                    if (task is null || !task.IsCompleted)
                        continue;

                    // recycle
                    workToDo.RemoveAt(i);
                    i--;

                    worker.ValueRef.CleanUp();
                    this._containerPool.Enqueue(worker);
                }

                flusingTaskCompletionSource = ManagedFlushingContext(workToDo.Count);
            }
            catch
            {

            }
            finally
            {
                this._locker.Release();
                flusingTaskCompletionSource?.TrySetResult();
            }
        }

        /// <summary>
        /// Threads the safe clean up after exit the remain func
        /// </summary>
        private async Task ThreadSafeCleanUpAfterExit(List<LinkedListNode<TaskWorkerContainerAsync>> processing)
        {
            IReadOnlyCollection<LinkedListNode<TaskWorkerContainerAsync>> nodes;

            TaskCompletionSource? flusingTaskCompletionSource = null;
            await this._locker.WaitAsync();
            try
            {
                var remainTasks = processing.ToArray();
                processing.Clear();

                var pendingTasks = this._waitingTasks.Nodes().ToArray();
                this._waitingTasks.Clear();

                nodes = remainTasks.Concat(pendingTasks).ToArray();

                this._processLoopTask = null;

                flusingTaskCompletionSource = ManagedFlushingContext(0);
            }
            finally
            {
                this._locker.Release();
                flusingTaskCompletionSource?.TrySetResult();
            }

            foreach (var node in nodes)
            {
                node.ValueRef.CleanUp();
                this._containerPool.Enqueue(node);
            }
        }

        private async Task ThreadSafeBuildRemainWorkTodo(List<LinkedListNode<TaskWorkerContainerAsync>> workToDo)
        {
            TaskCompletionSource? flusingTaskCompletionSource = null;

            await this._locker.WaitAsync(this._lifeToken.Token);
            try
            {
                var missing = this._maximumConcurrencyLevel - workToDo.Count;
                if (missing > 0)
                {
                    for (var i = missing; i > 0; i--)
                    {
                        var remain = this._waitingTasks.First;

                        if (remain == null)
                            continue;

                        workToDo.Add(remain);

                        this._waitingTasks.RemoveFirst();

                        Debug.Assert(remain.Next is null);
                        Debug.Assert(remain.Previous is null);
                        Debug.Assert(remain.Value is not null);
                    }
                }

                Interlocked.Exchange(ref this._taskProcessing, workToDo.Count);
                Interlocked.Exchange(ref this._taskPending, this._waitingTasks.Count);

                flusingTaskCompletionSource = ManagedFlushingContext(workToDo.Count);
            }
            finally
            {
                this._locker.Release();
                flusingTaskCompletionSource?.TrySetResult();
            }
        }

        private TaskCompletionSource? ManagedFlushingContext(int processinWorkRemains)
        {
            TaskCompletionSource? flusingTaskCompletionSource = null;
            if (this._flushingTaskCompletionSource is not null && processinWorkRemains == 0 && this._waitingTasks.Count == 0)
            {
                flusingTaskCompletionSource = this._flushingTaskCompletionSource;
                this._flushingTaskCompletionSource = null;
                this._flushing = false;
            }

            return flusingTaskCompletionSource;
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            this._newElementToProcessEventHandler.Set();
            this._lifeToken.Cancel();

            using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(10)))
            {
                try
                {
                    await FlushAsync(timeout.Content);
                }
                catch
                {

                }
            }
            await base.DisposeBeginAsync();
        }

        /// <inheritdoc />
        protected override ValueTask DisposeEndAsync()
        {
            this._lifeToken.Dispose();
            this._locker.Dispose();
            this._newElementToProcessEventHandler.Dispose();
            this._containerPool.Clear();
            return base.DisposeEndAsync();
        }

        #endregion

        #endregion
    }
}
