// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Extensions
{
    using Democrite.Framework.Toolbox.Extensions.Types;

    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Extend task
    /// </summary>
    public static class TaskExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, MethodInfo> s_taskFromResultCache;
        private static readonly ReaderWriterLockSlim s_taskFromLocker;
        private static readonly MethodInfo s_taskFromResultMthd;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize the class <see cref="TaskExtensions"/>
        /// </summary>
        static TaskExtensions()
        {
            s_taskFromResultMthd = typeof(Task).GetMethod(nameof(Task.FromResult), BindingFlags.Static | BindingFlags.Public) ?? throw new ArgumentNullException("Missing " + nameof(Task.FromResult));

            s_taskFromResultCache = new Dictionary<Type, MethodInfo>();
            s_taskFromLocker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets task result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult? GetResult<TResult>(this Task task)
        {
            ArgumentNullException.ThrowIfNull(task);

            if (task is Task<TResult> taskResult)
                return taskResult.GetAwaiter().GetResult();

            return (TResult?)GetResult(task);
        }

        /// <summary>
        /// Gets task result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetResult(this Task inst)
        {
            ArgumentNullException.ThrowIfNull(inst);

            var typeInfo = inst.GetType().GetTypeIntoExtension()!;

            return typeInfo.GetSpecifcTypeExtend<ITaskTypeInfoEnhancer>().GetResult(inst);
        }

        /// <summary>
        /// Gets <see cref="Task"/> from any <see cref="ValueTask"/> or <see cref="ValueTask{TResult}"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetTaskFromAnyValueTask(this ITypeInfoExtension? valueTaskInfo, object? inst)
        {
            if (valueTaskInfo is null)
                return Task.CompletedTask;

            if (!valueTaskInfo.IsValueTask)
                throw new InvalidCastException("Must only target value task");

            return valueTaskInfo.GetSpecifcTypeExtend<IValueTaskTypeInfoEnhancer>().AsTask(inst);
        }

        /// <summary>
        /// Gets the task result from Task.FromResult .
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetTaskFrom(this object? data, Type? forceType = null)
        {
            var type = forceType ?? data?.GetType() ?? throw new InvalidCastException("TaskExtensions.GetTaskFrom : No type could be detected plz enter one manually");
            var taskFromBuiler = s_taskFromResultMthd;

            s_taskFromLocker.EnterReadLock();
            try
            {
                if (s_taskFromResultCache.TryGetValue(type, out var result))
                    taskFromBuiler = result;
            }
            finally
            {
                s_taskFromLocker.ExitReadLock();
            }

            if (taskFromBuiler == null || taskFromBuiler == s_taskFromResultMthd)
            {
                s_taskFromLocker.EnterWriteLock();
                try
                {
                    taskFromBuiler = s_taskFromResultMthd.MakeGenericMethod(type);

                    if (!s_taskFromResultCache.ContainsKey(type))
                        s_taskFromResultCache.Add(type, taskFromBuiler);
                }
                finally
                {
                    s_taskFromLocker.ExitWriteLock();
                }
            }

            var task = (Task?)taskFromBuiler.Invoke(null, new[] { data });
            Debug.Assert(task != null);
            return task;
        }

        #endregion
    }
}
