// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Disposables
{
    using Democrite.Framework.Toolbox.Memories;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread Safe disposable implementations
    /// </summary>
    public abstract class SafeAsyncDisposable : IAsyncDisposable
    {
        #region Fields

        private readonly SafeContainer<IAsyncDisposable> _asyncDisposableDependencies;
        private readonly SafeContainer<IDisposable> _disposableDependencies;
        private long _disposableCounter = 0;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeAsyncDisposable"/> class.
        /// </summary>
        public SafeAsyncDisposable()
        {
            this._asyncDisposableDependencies = new SafeContainer<IAsyncDisposable>();
            this._disposableDependencies = new SafeContainer<IDisposable>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SafeDisposable"/> class.
        /// </summary>
        ~SafeAsyncDisposable()
        {
            Task.Run(async () => await DisposeAsync(false)).Wait(10000);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get { return Interlocked.Read(ref this._disposableCounter) > 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await DisposeAsync(true);
        }

        /// <summary>
        /// Registers the disposable dependency to dispose when this instance is.
        /// </summary>
        protected Guid? RegisterDisposableDependency<TInst>(TInst inst)
        {
            if (inst is IAsyncDisposable asyncDisposable)
                return this._asyncDisposableDependencies.Register(asyncDisposable);
            else if (inst is IDisposable disposable)
                return this._disposableDependencies.Register(disposable);

            return null;
        }

        /// <summary>
        /// Check and Throws if disposed.
        /// </summary>
        protected void CheckAndThrowIfDisposed()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        #region Tools

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        private async ValueTask DisposeAsync(bool manualCall)
        {
            if (Interlocked.Increment(ref this._disposableCounter) > 1)
                return;

            await DisposeBeginAsync();

            await DisposeUnmanagedAsync();

            if (manualCall)
                await DisposeManagedAsync();

            var dependencyAsyncDisposable = this._asyncDisposableDependencies.GetContainerCopy();
            this._asyncDisposableDependencies.Clear();

            foreach (var dependency in dependencyAsyncDisposable)
                await dependency.DisposeAsync();

            var dependencyDisposable = this._disposableDependencies.GetContainerCopy();
            this._disposableDependencies.Clear();

            foreach (var dependency in dependencyDisposable)
                dependency.Dispose();

            await DisposeEndAsync();
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected virtual ValueTask DisposeBeginAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Called to disposed un managed resources
        /// </summary>
        protected virtual ValueTask DisposeUnmanagedAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Called to disposed managed resources only on manual call
        /// </summary>
        protected virtual ValueTask DisposeManagedAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Call at the end of the dispose process
        /// </summary>
        protected virtual ValueTask DisposeEndAsync()
        {
            return ValueTask.CompletedTask;
        }

        #endregion

        #endregion
    }
}
