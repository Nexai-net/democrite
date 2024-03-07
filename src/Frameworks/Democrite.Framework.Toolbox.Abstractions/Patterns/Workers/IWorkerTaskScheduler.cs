// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Patterns.Workers
{
    using Democrite.Framework.Toolbox.Abstractions.Disposables;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Pool that managed a limiter quantity of worker that can perform task in simultanious
    /// </summary>
    public interface IWorkerTaskScheduler : ISafeAsyncDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the scheduler u identifier.
        /// </summary>
        Guid SchedulerUId { get; }

        /// <summary>
        /// Gets the maximum concurrency level.
        /// </summary>
        uint MaximumConcurrencyLevel { get; }

        /// <summary>
        /// Gets the number of task pending to be process
        /// </summary>
        ulong TaskPending { get; }

        /// <summary>
        /// Gets the number of task processing
        /// </summary>
        ulong TaskProcessing { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the maximum concurrency level.
        /// </summary>
        void ChangeMaximumConcurrencyLevel(uint maximumConcurrencyLevel);

        /// <summary>
        /// Enqueues the task to process
        /// </summary>
        void PushTask(Func<CancellationToken, Task> task, TaskScheduler? taskScheduler = null);

        /// <summary>
        /// Enqueues the task to process
        /// </summary>
        void PushTask<TInput>(Func<TInput?, CancellationToken, Task> task, TInput? input, TaskScheduler? taskScheduler = null);

        /// <summary>
        /// Enqueues the task to process and return a task to wait the result h.
        /// </summary>
        void ExecTask(Func<CancellationToken, Task> task, TaskCompletionSource? resultTask, CancellationToken? waitingCancellationToken = null, TaskScheduler? taskScheduler = null);

        /// <summary>
        /// Enqueues the task to process and return a task to wait the result h.
        /// </summary>
        void ExecTask<TInput>(Func<TInput?, CancellationToken, Task> task, TInput? input, TaskCompletionSource? resultTask, CancellationToken? waitingCancellationToken = null, TaskScheduler? taskScheduler = null);

        /// <summary>
        /// Pushes the task when theire is an available slot.
        /// </summary>
        /// <remarks>
        ///     The task is push on if theire is an available slot based on <<see cref="MaximumConcurrencyLevel"/>
        /// </remarks>
        /// <returns>
        ///     Return a task to wait task to be push.
        /// </returns>
        Task PushTaskWhenAvailableSlotAsync<TInput>(Func<TInput?, CancellationToken, Task> task, TInput? input, CancellationToken token, TaskScheduler? taskScheduler = null);

        /// <summary>
        /// Pushes the task when theire is an available slot.
        /// </summary>
        /// <remarks>
        ///     The task is push on if theire is an available slot based on <<see cref="MaximumConcurrencyLevel"/>
        /// </remarks>
        /// <returns>
        ///     Return a task to wait task to be push.
        /// </returns>
        Task PushTaskWhenAvailableSlotAsync(Func<CancellationToken, Task> task, CancellationToken token, TaskScheduler? taskScheduler = null);

        /// <summary>
        /// Wait all task push have been correctly process
        /// </summary>
        Task FlushAsync(CancellationToken token);

        #endregion
    }
}
