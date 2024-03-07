// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Patterns.Workers
{
    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Provder used to give <see cref="IPoolWorker"/>
    /// </summary>
    public interface IWorkerTaskSchedulerProvider
    {
        /// <summary>
        /// Gets or create the worker scheduler.
        /// </summary>
        IWorkerTaskScheduler GetWorkerScheduler(Guid workerId, uint maxConcurrent = 0, ILogger? logger = null);
    }
}
