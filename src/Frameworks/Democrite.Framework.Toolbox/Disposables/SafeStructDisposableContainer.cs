// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Disposables
{
    using Democrite.Framework.Toolbox.Abstractions.Disposables;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Structure used to group disposable instances
    /// </summary>
    /// <seealso cref="Democrite.Framework.Toolbox.Abstractions.Disposables.ISafeDisposableContainer" />
    public struct SafeStructDisposableContainer : ISafeDisposableContainer
    {
        #region Fields

        private readonly Queue<IDisposable> _disposables;
        private long _disposable;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeStructDisposableContainer"/> struct.
        /// </summary>
        public SafeStructDisposableContainer()
        {
            this._disposables = new Queue<IDisposable>();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Increment(ref this._disposable) > 1)
                return;

            GC.SuppressFinalize(this);

            IEnumerable<IDisposable> items;
            lock (this._disposables)
            {
                items = this._disposables.ToArray();
                this._disposables.Clear();
            }

            foreach (var item in items)
            {
                try
                {
                    item.Dispose();
                }
                catch
                {
                }
            }
        }

        /// <inheritdoc/>
        public void PushToken<TToken>(TToken token) where TToken : IDisposable
        {
            if (Interlocked.Read(ref this._disposable) > 0)
                throw new ObjectDisposedException(nameof(SafeStructDisposableContainer));

            lock (this._disposables)
            {
                this._disposables.Enqueue(token);
            }
        }

        #endregion
    }
}
