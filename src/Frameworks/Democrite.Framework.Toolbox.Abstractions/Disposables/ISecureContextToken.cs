// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Disposables
{
    using System;

    /// <summary>
    /// Token used as security temporary key
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ISecureContextToken : IDisposable
    {
        /// <summary>
        /// Gets the secure context identifier.
        /// </summary>
        Guid SecureContextId { get; }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Embed content
    /// </remarks>
    public interface ISecureContextToken<TContent> : ISecureContextToken
    //where TContent : class
    {
        /// <summary>
        /// Gets the content.
        /// </summary>
        TContent Content { get; }
    }
}
