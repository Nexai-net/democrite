// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Services
{
    using Democrite.Framework.Toolbox.Disposables;

    using Newtonsoft.Json.Linq;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Timer using async/await task to perform timer tasks
    /// </summary>
    public abstract class DelayBaseTimer : SafeAsyncDisposable
    {
        #region Fields

        private readonly CancellationToken _lifeTimeToken;
        private readonly bool _waitExecutionEnd;
        private readonly SemaphoreSlim _locker;

        private CancellationTokenSource? _localRunningCancellationTokenSource;
        private TimeSpan? _startDelay;
        private TimeSpan? _tickDelay;

        private Task? _runningTask;
        private long _runningCount;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayBaseTimer"/> class.
        /// </summary>
        protected DelayBaseTimer(CancellationToken lifeTimeToken,
                                 TimeSpan? tickDelay,
                                 TimeSpan? startDelay,
                                 bool waitExecutionEnd)
        {
            this._tickDelay = tickDelay;
            this._startDelay = startDelay;
            this._waitExecutionEnd = waitExecutionEnd;

            this._lifeTimeToken = lifeTimeToken;
            this._locker = new SemaphoreSlim(1);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        public bool IsRunning
        {
            get { return Interlocked.Read(ref this._runningCount) > 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <remarks>
        ///     If the timer is already running, it restart it
        /// </remarks>
        public Task StartAsync(CancellationToken token = default)
        {
            return StartImplAsync(null, false, null, false, token);
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <remarks>
        ///     If the timer is already running, it restart it
        /// </remarks>
        public Task StartTickChangeAsync(TimeSpan? tickDelay, CancellationToken token = default)
        {
            return StartImplAsync(tickDelay, true, null, false, token);
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <remarks>
        ///     If the timer is already running, it restart it
        /// </remarks>
        public Task StartStartChangeAsync(TimeSpan? startDelay, CancellationToken token = default)
        {
            return StartImplAsync(null, false, startDelay, true, token);
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <remarks>
        ///     If the timer is already running, it restart it
        /// </remarks>
        public Task StartAsync(TimeSpan? tickDelay, TimeSpan? startDelay, CancellationToken token = default)
        {
            return StartImplAsync(tickDelay, true, startDelay, true, token);
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public async Task StopAsync(CancellationToken token = default)
        {
            await this._locker.WaitAsync(token);
            try
            {
                await SafeStopAsync(token);
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Ticks the specified token.
        /// </summary>
        protected abstract Task TickAsync(CancellationToken token);

        #region Tools

        /// <summary>
        /// Timers the loop.
        /// </summary>
        private async Task TimerLoop(TimeSpan? tickDelay, TimeSpan? startDelay, CancellationToken token)
        {
            if (Interlocked.Increment(ref this._runningCount) > 1)
                return;

            try
            {
                if (startDelay != null)
                    await Task.Delay(startDelay.Value, token);

                while (!token.IsCancellationRequested)
                {
                    var tickTask = Task.Run(async () => await TickAsync(token));

                    if (this._waitExecutionEnd)
                        await tickTask;

                    if (tickDelay != null)
                    {
                        await Task.Delay(tickDelay.Value, token);
                        continue;
                    }

                    break;
                }

                //await StopAsync(token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Interlocked.Exchange(ref this._runningCount, 0);
            }
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        /// <remarks>
        ///     Assumed if run in thread safe context
        /// </remarks>
        private async Task SafeStopAsync(CancellationToken token)
        {
            this._localRunningCancellationTokenSource?.Cancel();
            this._localRunningCancellationTokenSource = null;

            try
            {
                if (this._runningTask != null)
                    await this._runningTask;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                this._runningTask = null;
            }
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        private async Task StartImplAsync(TimeSpan? tickDelay,
                                          bool changeTickDelay,
                                          TimeSpan? startDelay,
                                          bool changeStartDelay,
                                          CancellationToken token)
        {
            await this._locker.WaitAsync(token);
            try
            {
                if (changeTickDelay)
                    this._tickDelay = tickDelay;

                if (changeStartDelay)
                    this._startDelay = startDelay;

                await SafeStopAsync(token);

                Debug.Assert(this.IsRunning == false);
                Debug.Assert(this._localRunningCancellationTokenSource == null);
                Debug.Assert(this._runningTask == null);

                this._localRunningCancellationTokenSource?.Cancel();
                this._localRunningCancellationTokenSource = new CancellationTokenSource();

                var localTickDelay = this._tickDelay;
                var localStartDelay = this._startDelay;
                var localToken = CancellationTokenSource.CreateLinkedTokenSource(this._localRunningCancellationTokenSource.Token,
                                                                                 this._lifeTimeToken).Token;

                this._runningTask = Task.Run(async () => await TimerLoop(localTickDelay, localStartDelay, localToken));
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            await StopAsync();
        }

        /// <inheritdoc />
        protected override ValueTask DisposeEndAsync()
        {
            this._locker.Dispose();
            return base.DisposeEndAsync();
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Timer using async/await task to perform timer tasks
    /// </summary>
    public class DelayTimer : DelayBaseTimer
    {
        #region Fields

        private readonly Func<CancellationToken, Task> _callback;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayTimer"/> class.
        /// </summary>
        internal DelayTimer(Func<CancellationToken, Task> callback,
                            CancellationToken lifeTimeToken,
                            TimeSpan? tickDelay,
                            TimeSpan? startDelay,
                            bool waitExecutionEnd)
            : base(lifeTimeToken, tickDelay, startDelay, waitExecutionEnd)
        {
            this._callback = callback;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified delay timer with <paramref name="state"/>
        /// </summary>
        public static DelayTimer<TState> Create<TState>(Func<TState?, CancellationToken, Task> callback,
                                                        TState? state,
                                                        CancellationToken lifeTimeToken = default,
                                                        TimeSpan? tickDelay = null,
                                                        TimeSpan? startDelay = null,
                                                        bool waitExecutionEnd = false)
        {
            return new DelayTimer<TState>(callback,
                                          state,
                                          lifeTimeToken,
                                          tickDelay,
                                          startDelay,
                                          waitExecutionEnd);
        }

        /// <summary>
        /// Creates the specified delay timer
        /// </summary>
        public static DelayTimer Create(Func<CancellationToken, Task> callback,
                                        CancellationToken lifeTimeToken = default,
                                        TimeSpan? tickDelay = null,
                                        TimeSpan? startDelay = null,
                                        bool waitExecutionEnd = false)
        {
            return new DelayTimer(callback,
                                  lifeTimeToken,
                                  tickDelay,
                                  startDelay,
                                  waitExecutionEnd);
        }

        /// <inheritdoc />
        protected override Task TickAsync(CancellationToken token)
        {
            return this._callback(token);
        }

        #endregion
    }

    /// <summary>
    /// Timer using async/await task to perform timer tasks
    /// </summary>
    public class DelayTimer<TState> : DelayBaseTimer
    {
        #region Fields

        private readonly Func<TState?, CancellationToken, Task> _callback;
        private readonly TState? _state;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayTimer{TState}"/> class.
        /// </summary>
        public DelayTimer(Func<TState?, CancellationToken, Task> callback,
                          TState? state,
                          CancellationToken lifeTimeToken,
                          TimeSpan? tickDelay,
                          TimeSpan? startDelay,
                          bool waitExecutionEnd)
            : base(lifeTimeToken, tickDelay, startDelay, waitExecutionEnd)
        {
            this._callback = callback;
            this._state = state;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Task TickAsync(CancellationToken token)
        {
            return this._callback(this._state, token);
        }

        #endregion
    }
}
