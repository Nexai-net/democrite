// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Proxies
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Proxy used to call on dispatcher
    /// </summary>
    public interface IDispatcherProxy
    {
        #region Methods

        /// <summary>
        /// Sends <paramref name="callback"/> to be execute by the dispatcher without waiting the execution
        /// </summary>
        void Send(Action callback);

        /// <summary>
        /// Relay exception through dispatch context.
        /// </summary>
        void Throw(Exception ex, [CallerMemberName] string? callerMemberName = null);

        #endregion
    }
}
