// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Deferred
{
    using System.Threading.Tasks;

    /// <summary>
    /// Observer used to be notify on Deferred work changed
    /// </summary>
    /// <seealso cref="IGrainObserver" />
    public interface IDeferredObserver : IGrainObserver
    {
        /// <summary>
        /// Receive a notification to inform a deferred work change
        /// </summary>
        Task DeferredStatusChangedNotification(DeferredStatusMessage statusChangeMessage);
    }
}
