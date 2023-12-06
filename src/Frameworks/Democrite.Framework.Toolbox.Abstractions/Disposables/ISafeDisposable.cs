// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Disposables
{
    using System;

    /// <summary>
    /// Define a thread safe disposable instance.
    /// </summary>
    public interface ISafeDisposable : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        bool IsDisposed { get; }
    }

    /// <summary>
    /// Define a disposable instance thread safe that transport a <typeparamref name="TContent"/>
    /// </summary>
    public interface ISafeDisposable<TContent> : ISafeDisposable
    {
        /// <summary>
        /// Gets the content.
        /// </summary>
        public TContent Content { get; }
    }
}
