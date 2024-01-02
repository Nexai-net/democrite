// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Configurations
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Toolbox.Abstractions.Attributes;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class about cluster interaction node/client
    /// </summary>
    /// <typeparam name="TConfig">The type of the configuration.</typeparam>
    /// <seealso cref="SafeAsyncDisposable" />
    public abstract class DemocriteBasePart<TConfig> : SafeAsyncDisposable, IHostedService
    {
        #region Fields

        private readonly IDemocriteExecutionHandler _democriteExecutionHandler;

        private readonly SemaphoreSlim _locker;
        private readonly bool _hostOwned;
        private readonly ILogger _logger;
        private readonly IHost _host;

        private CancellationTokenSource? _runningTaskCancellationTokenSource;
        private Task? _runningTask;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterBasePart{TConfig}"/> class.
        /// </summary>
        protected DemocriteBasePart(IHost host,
                                    TConfig? config,
                                    IDemocriteExecutionHandler? democriteExecutionHandler,
                                    bool hostOwner)
        {
            ArgumentNullException.ThrowIfNull(host);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(democriteExecutionHandler);

            this.Config = config;
            this._host = host;

            this._hostOwned = hostOwner;

            this._logger = this._host.Services.GetService<ILoggerFactory>()?.CreateLogger(GetType().Name) ?? NullLogger.Instance;

            this._democriteExecutionHandler = democriteExecutionHandler;

            this._locker = new SemaphoreSlim(1);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public TConfig? Config { get; }

        /// <summary>
        /// Gets the services.
        /// </summary>
        protected IServiceProvider Services
        {
            get { return this._host.Services; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start the <see cref="ClusterNode"/> and await until the server stop
        /// </summary>
        public async Task StartUntilEndAsync(Func<IServiceProvider, IDemocriteExecutionHandler, CancellationToken, Task>? runningFunction = null, CancellationToken token = default)
        {
            await StartAsync(runningFunction, token);

            try
            {
                await this._locker.WaitAsync(token);

                if (this._runningTaskCancellationTokenSource != null)
                    token = CancellationTokenSource.CreateLinkedTokenSource(token, this._runningTaskCancellationTokenSource.Token).Token;
            }
            finally
            {
                this._locker.Release();
            }

            await this._host.WaitForShutdownAsync(token);
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken token = default)
        {
            return StartAsync(null, token);
        }

        /// <summary>
        /// Start the <see cref="ClusterNode"/>, execute <paramref name="runningFunction"/> and give back the hand while server run in background
        /// </summary>
        public async Task StartAsync(Func<IServiceProvider, IDemocriteExecutionHandler, CancellationToken, Task>? runningFunction = null, CancellationToken token = default)
        {
            Task? runningTask = null;
            try
            {
                await this._locker.WaitAsync(token);

                if (this._runningTask == null)
                {
                    this._runningTaskCancellationTokenSource = new CancellationTokenSource();
                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, this._runningTaskCancellationTokenSource.Token);
                    token = linkedToken.Token;

                    var serviceToInit = this._host.Services.GetServices<INodeInitService>();

                    var initTasks = serviceToInit.Where(s => !s.IsInitialized && !s.IsInitializing)
                                                 .Select(s => s.InitializationAsync(this._host.Services, token: token))
                                                 .ToArray();

                    try
                    {
                        await initTasks.SafeWhenAllAsync(token);
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var inner in ex.InnerExceptions)
                            this._logger.OptiLog(LogLevel.Critical, "Exceptions on INodeInitService : {exception}", inner);
                    }

                    if (this._hostOwned)
                        this._runningTask = this._host.StartAsync(token);
                    else
                        this._runningTask = Task.CompletedTask;
                }

                runningTask = this._runningTask;
            }
            finally
            {
                this._locker.Release();
            }

            await OnStartAsync(token);

            if (runningTask != null)
            {
                await runningTask;

                if (runningFunction != null)
                    await runningFunction(this._host.Services, this._democriteExecutionHandler, token);
            }
        }

        /// <summary>
        /// Stop the <see cref="ClusterNode"/>
        /// </summary>
        public async Task StopAsync(CancellationToken token = default)
        {
            Task? runningTask = null;
            try
            {
                await this._locker.WaitAsync(token);

                runningTask = this._runningTask;
                if (this._runningTask != null)
                {
                    var serviceToFinalize = this._host.Services.GetServices<INodeFinalizeService>();

                    var initTasks = serviceToFinalize.Where(s => !s.IsFinalized && !s.IsFinalizing)
                                                     .Select(s => s.FinalizeAsync(token: token))
                                                     .ToArray();

                    try
                    {
                        await initTasks.SafeWhenAllAsync(token);
                    }
                    catch (OperationCanceledException)
                    { 
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var inner in ex.InnerExceptions)
                            this._logger.OptiLog(LogLevel.Critical, "Exceptions on INodeInitService : {exception}", inner);
                    }

                    this._runningTaskCancellationTokenSource?.Cancel();

                    this._runningTaskCancellationTokenSource = null;
                    this._runningTask = null;

                    return;
                }

                if (this._hostOwned)
                    await this._host.StopAsync(token);
            }
            catch (Exception ex)
            {
                OnStopFailed(ex);
            }
            finally
            {
                this._locker.Release();
            }

            try
            {
                if (runningTask != null)
                    await runningTask;
            }
            catch (OperationCanceledException)
            {
                // It's normal if the only error is a cancellation : OperationCanceledException
            }
        }

        /// <summary>
        /// Called when stop action failed.
        /// </summary>
        [ThreadSafe]
        protected virtual void OnStopFailed(Exception ex)
        {
            this._logger.OptiLog(LogLevel.Critical, "Democrite node stop failed : {exception}", ex);
        }

        /// <summary>
        /// Called when cluster node start.
        /// </summary>
        protected virtual Task OnStartAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
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

        /// <summary>
        /// Configure <see cref="HostBuilder"/>
        /// </summary>
        protected static HostBuilder CreateAndConfigureHost(string[]? args,
                                                            Action<HostBuilderContext, IConfigurationBuilder>? setupConfig)
        {
            var host = new HostBuilder();

            if (args is not null && args.Any())
                host.ConfigureDefaults(args);

            if (setupConfig is not null)
            {
                host.ConfigureAppConfiguration((ctx, builder) =>
                {
                    setupConfig(ctx, builder);
                    builder.ActivateTemplatedConfiguration();
                });
            }

            return host;
        }

        #endregion
    }
}
