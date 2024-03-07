// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Patterns.Workers
{
    using Democrite.Framework.Toolbox.Extensions;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Patterns.Workers;

    using NFluent;

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    /*
     * Test are split in multiple class to allow XUNIT to perform parallel test
     */

    /// <summary>
    /// Test for <see cref="WorkerTaskScheduler"/>
    /// </summary>
    public abstract class WorkerTaskSchedulerUTests
    {
        protected async Task Concurrent_push_Execution_ImplAsync(uint maxConcurrent, int taskQuantity)
        {
            var count = 0;
            var maxProcessingConcurrent = int.MinValue;
            var concurrentBag = new ConcurrentBag<int>();

            using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(20)))
            await using (var worker = new WorkerTaskScheduler(maxConcurrent))
            {
                foreach (var i in Enumerable.Range(0, taskQuantity))
                {
                    worker.PushTask(async (indx, token) =>
                                    {
                                        token.ThrowIfCancellationRequested();

                                        await Task.Delay(TimeSpan.FromMilliseconds((Random.Shared.NextDouble() * 3)), token);
                                        Interlocked.Increment(ref count);
                                        concurrentBag.Add(indx);

                                        lock (worker)
                                        {
                                            maxProcessingConcurrent = Math.Max((int)worker.TaskProcessing, maxProcessingConcurrent);
                                        }
                                    }, i);
                }

                await worker.FlushAsync(timeout.Content);
            }

            var concurrentUniqueValues = concurrentBag.Distinct().OrderBy(o => o).ToArray();

            Check.That(count).IsEqualTo(taskQuantity);
            Check.That(concurrentUniqueValues.Length).IsEqualTo(taskQuantity);
            Check.That(maxProcessingConcurrent).IsLessOrEqualThan((int)maxConcurrent);
        }

        protected async Task Concurrent_push_Execution_withoutarg_ImplAsync(uint maxConcurrent, int taskQuantity)
        {
            var count = 0;
            var maxProcessingConcurrent = int.MinValue;

            using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(20)))
            await using (var worker = new WorkerTaskScheduler(maxConcurrent))
            {
                foreach (var i in Enumerable.Range(0, taskQuantity))
                {
                    worker.PushTask(async (token) =>
                    {
                        token.ThrowIfCancellationRequested();

                        await Task.Delay(TimeSpan.FromMilliseconds((Random.Shared.NextDouble() * 3)), token);
                        Interlocked.Increment(ref count);

                        lock (worker)
                        {
                            maxProcessingConcurrent = Math.Max((int)worker.TaskProcessing, maxProcessingConcurrent);
                        }
                    });
                }

                await worker.FlushAsync(timeout.Content);
            }

            Check.That(count).IsEqualTo(taskQuantity);
            Check.That(maxProcessingConcurrent).IsLessOrEqualThan((int)maxConcurrent);
        }

        protected async Task Concurrent_ExecTask_WaitExecuted_ImplAsync(uint maxConcurrent, int taskQuantity)
        {
            var count = 0;
            var maxProcessingConcurrent = int.MinValue;
            var concurrentBag = new ConcurrentBag<int>();

            using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(20)))
            await using (var worker = new WorkerTaskScheduler(maxConcurrent))
            {
                var tasks = new List<TaskCompletionSource>(taskQuantity);
                foreach (var i in Enumerable.Range(0, taskQuantity))
                {
                    var completion = new TaskCompletionSource();
                    worker.ExecTask(async (indx, token) =>
                    {
                        timeout.Content.ThrowIfCancellationRequested();

                        await Task.Delay(TimeSpan.FromMilliseconds((Random.Shared.NextDouble() * 3)), timeout.Content);
                        Interlocked.Increment(ref count);
                        concurrentBag.Add(indx);

                        lock (worker)
                        {
                            maxProcessingConcurrent = Math.Max((int)worker.TaskProcessing, maxProcessingConcurrent);
                        }
                    }, i, completion);

                    tasks.Add(completion);
                }

                await worker.FlushAsync(timeout.Content);
                await tasks.Select(t => t.Task).ToArray().SafeWhenAllAsync(timeout.Content);
            }

            var concurrentUniqueValues = concurrentBag.Distinct().OrderBy(o => o).ToArray();

            Check.That(count).IsEqualTo(taskQuantity);
            Check.That(concurrentUniqueValues.Length).IsEqualTo(taskQuantity);
            Check.That(maxProcessingConcurrent).IsLessOrEqualThan((int)maxConcurrent);
        }

        protected async Task Concurrent_ExecTask_WaitExecuted_WithoutArg_ImplAsync(uint maxConcurrent, int taskQuantity)
        {
            var count = 0;
            var maxProcessingConcurrent = int.MinValue;

            using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(20)))
            await using (var worker = new WorkerTaskScheduler(maxConcurrent))
            {
                var tasks = new List<TaskCompletionSource>(taskQuantity);
                foreach (var i in Enumerable.Range(0, taskQuantity))
                {
                    var completion = new TaskCompletionSource();
                    worker.ExecTask(async (token) =>
                    {
                        token.ThrowIfCancellationRequested();

                        await Task.Delay(TimeSpan.FromMilliseconds((Random.Shared.NextDouble() * 3)), token);
                        Interlocked.Increment(ref count);

                        lock (worker)
                        {
                            maxProcessingConcurrent = Math.Max((int)worker.TaskProcessing, maxProcessingConcurrent);
                        }
                    }, completion);

                    tasks.Add(completion);
                }

                await worker.FlushAsync(timeout.Content);
                await tasks.Select(t => t.Task).ToArray().SafeWhenAllAsync(timeout.Content);
            }

            Check.That(count).IsEqualTo(taskQuantity);
            Check.That(maxProcessingConcurrent).IsLessOrEqualThan((int)maxConcurrent);
        }

        protected async Task Concurrent_push_wait_availableslot_Execution_ImplAsync(uint maxConcurrent, int taskQuantity)
        {
            var count = 0;
            var maxProcessingConcurrent = int.MinValue;
            var maxWaitingConcurrent = int.MinValue;
            var concurrentBag = new ConcurrentBag<int>();

            using (var timeout = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(40)))
            await using (var worker = new WorkerTaskScheduler(maxConcurrent))
            {
                var watch = new Stopwatch();

                foreach (var i in Enumerable.Range(0, taskQuantity))
                {
                    watch.Start();

                    await worker.PushTaskWhenAvailableSlotAsync(async (indx, token) =>
                    {
                        token.ThrowIfCancellationRequested();

                        await Task.Delay(TimeSpan.FromMilliseconds((Random.Shared.NextDouble() * 3)), token);
                        Interlocked.Increment(ref count);
                        concurrentBag.Add(indx);

                        lock (worker)
                        {
                            maxProcessingConcurrent = Math.Max((int)worker.TaskProcessing, maxProcessingConcurrent);
                            maxWaitingConcurrent = Math.Max((int)worker.TaskPending, maxWaitingConcurrent);
                        }
                    }, i, timeout.Content);

                    watch.Stop();
                }

                await worker.FlushAsync(timeout.Content);
            }

            var concurrentUniqueValues = concurrentBag.Distinct().OrderBy(o => o).ToArray();

            Check.That(count).IsEqualTo(taskQuantity);
            Check.That(concurrentUniqueValues.Length).IsEqualTo(taskQuantity);
            Check.That(maxProcessingConcurrent).IsLessOrEqualThan((int)maxConcurrent);
            Check.That(maxWaitingConcurrent).IsLessOrEqualThan((int)maxConcurrent);
        }
    }

    [Trait("Type", nameof(WorkerTaskScheduler))]
    public sealed class WorkerTaskSchedulerUTestsGeneric : WorkerTaskSchedulerUTests
    {
        [Fact]
        public async Task Create()
        {
            uint maxConcurrent = (uint)Random.Shared.Next(1, 50);
            await using (var worker = new WorkerTaskScheduler(maxConcurrent))
            {
                Check.That(worker).IsNotNull();
                Check.That(worker.TaskPending).IsEqualTo(0);
                Check.That(worker.TaskProcessing).IsEqualTo(0);
                Check.That(worker.MaximumConcurrencyLevel).IsEqualTo(maxConcurrent);
            }
        }

        [Fact]
        public async Task Create_With_Invalid_MaxConcurrent()
        {
            var autoMaxConcurrent = (uint)(Environment.ProcessorCount / 3.0);

            await using (var worker = new WorkerTaskScheduler(0))
            {
                Check.That(worker).IsNotNull();
                Check.That(worker.TaskPending).IsEqualTo(0);
                Check.That(worker.TaskProcessing).IsEqualTo(0);
                Check.That(worker.MaximumConcurrencyLevel).IsEqualTo(autoMaxConcurrent);
            }
        }
    }

    [Trait("Type", nameof(WorkerTaskScheduler))]
    public sealed class WorkerTaskSchedulerUTestsSimplePush : WorkerTaskSchedulerUTests
    {
        [Theory()]
        [InlineData(1, 1_000)]
        [InlineData(5, 1_000)]
        [InlineData(15, 1_000)]
        public async Task Concurrent_push_ExecutionAsync(uint maxConcurrent, int taskQuantity)
        {
            await Concurrent_push_Execution_ImplAsync(maxConcurrent, taskQuantity);
        }
    }

    [Trait("Type", nameof(WorkerTaskScheduler))]
    public sealed class WorkerTaskSchedulerUTestsSimplePushWithoutarg : WorkerTaskSchedulerUTests
    {
        [Theory()]
        [InlineData(1, 1_000)]
        [InlineData(5, 1_000)]
        [InlineData(15, 1_000)]
        public async Task Concurrent_push_Execution_withoutarg_Async(uint maxConcurrent, int taskQuantity)
        {
            await Concurrent_push_Execution_withoutarg_ImplAsync(maxConcurrent, taskQuantity);
        }
    }

    [Trait("Type", nameof(WorkerTaskScheduler))]
    public sealed class WorkerTaskSchedulerUTestsSimpleExec : WorkerTaskSchedulerUTests
    {
        [Theory()]
        [InlineData(1, 1_000)]
        [InlineData(5, 1_000)]
        [InlineData(15, 1_000)]
        public async Task Concurrent_exec_Execution_Async(uint maxConcurrent, int taskQuantity)
        {
            await Concurrent_ExecTask_WaitExecuted_ImplAsync(maxConcurrent, taskQuantity);
        }
    }

    [Trait("Type", nameof(WorkerTaskScheduler))]
    public sealed class WorkerTaskSchedulerUTestsSimpleExecWithoutArg : WorkerTaskSchedulerUTests
    {
        [Theory()]
        [InlineData(1, 1_000)]
        [InlineData(5, 1_000)]
        [InlineData(15, 1_000)]
        public async Task Concurrent_exec_Execution_withoutArg_Async(uint maxConcurrent, int taskQuantity)
        {
            await Concurrent_ExecTask_WaitExecuted_WithoutArg_ImplAsync(maxConcurrent, taskQuantity);
        }
    }

    [Trait("Type", nameof(WorkerTaskScheduler))]
    public sealed class WorkerTaskSchedulerUTestsSimplePushAvailable: WorkerTaskSchedulerUTests
    {
        [Theory()]
        [InlineData(1, 1_000)]
        [InlineData(5, 1_000)]
        [InlineData(15, 1_000)]
        public async Task Concurrent_push_wait_availableslot_Execution_Async(uint maxConcurrent, int taskQuantity)
        {
            await Concurrent_push_wait_availableslot_Execution_ImplAsync(maxConcurrent, taskQuantity);
        }
    }
}
