// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Remoting
{
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Helpers;
    using Democrite.UnitTests.ToolKit.Extensions;
    using Democrite.UnitTests.ToolKit.Models;

    using H.Formatters;
    using H.Pipes;
    using H.Pipes.Args;

    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class RemoteControllerService<TService> : IRemoteControllerService
        where TService : class
    {
        #region Fields

        private static readonly IReadOnlyDictionary<string, MethodInfo> s_resolutionCall;
        private static readonly Type s_traits = typeof(TService);

        private readonly WeakReference<TService> _serviceInstance;
        private readonly string _uniqueSuffix;

        private PipeServer<RemoteCallMessage>? _server;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="RemoteControllerService{TService}"/> class.
        /// </summary>
        static RemoteControllerService()
        {
            s_resolutionCall = s_traits.GetTreeValues(t => t?.GetInterfaces() ?? EnumerableHelper<Type>.ReadOnlyArray)
                                       .SelectMany(t => t.GetMethods())
                                       // raisedErrorIfDuplicate: true => Prevent duplicate because message sender came from another AppDomain
                                       .ToDictionary(k => k.GetUniqueId(raisedErrorIfDuplicate: true),
                                                     v => v.GetDefaultGenericImplementationMethod());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteControllerService"/> class.
        /// </summary>
        public RemoteControllerService(string uniqueSuffix, TService serviceInstance)
        {
            this._serviceInstance = new WeakReference<TService>(serviceInstance);
            this._uniqueSuffix = uniqueSuffix;

            this.Uid = typeof(TService).Name + "_" + uniqueSuffix;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the uid.
        /// </summary>
        public string Uid { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask InitializeRemoteAsync(CancellationToken token = default)
        {
            this._server = new PipeServer<RemoteCallMessage>(this.Uid, formatter: new NewtonsoftJsonFormatter());
            Debug.WriteLine("Start listening on pipe " + this.Uid);

            this._server.MessageReceived += ServerMessageReceived;
            this._server.ClientConnected += ServerClientConnected;

            await this._server.StartAsync(token);
            Debug.WriteLine("Start listening on pipe " + this.Uid);
        }

        /// <summary>
        /// Call when a new client connect
        /// </summary>
        private void ServerClientConnected(object? sender, ConnectionEventArgs<RemoteCallMessage> e)
        {
            //e.Connection.MessageReceived += Connection_MessageReceived;

            //if (!e.Connection.IsConnected)
            //    e.Connection.StartAt();

            Debug.WriteLine("Client connect on pipe " + this.Uid);
        }

        /// <summary>
        /// Process the message received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConnectionMessageEventArgs{System.Nullable{RemoteCallMessage}}"/> instance containing the event data.</param>
        private async void ServerMessageReceived(object? sender, ConnectionMessageEventArgs<RemoteCallMessage?> e)
        {
            if (e == null || e.Message == null || string.IsNullOrEmpty(e.Message.JsonContent))
                return;

            var msg = JsonConvert.DeserializeObject<MethodCallMessage>(e.Message.JsonContent, TestRemotingService.SerializationSettings);
            if (s_resolutionCall.TryGetValue(msg!.MethodUniqueId, out var resolution))
            {
                if (this._serviceInstance.TryGetTarget(out var inst))
                {
                    var returnObjJsonStr = string.Empty;
                    string? exception = null;
                    bool isCancelled = false;

                    try
                    {
                        var result = resolution.Invoke(inst, msg.ArgumentRoot?.Flattern().ToArray() ?? EnumerableHelper<object>.ReadOnlyArray);

                        var resultInfo = result?.GetType().GetTypeInfoExtension();
                        object? returnObj = null;

                        if (resultInfo?.IsValueTask ?? false)
                        {
                            result = resultInfo.GetTaskFromAnyValueTask(result);
                        }

                        if (result is Task tsk)
                        {
                            await tsk;

                            if (resolution.ReturnType.GetGenericArguments().Length == 1)
                                returnObj = tsk.GetResult();
                        }

                        if (returnObj != null)
                            returnObjJsonStr = JsonConvert.SerializeObject(returnObj, TestRemotingService.SerializationSettings);
                    }
                    catch (OperationCanceledException)
                    {
                        isCancelled = true;
                    }
                    catch (Exception ex)
                    {
                        exception = ex.GetFullString();
                    }

                    await e.Connection.WriteAsync(new RemoteCallMessage(e.Message.InstanceId,
                                                                        e.Message.ExecutionId,
                                                                        returnObjJsonStr,
                                                                        exception,
                                                                        isCancelled));
                    return;
                }
            }

            throw new NotImplementedException(GetType().Name + " - ServerMessageReceived - could not resolve - " + e.Message.JsonContent);
        }

        #endregion
    }
}
