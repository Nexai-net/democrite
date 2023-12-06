// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Helpers
{
    using Democrite.Framework.Toolbox.Abstractions.Disposables;
    using Democrite.Framework.Toolbox.Disposables;

    using System;

    /// <summary>
    /// Context used to simplify usage of <see cref="CancellationHelper.SingleAccessScope(SemaphoreSlim, Func{CancellationTokenSource}, Action{CancellationTokenSource}, TimeSpan?)"/>
    /// </summary>
    public sealed class CancellationContext : SafeDisposable
    {
        #region Fields

        private CancellationTokenSource? _cancellationSource;
        private readonly SemaphoreSlim _locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationContext"/> class.
        /// </summary>
        public CancellationContext()
        {
            this._locker = new SemaphoreSlim(1);
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="CancellationHelper.SingleAccessScope"/>
        public ISafeDisposable<CancellationToken> Lock()
        {
            return CancellationHelper.SingleAccessScope(this._locker, () => this._cancellationSource, c => this._cancellationSource = c);
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected override void DisposeBegin()
        {
            try
            {
                this._cancellationSource?.Cancel();
            }
            catch (Exception)
            {

            }

            base.DisposeBegin();
        }

        /// <summary>
        /// Call at the end of the dispose process
        /// </summary>
        protected override void DisposeEnd()
        {
            this._locker.Dispose();
            base.DisposeEnd();
        }

        #endregion
    }
}
