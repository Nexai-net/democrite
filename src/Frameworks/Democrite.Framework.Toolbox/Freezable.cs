// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox
{
    using Democrite.Framework.Toolbox.Disposables;

    using System;

    /// <summary>
    /// Provide freezable notation handling
    /// </summary>
    public abstract class Freezable
    {
        #region Fields

        private readonly SemaphoreSlim _locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Freezable"/> class.
        /// </summary>
        protected Freezable()
        {
            this._locker = new SemaphoreSlim(1);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Open a freeze safe context
        /// </summary>
        protected IDisposable FreezeAccess()
        {
            this._locker.Wait();

            return new DisposableAction(() =>
            {
                this._locker.Release();
            });
        }

        /// <summary>
        /// Open un freeze safe scope to do some changes
        /// </summary>
        protected IDisposable UnFreezeAccess()
        {
            this._locker.Wait();

            return new DisposableAction(() =>
            {
                this._locker.Release();
            });
        }

        /// <summary>
        /// Releases the resources.
        /// </summary>
        /// <remarks>
        ///     Use to release un managed data without to impose <see cref="IDisposable"/> to child
        /// </remarks>
        private void ReleaseResources()
        {
            this._locker.Dispose();
        }

        #endregion
    }
}
