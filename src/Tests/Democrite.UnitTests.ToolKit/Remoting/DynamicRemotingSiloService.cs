// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.UnitTests.ToolKit.Remoting
{
    using Elvex.Toolbox.Abstractions.Extensions.Types;
    using Elvex.Toolbox.Abstractions.Models;
    using Democrite.UnitTests.ToolKit.Extensions;
    using Democrite.UnitTests.ToolKit.Models;

    using H.Formatters;
    using H.Pipes;

    using Moq;

    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Dynamic.DynamicObject" />
    internal sealed class DynamicRemotingSiloService<TService> : Mock<TService>
        where TService : class
    {
        #region Fields

        private static readonly TimeSpan s_callTimeout;
        private static readonly MethodInfo s_anyMthd;

        private readonly Dictionary<Guid, TaskCompletionSource<object?>> _callRegistry;
        private readonly string _uniqueInstanceId;
        private readonly SemaphoreSlim _locker;

        private PipeClient<RemoteCallMessage>? _client;

        #endregion

        #region Ctor

        static DynamicRemotingSiloService()
        {
            s_anyMthd = typeof(It).GetMethod(nameof(It.IsAny)) ?? throw new ArgumentNullException("It.IsAny");

            s_callTimeout = Debugger.IsAttached ? TimeSpan.FromMinutes(10) : TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicRemotingSiloService"/> class.
        /// </summary>
        public DynamicRemotingSiloService(string uniqueInstanceId)
        {
            this._locker = new SemaphoreSlim(1);
            this._uniqueInstanceId = uniqueInstanceId.Replace(nameof(IRemoteControllerService) + "_", "");
            this._callRegistry = new Dictionary<Guid, TaskCompletionSource<object?>>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        public void Map()
        {
            MapRec(typeof(TService), new HashSet<Type>());
        }

        /// <summary>
        /// Connect to remove servoce
        /// </summary>
        private void EnsureConnectToRemote()
        {
            EnsureConnectToRemoteAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
        }

        /// <summary>
        /// Connect to remove servoce
        /// </summary>
        private async Task EnsureConnectToRemoteAsync(CancellationToken token = default)
        {
            try
            {
                await this._locker.WaitAsync(token);

                if (this._client?.IsConnected == true)
                    return;

                if (this._client == null)
                {
                    this._client = new PipeClient<RemoteCallMessage>(this._uniqueInstanceId, reconnectionInterval: TimeSpan.FromMilliseconds(100), formatter: new NewtonsoftJsonFormatter());
                    this._client.MessageReceived += ResponseReceived;
                }

                this._client.AutoReconnect = true;

                Debug.WriteLine("Try to connect on pipe " + this._uniqueInstanceId);
                await this._client.ConnectAsync(token);
                Debug.WriteLine("Connected on pipe " + this._uniqueInstanceId);

            }
            catch (Exception)
            {
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Send as message response
        /// </summary>
        private void ResponseReceived(object? sender, H.Pipes.Args.ConnectionMessageEventArgs<RemoteCallMessage?> e)
        {
            try
            {
                this._locker.Wait();

                Debug.Assert(e.Message != null);
                if (this._callRegistry.TryGetValue(e.Message.ExecutionId, out var task))
                {
                    if (e.Message.IsCancelled)
                    {
                        task.TrySetCanceled();
                        return;
                    }

                    if (!string.IsNullOrEmpty(e.Message.Error))
                    {
                        task.TrySetException(new Exception(e.Message.Error));
                        return;
                    }

                    object? result = null;

                    if (!string.IsNullOrEmpty(e.Message.JsonContent))
                    {
                        result = JsonConvert.DeserializeObject(e.Message.JsonContent, TestRemotingService.SerializationSettings);
                    }

                    task.TrySetResult(result);
                }
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Remote call without expected result
        /// </summary>
        private void SolverVoidMethod(IInvocation invocation)
        {
            Task.Run(async () => await SolverVoidMethodAsync(invocation)).GetAwaiter().GetResult();
            return;
        }

        /// <summary>
        /// Remote call with result
        /// </summary>
        private object? SolverVoidMethodWithReturn(IInvocation invocation)
        {
            var result = Task.Run(async () => await SolverVoidMethodAsync(invocation)).GetAwaiter().GetResult();
            return result;
        }

        /// <summary>
        /// Async remote call with result
        /// </summary>
        private async Task<object?> SolverVoidMethodAsync(IInvocation invocation)
        {
            using (var timeoutCallToken = new CancellationTokenSource(s_callTimeout))
            {
                await EnsureConnectToRemoteAsync(timeoutCallToken.Token);

                ArgumentNullException.ThrowIfNull(this._client);

                var taskCompletion = new TaskCompletionSource<object?>();
                var execId = Guid.NewGuid();

                try
                {
                    await this._locker.WaitAsync(timeoutCallToken.Token);

                    this._callRegistry.Add(execId, taskCompletion);
                }
                finally
                {
                    this._locker.Release();
                }

                var callSerialized = JsonConvert.SerializeObject(new MethodCallMessage(invocation.Method.Name,

                                                                                       // raisedErrorIfDuplicate: true => Prevent duplicate because message sender came from another AppDomain
                                                                                       (invocation.Method.IsGenericMethod
                                                                                                ? invocation.Method.GetGenericMethodDefinition()
                                                                                                : invocation.Method).GetUniqueId(raisedErrorIfDuplicate: true),

                                                                                       TypedArgument.From(invocation.Arguments,
                                                                                                          invocation.Method.GetParameters().Select(s => s.ParameterType).ToArray()),
                                                                                       invocation.Method.ReflectedType?.AssemblyQualifiedName),

                                                                 TestRemotingService.SerializationSettings);

                var callMsg = new RemoteCallMessage(this._uniqueInstanceId, execId, callSerialized, string.Empty, false);

                await this._client.WriteAsync(callMsg, timeoutCallToken.Token);
                timeoutCallToken.Token.ThrowIfCancellationRequested();

                await taskCompletion.Task;
                timeoutCallToken.Token.ThrowIfCancellationRequested();

                var returnType = invocation.Method.ReturnType;

                var noReturn = returnType != null &&

                               (returnType == typeof(void) ||
                                returnType == typeof(Task) ||
                                returnType == typeof(ValueTask));

                // Convert result to well format
                var resultInst = taskCompletion.Task.Result;

                if (returnType != null && (resultInst?.GetType().IsAssignableTo(returnType) ?? false) == false)
                {
                    var extendInfo = returnType.GetTypeInfoExtension();

                    if (extendInfo.IsTask || extendInfo.IsValueTask)
                    {
                        if (extendInfo.IsTask)
                        {
                            if (noReturn)
                                resultInst = Task.CompletedTask;
                            else
                                resultInst = extendInfo.GetSpecifcTypeExtend<ITaskTypeInfoEnhancer>().GetTaskFromResult(resultInst);
                        }
                        else if (extendInfo.IsValueTask)
                        {
                            if (noReturn)
                                resultInst = ValueTask.CompletedTask;
                            else
                                resultInst = extendInfo.GetSpecifcTypeExtend<IValueTaskTypeInfoEnhancer>().GetValueTaskFromResult(resultInst);
                        }
                    }
                    else
                    {
                        throw new InvalidCastException("[Remote][" + invocation.Method + "] Could not convert remote response type " + resultInst!.GetType() + " to method return type " + returnType);
                    }
                }

                return resultInst;
            }
        }

        /// <summary>
        /// Recursive map all public mehotd
        /// </summary>
        private void MapRec(Type type, HashSet<Type> mapTypes)
        {
            if (mapTypes.Contains(type))
                return;

            mapTypes.Add(type);

            foreach (var mthd in type.GetMethods())
            {
                var parameters = (from p in mthd.GetParameters()
                                  let anyType = (p.ParameterType.IsGenericParameter == true)
                                                        // Must at least have an interface in constraint to be able to mock properly
                                                        ? p.ParameterType.GetGenericParameterConstraints().FirstOrDefault(t => t.IsInterface) ?? typeof(It.IsAnyType)
                                                        : p.ParameterType
                                  select Expression.Call(null, s_anyMthd.MakeGenericMethod(anyType))).ToArray();

                var inst = Expression.Parameter(typeof(TService), "s");

                if (mthd.ReturnParameter.ParameterType == typeof(void))
                {
                    SetupVoidCall(inst, mthd, parameters);
                }
                else
                {
                    var m = GetType().GetMethod(nameof(SetupFuncCall), BindingFlags.NonPublic | BindingFlags.Instance);
                    Debug.Assert(m != null);
                    m.MakeGenericMethod(mthd.ReturnType).Invoke(this, new object[] { inst, mthd, parameters });
                }
            }

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                MapRec(type, mapTypes);
            }

            var interfaces = type.GetInterfaces();
            foreach (var i in interfaces)
            {
                MapRec(i, mapTypes);
            }
        }

        private void SetupVoidCall(ParameterExpression inst, MethodInfo mthd, IEnumerable<Expression> parameters)
        {
            base.Setup(Expression.Lambda<Action<TService>>(Expression.Call(inst, mthd, parameters), inst))
                .Callback(new InvocationAction(SolverVoidMethod));
        }

        private void SetupFuncCall<TReturn>(ParameterExpression inst, MethodInfo mthd, IEnumerable<Expression> parameters)
        {
            if (mthd.IsGenericMethod)
                mthd = mthd.GetDefaultGenericImplementationMethod();

#pragma warning disable CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
            base.Setup(Expression.Lambda<Func<TService, TReturn>>(Expression.Call(inst, mthd, parameters), inst))
                .Returns(new InvocationFunc(SolverVoidMethodWithReturn));
#pragma warning restore CS8621 // Nullability of reference types in return type doesn't match the target delegate (possibly because of nullability attributes).
        }

        #endregion
    }

    internal static class DyncamicRemotingSiloServiceExtension
    {
        /// <summary>
        /// Creates dynamic remote handler
        /// </summary>
        public static Mock? CreateRemoteServiceFrom(this Type service, string uniqueInstanceId)
        {
            var finalType = typeof(DynamicRemotingSiloService<>).MakeGenericType(service);

            var mockController = (Mock?)Activator.CreateInstance(finalType, new object[] { uniqueInstanceId });

            ArgumentNullException.ThrowIfNull(mockController);

            var initMthd = mockController.GetType().GetMethod(nameof(DynamicRemotingSiloService<string>.Map));

            ArgumentNullException.ThrowIfNull(initMthd);
            initMthd.Invoke(mockController, Array.Empty<object>());

            return mockController;
        }
    }
}
