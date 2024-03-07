// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Patterns.Workers
{
    using Democrite.Framework.Toolbox.Patterns.Workers;

    using NFluent;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="WorkerTaskSchedulerProvider"/>
    /// </summary>
    [Trait("Type", nameof(WorkerTaskSchedulerProvider))]
    public sealed class WorkerTaskSchedulerProviderUTests
    {
        [Fact]
        public async Task Create_New()
        {
            var workerId = Guid.NewGuid();
            using (var provider = new WorkerTaskSchedulerProvider())
            {
                await using (var scheduler = provider.GetWorkerScheduler(workerId, 42))
                {
                    Check.That(scheduler).IsNotNull();
                    Check.That(scheduler.SchedulerUId).IsEqualTo(workerId);
                    Check.That(scheduler.MaximumConcurrencyLevel).IsEqualTo(42);
                }
            }
        }

        [Fact]
        public async Task Create_And_Retreive()
        {
            var workerId = Guid.NewGuid();
            using (var provider = new WorkerTaskSchedulerProvider())
            {
                await using (var scheduler = provider.GetWorkerScheduler(workerId, 42))
                {
                    Check.That(scheduler).IsNotNull();
                    Check.That(scheduler.SchedulerUId).IsEqualTo(workerId);
                    Check.That(scheduler.MaximumConcurrencyLevel).IsEqualTo(42);

                    var newsScheduler = provider.GetWorkerScheduler(workerId, 42);

                    Check.That(newsScheduler).IsNotNull();
                    Check.That(newsScheduler).IsSameReferenceAs(scheduler);
                }
            }
        }

        [Fact]
        public async Task Create_And_Retreive_Change_Max()
        {
            var workerId = Guid.NewGuid();
            using (var provider = new WorkerTaskSchedulerProvider())
            {
                await using (var scheduler = provider.GetWorkerScheduler(workerId, 42))
                {
                    Check.That(scheduler).IsNotNull();
                    Check.That(scheduler.SchedulerUId).IsEqualTo(workerId);
                    Check.That(scheduler.MaximumConcurrencyLevel).IsEqualTo(42);

                    var newsScheduler = provider.GetWorkerScheduler(workerId, 24);

                    Check.That(newsScheduler).IsNotNull();
                    Check.That(newsScheduler).IsSameReferenceAs(scheduler);
                    Check.That(scheduler.MaximumConcurrencyLevel).IsEqualTo(24);
                    Check.That(newsScheduler.MaximumConcurrencyLevel).IsEqualTo(24);
                }
            }
        }

        [Fact]
        public async Task Create_And_Recreate_After_Dispose()
        {
            var workerId = Guid.NewGuid();
            using (var provider = new WorkerTaskSchedulerProvider())
            {
                var scheduler = provider.GetWorkerScheduler(workerId, 42);
                
                Check.That(scheduler).IsNotNull();
                Check.That(scheduler.SchedulerUId).IsEqualTo(workerId);
                Check.That(scheduler.MaximumConcurrencyLevel).IsEqualTo(42);

                var newsScheduler = provider.GetWorkerScheduler(workerId, 42);

                Check.That(newsScheduler).IsNotNull();
                Check.That(newsScheduler).IsSameReferenceAs(scheduler);

                await scheduler.DisposeAsync();

                await using (var renewScheduler = provider.GetWorkerScheduler(workerId, 42))
                {
                    Check.That(renewScheduler).IsNotNull();
                    Check.That(renewScheduler.SchedulerUId).IsEqualTo(workerId);
                    Check.That(renewScheduler.MaximumConcurrencyLevel).IsEqualTo(42);
                    Check.That(renewScheduler).Not.IsSameReferenceAs(scheduler);
                }
            }
        }
    }
}
