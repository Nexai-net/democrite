// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Disposables
{
    using Democrite.Framework.Toolbox.Abstractions.Disposables;

    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SafeDisposable" />
    /// <seealso cref="ISecureContextToken" />
    public class SecureContextToken : DisposableAction<Guid>, ISecureContextToken
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureContextToken"/> class.
        /// </summary>
        public SecureContextToken(Guid secureContextId, Action<Guid> actionAtDisposable)
            : base(actionAtDisposable, secureContextId)
        {
            this.SecureContextId = secureContextId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the secure context identifier.
        /// </summary>
        public Guid SecureContextId { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified ISecureContextToken.
        /// </summary>
        public static ISecureContextToken<TContent> Create<TContent>(TContent content, Action<Guid> actionAtDisposable)
        {
            return new SecureContextToken<TContent>(Guid.NewGuid(), content, actionAtDisposable);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TContent">The type of the token.</typeparam>
    /// <seealso cref="SafeDisposable" />
    /// <seealso cref="ISecureContextToken" />
    public class SecureContextToken<TContent> : SecureContextToken, ISecureContextToken<TContent>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureContextToken{TContent}"/> class.
        /// </summary>
        public SecureContextToken(Guid secureContextId, TContent token, Action<Guid> actionAtDisposable)
            : base(secureContextId, actionAtDisposable)
        {
            this.Token = token;
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public TContent Token { get; }

        #endregion
    }
}
