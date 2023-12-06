// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Disposables
{
    using Democrite.Framework.Toolbox.Abstractions.Disposables;
    using Democrite.Framework.Toolbox.Memories;

    using System;

    /// <summary>
    /// Thread Safe disposable implementations
    /// </summary>
    public abstract class SafeDisposable : IDisposable
    {
        #region Fields

        private readonly SafeContainer<IDisposable> _disposableDependencies;
        private long _disposableCounter = 0;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SafeDisposable"/> class.
        /// </summary>
        static SafeDisposable()
        {
            Empty = new DisposableAction(() => { });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeDisposable"/> class.
        /// </summary>
        public SafeDisposable()
        {
            this._disposableDependencies = new SafeContainer<IDisposable>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SafeDisposable"/> class.
        /// </summary>
        ~SafeDisposable()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the empty.
        /// </summary>
        public static IDisposable Empty { get; }

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
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Registers the disposable dependency to dispose when this instance is.
        /// </summary>
        protected Guid RegisterDisposableDependency(IDisposable disposable)
        {
            return this._disposableDependencies.Register(disposable);
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
        private void Dispose(bool manualCall)
        {
            if (Interlocked.Increment(ref this._disposableCounter) > 1)
                return;

            DisposeBegin();

            DisposeUnmanaged();

            if (manualCall)
                DisposeManaged();

            var dependencyDisposable = this._disposableDependencies.GetContainerCopy();
            this._disposableDependencies.Clear();

            foreach (var dependency in dependencyDisposable)
                dependency.Dispose();

            DisposeEnd();
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected virtual void DisposeBegin()
        {
        }

        /// <summary>
        /// Called to disposed un managed resources
        /// </summary>
        protected virtual void DisposeUnmanaged()
        {
        }

        /// <summary>
        /// Called to disposed managed resources only on manual call
        /// </summary>
        protected virtual void DisposeManaged()
        {
        }

        /// <summary>
        /// Call at the end of the dispose process
        /// </summary>
        protected virtual void DisposeEnd()
        {
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Thread Safe disposable implementations
    /// </summary>
    public abstract class SafeDisposable<TContent> : SafeDisposable, ISafeDisposable<TContent>, ISafeDisposable, IDisposable
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeDisposable"/> class.
        /// </summary>
        public SafeDisposable(TContent content)
        {
            this.Content = content;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the content.
        /// </summary>
        public TContent Content { get; }

        #endregion
    }
}
