// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Patterns.Workers
{
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Workers;
    using Democrite.Framework.Toolbox.Disposables;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;

    /// <summary>
    /// Provider used to get scheduler record by id
    /// </summary>
    /// <seealso cref="IWorkerTaskSchedulerProvider" />
    public class WorkerTaskSchedulerProvider : SafeDisposable, IWorkerTaskSchedulerProvider
    {
        #region Fields

        private readonly Dictionary<Guid, IWorkerTaskScheduler> _cache;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ReaderWriterLockSlim _locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerTaskSchedulerProvider"/> class.
        /// </summary>
        public WorkerTaskSchedulerProvider(ILoggerFactory? loggerFactory = null)
        {
            this._locker = new ReaderWriterLockSlim();
            this._cache = new Dictionary<Guid, IWorkerTaskScheduler>();
            this._loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IWorkerTaskScheduler GetWorkerScheduler(Guid workerId, uint maxConcurrent = 0, ILogger? logger = null)
        {
            var scheduler = GetWorkerSchedulerImpl(workerId, maxConcurrent, logger);

            if (scheduler.MaximumConcurrencyLevel != maxConcurrent)
                scheduler.ChangeMaximumConcurrencyLevel(maxConcurrent);

            return scheduler;
        }

        /// <inheritdoc cref="IWorkerTaskSchedulerProvider.GetWorkerScheduler(Guid, uint)" />
        private IWorkerTaskScheduler GetWorkerSchedulerImpl(Guid workerId, uint maxConcurrent, ILogger? logger)
        {
            this._locker.EnterReadLock();
            try
            {
                if (this._cache.TryGetValue(workerId, out var scheduler) && !scheduler.IsDisposed)
                    return scheduler;
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            this._locker.EnterWriteLock();
            try
            {
                if (this._cache.TryGetValue(workerId, out var scheduler))
                {
                    if (!scheduler.IsDisposed)
                        return scheduler;

                    var disposableWorkers = this._cache.Where(kv => kv.Value.IsDisposed).ToArray();
                    foreach (var key in disposableWorkers)
                        this._cache.Remove(key.Key);
                }

                var newScheduler = BuildScheduler(workerId, maxConcurrent, logger);

                this._cache.Add(workerId, newScheduler);
                return newScheduler;
            }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Builds a new dedicated scheduler.
        /// </summary>
        protected virtual IWorkerTaskScheduler BuildScheduler(Guid workerId, uint maxConcurrent, ILogger? logger)
        {
            return new WorkerTaskScheduler(maxConcurrent, workerId, logger ?? this._loggerFactory.CreateLogger(workerId.ToString()));
        }

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            this._locker.EnterWriteLock();
            try
            {
                this._cache.Clear();
            }
            finally
            {
                this._locker.ExitWriteLock();
            }
            base.DisposeBegin();
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            this._locker.Dispose();
            base.DisposeEnd();
        }

        #endregion
    }
}
