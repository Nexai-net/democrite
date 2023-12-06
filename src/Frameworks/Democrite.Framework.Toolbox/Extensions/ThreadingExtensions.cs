// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Helpers;

    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extensions enhanced threading manipulation
    /// </summary>
    public static class ThreadingExtensions
    {
        #region Fields

        private static readonly TimeSpan s_defaultTimeout = TimeSpan.FromSeconds(10);

        #endregion

        #region Methods

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this Mutex locker)
        {
            return Lock(locker, s_defaultTimeout);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this Mutex locker, TimeSpan timeout)
        {
            return Lock(locker, CancellationHelper.Timeout(timeout));
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this Mutex locker, CancellationToken token)
        {
            bool locked = locker.WaitOne();
            return new DisposableAction<Mutex>(static s => s.ReleaseMutex(), locker);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this SemaphoreSlim locker)
        {
            return Lock(locker, s_defaultTimeout);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this SemaphoreSlim locker, TimeSpan timeout)
        {
            return Lock(locker, CancellationHelper.Timeout(timeout));
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this SemaphoreSlim locker, CancellationToken token)
        {
            locker.Wait(token);
            return new DisposableAction<SemaphoreSlim>(static s => s.Release(), locker);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this Semaphore locker)
        {
            return Lock(locker, s_defaultTimeout);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this Semaphore locker, TimeSpan timeout)
        {
            return Lock(locker, CancellationHelper.Timeout(timeout));
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable Lock(this Semaphore locker, CancellationToken token)
        {
            bool hasLocked = false;

            do
            {
                token.ThrowIfCancellationRequested();
                hasLocked = locker.WaitOne(100);
                token.ThrowIfCancellationRequested();

            } while (!hasLocked);

            return new DisposableAction<Semaphore>(static s => s.Release(), locker);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static Task<IDisposable> LockAsync(this SemaphoreSlim locker)
        {
            return LockAsync(locker, s_defaultTimeout);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static Task<IDisposable> LockAsync(this SemaphoreSlim locker, TimeSpan timeout)
        {
            return LockAsync(locker, CancellationHelper.Timeout(timeout));
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static async Task<IDisposable> LockAsync(this SemaphoreSlim locker, CancellationToken token)
        {
            var lockResult = await locker.WaitAsync(Timeout.Infinite, token);

            Debug.Assert(lockResult, "SemaphoreSlim - LockAsync FAILED");

            return new DisposableAction<SemaphoreSlim>(static s => s.Release(), locker);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable LockRead(this ReaderWriterLockSlim locker)
        {
            locker.EnterReadLock();
            return new DisposableAction<ReaderWriterLockSlim>(static (r) => r.ExitReadLock(), locker);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        public static IDisposable LockWrite(this ReaderWriterLockSlim locker)
        {
            locker.EnterWriteLock();
            return new DisposableAction<ReaderWriterLockSlim>(static (l) => l.ExitWriteLock(), locker);
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeScopeRead(this ReaderWriterLockSlim locker, Action action)
        {
            locker.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        /// <summary>
        /// lock using ScopeLock pattern's implementation 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeScopeWrite(this ReaderWriterLockSlim locker, Action action)
        {
            locker.EnterWriteLock();
            try
            {
                action();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        #endregion
    }
}
