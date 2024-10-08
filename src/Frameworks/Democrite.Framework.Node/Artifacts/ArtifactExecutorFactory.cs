﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Exceptions;

    using Elvex.Toolbox.Disposables;
    using Elvex.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc cref="IArtifactExecutorFactory" />
    /// <seealso cref="IArtifactExecutorFactory" />
    public sealed class ArtifactExecutorFactory : SafeDisposable, IArtifactExecutorFactory
    {
        #region Fields

        private readonly Dictionary<Guid, IArtifactExternalCodeExecutor> _cacheExec;
        private readonly ReaderWriterLockSlim _execCacheLocker;

        private readonly IReadOnlyCollection<IArtifactExecutorDedicatedFactory> _artifactExecutorDedicatedFactories;
        private readonly Dictionary<Guid, Mutex> _lockByArtifact;
        private readonly SemaphoreSlim _lock;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutorFactory"/> class.
        /// </summary>
        public ArtifactExecutorFactory(IEnumerable<IArtifactExecutorDedicatedFactory> artifactExecutorDedicatedFactories)
        {
            this._cacheExec = new Dictionary<Guid, IArtifactExternalCodeExecutor>();
            this._lockByArtifact = new Dictionary<Guid, Mutex>();
            
            this._execCacheLocker = new ReaderWriterLockSlim();
            this._lock = new SemaphoreSlim(1);

            this._artifactExecutorDedicatedFactories = artifactExecutorDedicatedFactories?.ToArray() ?? EnumerableHelper<IArtifactExecutorDedicatedFactory>.ReadOnlyArray;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<IArtifactExternalCodeExecutor> BuildAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                                         IExecutionContext executionContext,
                                                                         ILogger logger,
                                                                         CancellationToken token)
        {
            Mutex locker;
            await this._lock.LockAsync(token);
            try
            {
                if (this._lockByArtifact.TryGetValue(artifactCodePackageResource.Uid, out var cachedLocker))
                {
                    locker = cachedLocker;
                }
                else
                {
                    locker = new Mutex(false, artifactCodePackageResource.Uid.ToString());
                    this._lockByArtifact.Add(artifactCodePackageResource.Uid, locker);
                }
            }
            finally
            {
                this._lock.Release();
            }

            // Get a system thread safe to ensure the resources link to the artefact are not already used.
            using (locker.Lock(token: token))
            {
                // Ensure asyc code and mutext lock and unlock in the same thread
                var builTask = Task.Run(async () => await BuildInstallAsync(artifactCodePackageResource, executionContext, logger, token), token);
                return builTask.GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Builds the install.
        /// </summary>
        private async ValueTask<IArtifactExternalCodeExecutor> BuildInstallAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                                                 IExecutionContext executionContext,
                                                                                 ILogger logger,
                                                                                 CancellationToken token)
        {
            IArtifactExternalCodeExecutor? cachedExecutor = null;
            this._execCacheLocker.EnterReadLock();
            try
            {
                if (this._cacheExec.TryGetValue(artifactCodePackageResource.Uid, out var artifactExecutor))
                    cachedExecutor = artifactExecutor;
            }
            finally
            {
                this._execCacheLocker.ExitReadLock();
            }

            token.ThrowIfCancellationRequested();

            var dedicateFactory = this._artifactExecutorDedicatedFactories.FirstOrDefault(f => f.CanManaged(artifactCodePackageResource));

            if (dedicateFactory is null)
                throw new ArtifactPreparationFailedException("Missing dedicated artifact executor. " + artifactCodePackageResource.ToDebugDisplayName(), executionContext);

            if (cachedExecutor is not null)
            {
                if (await dedicateFactory.CheckExecutorValidityAsync(artifactCodePackageResource, cachedExecutor, executionContext, logger, token))
                    return cachedExecutor;

                await cachedExecutor.StopAsync(executionContext, logger, token);
            }

            var executor = await dedicateFactory.BuildNewExecutorAsync(artifactCodePackageResource, cachedExecutor, executionContext, logger, token);
            if (cachedExecutor is not null && object.ReferenceEquals(executor, cachedExecutor) == false)
                await cachedExecutor.DisposeAsync();

            this._execCacheLocker.EnterWriteLock();
            try
            {
                /*
                 * Due to dedicated locker by artifact Uid we are sure that previous (if exist) is not different from 'cachedExecutor' variable.
                 */
                this._cacheExec[artifactCodePackageResource.Uid] = executor;
                token.ThrowIfCancellationRequested();
            }
            finally
            {
                this._execCacheLocker.ExitWriteLock();
            }

            return executor;
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            this._execCacheLocker.Dispose();
            this._lock.Dispose();
            base.DisposeEnd();
        }

        #endregion
    }
}
