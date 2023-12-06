// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Proxies
{
    using Democrite.Framework.Toolbox.Abstractions.Proxies;

    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Proxy that apply direct call used to provide <see cref="IDispatcherProxy"/> when no dispatcher is provide or setup
    /// </summary>
    /// <seealso cref="IDispatcherProxy" />
    public sealed class NullDispatcherProxy : IDispatcherProxy
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NullDispatcherProxy"/> class.
        /// </summary>
        static NullDispatcherProxy()
        {
            Instance = new NullDispatcherProxy();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NullDispatcherProxy"/> class from being created.
        /// </summary>
        private NullDispatcherProxy()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton stateless instance.
        /// </summary>
        public static IDispatcherProxy Instance { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Send(Action callback)
        {
            callback();
        }

        /// <inheritdoc />
        public void Throw(Exception ex, [CallerMemberName] string? callerMemberName = null)
        {
            throw ex;
        }

        #endregion
    }
}
