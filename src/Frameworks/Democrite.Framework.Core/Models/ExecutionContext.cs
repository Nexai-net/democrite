// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Models
{
    using Democrite.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Threading;

    /// <summary>
    /// Define an execution context
    /// </summary>
    /// <seealso cref="IExecutionContext" />
    [Immutable]
    [Serializable]
    [ImmutableObject(true)]
    public class ExecutionContext : IExecutionContext
    {
        #region Fields

        private static readonly Dictionary<Type, FieldInfo> s_grainReferenceRuntimeCache;
        private static readonly ReaderWriterLockSlim s_grainReferenceRuntimeCacheLocker;

        private static readonly Dictionary<Type, MethodInfo> s_specializedGenericMthd;
        private static readonly ReaderWriterLockSlim s_rwMethodCache;
        private static readonly MethodInfo s_duplicateGenericMthd;

        private static readonly MethodInfo s_addGrainCancelReference;
        private static readonly PropertyInfo s_propGrainRuntime;

        [NonSerialized]
        private readonly GrainCancellationTokenSource _grainCancelTokenSource;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExecutionContext"/> class.
        /// </summary>
        static ExecutionContext()
        {
            s_grainReferenceRuntimeCache = new Dictionary<Type, FieldInfo>();
            s_specializedGenericMthd = new Dictionary<Type, MethodInfo>();

            s_duplicateGenericMthd = typeof(ExecutionContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                             .Single(m => m.Name == nameof(DuplicateWithContext) && m.IsGenericMethodDefinition);

            var propGrainRuntime = typeof(GrainReference).GetProperty("Runtime", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
            Debug.Assert(propGrainRuntime != null);
            s_propGrainRuntime = propGrainRuntime;

            var addGrainCancelReference = typeof(GrainCancellationToken).GetMethod("AddGrainReference", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(addGrainCancelReference != null);
            s_addGrainCancelReference = addGrainCancelReference;

            s_rwMethodCache = new ReaderWriterLockSlim();
            s_grainReferenceRuntimeCacheLocker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ExecutionContext"/> class from being created.
        /// </summary>
        public ExecutionContext(Guid flowUID,
                                Guid currentExecutionId,
                                Guid? parentExecutionId)
        {
            this._grainCancelTokenSource = new GrainCancellationTokenSource();

            this.FlowUID = flowUID;
            this.CurrentExecutionId = currentExecutionId;
            this.ParentExecutionId = parentExecutionId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContext"/> class.
        /// </summary>
        /// <remarks>
        ///     Used to create a linked context
        /// </remarks>
        private ExecutionContext(Guid flowUID,
                                 Guid currentExecutionId,
                                 Guid? parentExecutionId,
                                 GrainCancellationTokenSource grainCancellationTokenSource)
        {
            this._grainCancelTokenSource = grainCancellationTokenSource;
            this.FlowUID = flowUID;
            this.CurrentExecutionId = currentExecutionId;
            this.ParentExecutionId = parentExecutionId;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Guid FlowUID { get; }

        /// <inheritdoc />
        public Guid CurrentExecutionId { get; }

        /// <inheritdoc />
        [IgnoreDataMember]
        public CancellationToken CancellationToken
        {
            get { return this._grainCancelTokenSource.Token.CancellationToken; }
        }

        /// <inheritdoc />
        public Guid? ParentExecutionId { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ILogger GetLogger<T>(ILoggerProvider loggerProvider)
            where T : IVGrain
        {
            lock (this)
            {
                return GetLogger<T>(loggerProvider, this.FlowUID, this.CurrentExecutionId);
            }
        }

        /// <inheritdoc />
        public IExecutionContext NextContext()
        {
            return new ExecutionContext(this.FlowUID,
                                        Guid.NewGuid(),
                                        this.CurrentExecutionId,
                                        this._grainCancelTokenSource);
        }

        /// <inheritdoc />
        public IExecutionContext<TContextInfo> DuplicateWithContext<TContextInfo>(TContextInfo contextInfo)
        {
            return new ExecutionContextWithConfiguration<TContextInfo>(this.FlowUID, this.CurrentExecutionId, this.ParentExecutionId, contextInfo);
        }

        /// <inheritdoc />
        public IExecutionContext DuplicateWithContext(object? contextInfo, Type contextType)
        {
            MethodInfo? genericMthd = null;

            s_rwMethodCache.EnterReadLock();
            try
            {
                s_specializedGenericMthd.TryGetValue(contextType, out genericMthd);
            }
            finally
            {
                s_rwMethodCache.ExitReadLock();
            }

            if (genericMthd == null)
            {
                s_rwMethodCache.EnterWriteLock();
                try
                {
                    genericMthd = s_duplicateGenericMthd.MakeGenericMethod(contextType);

                    if (!s_specializedGenericMthd.ContainsKey(contextType))
                        s_specializedGenericMthd.Add(contextType, genericMthd);
                }
                finally
                {
                    s_rwMethodCache.ExitWriteLock();
                }
            }

            return (IExecutionContext)(genericMthd?.Invoke(this, new object?[] { contextInfo }) ?? throw new InvalidOperationException("Invalid context cast"));
        }

        #region Tools

        /// <inheritdoc cref="IExecutionContext.GetLogger{T}(ILoggerProvider)" />
        private static ILogger GetLogger<T>(ILoggerProvider loggerProvider, Guid flowUid, Guid currentExecutionId)
            where T : IVGrain
        {
            return loggerProvider.CreateLogger(string.Format("[Flow {0}][VGrain {1} - {2}]", flowUid, currentExecutionId, typeof(T).Name));
        }

        /// <inheritdoc />
        public void Cancel()
        {
            this._grainCancelTokenSource.Cancel();
        }

        /// <summary>
        /// Link vgrain invoked to global cancel system
        /// </summary>
        /// <remarks>
        ///
        ///  By default orlean manage automatically the cancellation token track using
        ///  GrainCancellationToken in method parameters
        ///  
        ///  In the goal to minimize the impact and knowlegde of orlean in the user code
        ///  We decide to used the IExecutionContext to carry the CancellationToken
        ///  
        ///  but to use orlean cancel hierarchy track we have to manually reproduce the
        ///  behavior contain in GrainReferenceRuntime.SetGrainCancellationTokensTarget(GrainReference target, IInvokable request)
        ///  
        ///  grainToken.AddGrainReference(cancellationTokenRuntime, target);
        /// 
        /// </remarks>
        internal void AddCancelGrainReference(IGrainContext grainToAttach)
        {
            var grainReferenceRuntime = (IGrainReferenceRuntime)s_propGrainRuntime.GetValue(grainToAttach.GrainReference)!;

            var cancellationTokenRuntime = GetCancellationTokenRuntimeFromCache(grainReferenceRuntime);

            s_addGrainCancelReference.Invoke(this._grainCancelTokenSource.Token,
                                             new object?[]
                                             {
                                                 cancellationTokenRuntime.GetValue(grainReferenceRuntime),
                                                 grainToAttach.GrainReference
                                             });
        }

        /// <summary>
        /// Gets the cancellation token runtime field info from cache or build data.
        /// </summary>
        private static FieldInfo GetCancellationTokenRuntimeFromCache(IGrainReferenceRuntime grainReferenceRuntime)
        {
            var traitRuntime = grainReferenceRuntime.GetType();

            s_grainReferenceRuntimeCacheLocker.EnterReadLock();
            try
            {
                if (s_grainReferenceRuntimeCache.TryGetValue(traitRuntime, out var fieldICachenfo))
                {
                    Debug.Assert(fieldICachenfo != null);
                    return fieldICachenfo;
                }
            }
            finally
            {
                s_grainReferenceRuntimeCacheLocker.ExitReadLock();
            }

            s_grainReferenceRuntimeCacheLocker.EnterWriteLock();
            try
            {
                if (s_grainReferenceRuntimeCache.TryGetValue(traitRuntime, out var fieldICachenfo))
                    return fieldICachenfo;

                var cancellationTokenRuntime = traitRuntime.GetField("cancellationTokenRuntime", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

                Debug.Assert(cancellationTokenRuntime != null);
                s_grainReferenceRuntimeCache.Add(traitRuntime, cancellationTokenRuntime);

                return cancellationTokenRuntime;
            }
            finally
            {
                s_grainReferenceRuntimeCacheLocker.ExitWriteLock();
            }
        }

        #endregion

        #endregion
    }
}
