// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Communications
{
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Helpers;

    using System.Diagnostics;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using static Democrite.Framework.Toolbox.Communications.ComClientProxy;

    /// <summary>
    /// Proxy to send or received data from a client
    /// </summary>
    public sealed class ComClientProxy : SafeDisposable, IObservable<UnmanagedMessage>
    {
        #region Fields

        private static readonly int s_guidSize;

        private readonly Dictionary<Guid, TaskCompletionSource<byte[]>> _pendingResultTasks;
        private readonly ComClient _comClient;

        private readonly IDisposable _observerToken;
        private readonly Subject<UnmanagedMessage> _subject;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ComClientProxy"/> class.
        /// </summary>
        static ComClientProxy()
        {
            var emptyBytes = Guid.Empty.ToString();
            s_guidSize = emptyBytes.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComClientProxy"/> class.
        /// </summary>
        internal ComClientProxy(ComClient comClient)
        {
            this._pendingResultTasks = new Dictionary<Guid, TaskCompletionSource<byte[]>>();
            this._comClient = comClient;
            this.Uid = Guid.NewGuid();

            var observer = Observer.Create<byte[]>(OnMessageReceived, OnComplete);
            this._observerToken = this._comClient.Subscribe(observer);

            this._subject = new Subject<UnmanagedMessage>();
            var connectable = this._subject.SubscribeOn(TaskPoolScheduler.Default)
                                           .Publish();

            var connectObservableToken = connectable.Connect();
            RegisterDisposableDependency(connectObservableToken);
        }

        #endregion

        #region Nested

        /// <summary>
        /// Define the type of message carry
        /// </summary>
        public enum MessageType : byte
        {
            User,
            System,
            Ping,
            Pong,
        }

        public record UnmanagedMessage(MessageType Type, string MessageId, byte[] Message);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the client uid.
        /// </summary>
        public Guid Uid { get; }

        #endregion

        #region Method

        /// <summary>
        /// Ping target to ensure client is repondings
        /// </summary>
        public async Task<bool> PingAsync(CancellationToken cancellationToken)
        {
            try
            {
                var timeoutCancelToken = CancellationHelper.Timeout(TimeSpan.FromSeconds(5));
                var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelToken);

                var sendUid = SendImpl(MessageType.Ping, null, out var waitingResultTask, true, token.Token);
                await Task.Factory.StartNew(async () => await waitingResultTask!.Task, token.Token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Asks data
        /// </summary>
        public async Task<string> AskAsync(string data, CancellationToken cancellationToken)
        {
            var timeoutCancelToken = CancellationHelper.Timeout(TimeSpan.FromSeconds(Debugger.IsAttached ? 50000 : 5));
            var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelToken);

            var sendUid = SendImpl(MessageType.User, Encoding.UTF8.GetBytes(data), out var waitingResultTask, true, token.Token);
            await Task.Factory.StartNew(async () => await waitingResultTask!.Task, token.Token);

            var results = waitingResultTask!.Task.Result;
            if (results != null && results.Any())
                return Encoding.UTF8.GetString(results);

            return string.Empty;
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<UnmanagedMessage> observer)
        {
            return this._subject.Subscribe(observer);
        }

        #region Tools

        /// <inheritdoc />
        private void OnMessageReceived(byte[] msg)
        {
            if (msg == null || msg.Length == 0)
                return;

            ReadOnlySpan<byte> msgSpan = msg;

            var type = (MessageType)msgSpan[0];

            var msgIdStr = Encoding.UTF8.GetString(msgSpan.Slice(1, s_guidSize));

            var msgRemains = msgSpan.Slice(1 + s_guidSize).ToArray();

            if (Guid.TryParse(msgIdStr, out var uid))
            {
                TaskCompletionSource<byte[]>? resultTask;

                lock (this._pendingResultTasks)
                {
                    this._pendingResultTasks.TryGetValue(uid, out resultTask);
                }

                if (resultTask != null)
                {
                    resultTask.TrySetResult(msgRemains);
                    return;
                }
            }

            if (type == MessageType.Ping)
            {
                SendImpl(MessageType.Pong, null, out _, false, default);
                return;
            }

            this._subject.OnNext(new UnmanagedMessage(type, msgIdStr, msgRemains));
        }

        /// <inheritdoc />
        private void OnComplete()
        {
            Dispose();
        }

        /// <summary>
        /// Sends message
        /// </summary>
        private Guid SendImpl(MessageType type,
                              byte[]? messages,
                              out TaskCompletionSource<byte[]>? task,
                              bool createWaitTask,
                              CancellationToken cancellationToken)
        {
            var msg = FormatData(type, messages, out var msgId);
            task = null;

            if (createWaitTask)
            {
                lock (this._pendingResultTasks)
                {
                    task = new TaskCompletionSource<byte[]>();
                    this._pendingResultTasks.Add(msgId, task);
                }

                cancellationToken.Register((t) => ((TaskCompletionSource<byte[]>)t!).TrySetCanceled(), task);
            }

            this._comClient.Send(msg);
            return msgId;
        }

        /// <summary>
        /// Formats the data.
        /// </summary>
        private byte[] FormatData(MessageType ping, byte[]? messages, out Guid messageId)
        {
            messageId = Guid.NewGuid();
            var idArray = Encoding.UTF8.GetBytes(messageId.ToString());

            var data = new byte[(messages?.Length ?? 0) + 1 + s_guidSize];

            data[0] = (byte)ping;
            idArray.CopyTo(data, 1);
            messages?.CopyTo(data, 1 + idArray.Length);

            return data;
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected override void DisposeBegin()
        {
            this._observerToken.Dispose();

            IReadOnlyCollection<TaskCompletionSource<byte[]>> tasks;

            lock (this._pendingResultTasks)
            {
                tasks = this._pendingResultTasks.Values.ToArray();
                this._pendingResultTasks.Clear();
            }

            foreach (var task in tasks)
                task.TrySetCanceled();

            base.DisposeBegin();
        }

        #endregion

        #endregion
    }
}
