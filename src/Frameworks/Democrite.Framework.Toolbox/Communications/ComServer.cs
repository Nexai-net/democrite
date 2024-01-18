// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Communications
{
    using Democrite.Framework.Toolbox.Disposables;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Communication local server used to communicate between apps in local
    /// </summary>
    public sealed class ComServer : SafeAsyncDisposable
    {
        #region Fields

        private readonly CancellationTokenSource _serverWorkingCancellationTokenSource;
        private readonly SemaphoreSlim _locker;

        private readonly List<ComClient> _clients;
        private readonly int _allowedClient;
        private readonly int _port;

        private TaskCompletionSource<ComClientProxy>? _nextClientWaitTask;
        private Task? _listenningTask;
        private TcpListener? _server;
        private bool _hasStart;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ComServer"/> class.
        /// </summary>
        public ComServer(int port, int allowedClient = 1)
        {
            this._allowedClient = Math.Max(allowedClient, 1);

            this._serverWorkingCancellationTokenSource = new CancellationTokenSource();
            this._clients = new List<ComClient>();
            this._locker = new SemaphoreSlim(1);
            this._port = port;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the client proxies.
        /// </summary>
        public IReadOnlyCollection<ComClientProxy> GetClientProxies()
        {
            this._locker.Wait();
            try
            {
                return this._clients.Select(c => c.Proxy).ToArray();
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Starts the communication server
        /// </summary>
        public async Task StartAsync(CancellationToken token)
        {
            await this._locker.WaitAsync(token);
            try
            {
                if (this._hasStart)
                    return;

                var tcpServer = new TcpListener(IPAddress.Loopback, this._port);

                // Only one app could connect
                tcpServer.Start(this._allowedClient);
                this._server = tcpServer;

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this._listenningTask = Task.Run(ManagedClientAsync);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods

                this._hasStart = true;
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Waits the next client to connect.
        /// </summary>
        public Task<ComClientProxy> WaitNextClientAsync(CancellationToken token)
        {
            this._locker.Wait(token);
            try
            {
                if (this._nextClientWaitTask == null)
                {
                    var taskCompletion = new TaskCompletionSource<ComClientProxy>();
                    this._nextClientWaitTask = taskCompletion;
                    token.Register(() => taskCompletion.TrySetCanceled());
                }

                return this._nextClientWaitTask.Task;
            }
            finally
            {
                this._locker.Release();
            }
        }

        #region Tools

        /// <summary>
        /// Infinit loop that managed server acceptance
        /// </summary>
        private async Task ManagedClientAsync()
        {
            while (this._serverWorkingCancellationTokenSource.IsCancellationRequested == false)
            {
                var client = await this._server!.AcceptTcpClientAsync(this._serverWorkingCancellationTokenSource.Token);

                var comClient = new ComClient(client, this, this._serverWorkingCancellationTokenSource.Token);
                comClient.ClientLeaveEvent += ComClient_ClientLeaveEvent;

                await this._locker.WaitAsync(this._serverWorkingCancellationTokenSource.Token);
                try
                {
                    this._clients.Add(comClient);
                    comClient.Start();

                    if (this._nextClientWaitTask != null)
                    {
                        this._nextClientWaitTask.TrySetResult(comClient.Proxy);
                        this._nextClientWaitTask = null;
                    }

                    if (this._clients.Count >= this._allowedClient)
                        break;
                }
                finally
                {
                    this._listenningTask = null;
                    this._locker.Release();
                }
            }
        }

        /// <summary>
        /// COMs the client client leave event.
        /// </summary>
        private void ComClient_ClientLeaveEvent(ComClient obj)
        {
            if (obj is null)
                return;

            this._locker.Wait();
            try
            {
                this._clients.Remove(obj);

                if (this._clients.Count < this._allowedClient && this._listenningTask == null)
                {
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this._listenningTask = Task.Run(ManagedClientAsync);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
                }
            }
            finally
            {
                this._locker.Release();
            }

            obj.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            this._serverWorkingCancellationTokenSource.Cancel();
            this._server?.Stop();

            this._locker.Wait();
            try
            {
                foreach (var c in this._clients)
                    await c.DisposeAsync();
            }
            catch
            {
                this._locker.Release();
            }
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
}
