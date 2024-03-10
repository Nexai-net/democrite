// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Helpers
{
    using Democrite.Framework.Toolbox.Helpers;

    using NFluent;

    using System;
    using System.Diagnostics;

    /// <summary>
    /// Test <see cref="CancellationHelper"/> functionalities
    /// </summary>
    public sealed class CancellationHelperUnitTests
    {
        /// <summary>
        /// Test that <see cref="CancellationHelper.SingleAccessScope"/> allow only one scope to run at same time to automatically cancel the previous one
        /// </summary>
        [Fact]
        public void CancellationHelper_SingleScopeAccess()
        {
            CancellationTokenSource? source = null;

            var callCount = 0;

            using (var locker = new SemaphoreSlim(1))
            {
                var getAccessor = new Func<CancellationTokenSource?>(() =>
                {
                    // Ensure that locker is lock
                    Check.That(locker.CurrentCount).IsEqualTo(0);
                    callCount++;

                    return source;
                });

                var setAccessor = new Action<CancellationTokenSource>((s) =>
                {
                    // Ensure that locker is lock
                    Check.That(locker.CurrentCount).IsEqualTo(0);

                    callCount++;
                    source = s;
                });

                using (var scope = CancellationHelper.SingleAccessScope(locker, getAccessor, setAccessor))
                {
                    using (var secondScope = CancellationHelper.SingleAccessScope(locker, getAccessor, setAccessor))
                    {
                        // Check that is a second scope is open on the same code section the first one is cancelled
                        Check.That(scope.Content.IsCancellationRequested).IsTrue();
                        Check.That(secondScope.Content.IsCancellationRequested).IsFalse();

                        Check.That(scope.Content).Not.IsSameReferenceAs(secondScope.Content);
                    }
                }

                Check.That(callCount).IsEqualTo(4);
            }
        }

        /// <summary>
        /// Test that <see cref="CancellationHelper.SingleAccessScope"/> allow only one scope to run at same time to automatically cancel the previous one
        /// USING a <see cref="CancellationContext"/>
        /// </summary>
        [Fact]
        public void CancellationHelper_And_CancellationContext_SingleScopeAccess()
        {
            using (var locker = new SemaphoreSlim(1))
            {
                using (var ctx = new CancellationContext())
                {
                    using (var scope = ctx.Lock())
                    {
                        using (var secondScope = ctx.Lock())
                        {
                            // Check that is a second scope is open on the same code section the first one is cancelled
                            Check.That(scope.Content.IsCancellationRequested).IsTrue();
                            Check.That(secondScope.Content.IsCancellationRequested).IsFalse();

                            Check.That(scope.Content).Not.IsSameReferenceAs(secondScope.Content);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test <see cref="CancellationHelper.Timeout(TimeSpan?)"/> correcly cancel the token after the time asked
        /// </summary>
        [Fact(Timeout = 5000)] // Security test MUST exist normally on the time setups
        public async Task CancellationHelper_Timeout()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            Check.ThatCode(async () => await Task.Delay(Timeout.Infinite, CancellationHelper.Timeout(TimeSpan.FromMilliseconds(1_000)))).Throws<OperationCanceledException>();
            stopwatch.Stop();

            // Mainly if it got to this line the cancellation work
            Check.That(stopwatch.ElapsedMilliseconds).IsGreaterOrEqualThan(900).And.IsLessOrEqualThan(4_000);

            await Task.CompletedTask;
        }
    }
}
