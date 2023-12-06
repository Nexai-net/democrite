// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Disposables
{
    using System;

    /// <summary>
    /// Disposable object that perform an action pass in argument at dispose time
    /// </summary>
    public class DisposableAction<TContent> : SafeDisposable<TContent>
    {
        #region Fields

        private readonly Action<TContent> _callbackAtDisposeTime;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableAction"/> class.
        /// </summary>
        public DisposableAction(Action<TContent> callbackAtDisposeTime, TContent content)
            : base(content)
        {
            ArgumentNullException.ThrowIfNull(callbackAtDisposeTime);
            this._callbackAtDisposeTime = callbackAtDisposeTime;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected override void DisposeBegin()
        {
            this._callbackAtDisposeTime(this.Content);
            base.DisposeBegin();
        }

        #endregion
    }

    /// <summary>
    /// Disposable object that perform an action pass in argument at dispose time
    /// </summary>
    public class DisposableAction : DisposableAction<NoneType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableAction"/> class.
        /// </summary>
        public DisposableAction(Action callbackAtDisposeTime)
            : base(_ => callbackAtDisposeTime(), NoneType.Instance)
        {
        }
    }
}