// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Exceptions;
    using Democrite.Framework.Node.Resources;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Disposables;
    using Democrite.Framework.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc cref="IArtifactExecutorFactory" />
    /// <seealso cref="IArtifactExecutorFactory" />
    public sealed class ArtifactExecutorFactory : SafeDisposable, IArtifactExecutorFactory
    {
        #region Fields

        private static readonly string[] s_verionsArg = new[] { "--version" };

        private readonly Dictionary<Guid, string> _cacheNewHashRuntime;

        private readonly Dictionary<Guid, IArtifactExternalCodeExecutor> _cacheExec;
        private readonly ReaderWriterLockSlim _execCacheLocker;
        private readonly Dictionary<Guid, Semaphore> _lockByArtifact;
        private readonly SemaphoreSlim _lock;

        private readonly IProcessSystemService _processSystemService;
        private readonly IFileSystemHandler _fileSystemHandler;
        private readonly INetworkInspector _networkInspector;
        private readonly IServiceProvider _serviceProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IHashService _hashService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutorFactory"/> class.
        /// </summary>
        public ArtifactExecutorFactory(IJsonSerializer jsonSerializer,
                                       IServiceProvider serviceProvider,
                                       IFileSystemHandler fileSystemHandler,
                                       IProcessSystemService processSystemService,
                                       INetworkInspector networkInspector,
                                       IHashService hashService)
        {
            this._hashService = hashService;
            this._jsonSerializer = jsonSerializer;
            this._serviceProvider = serviceProvider;
            this._fileSystemHandler = fileSystemHandler;
            this._processSystemService = processSystemService;
            this._networkInspector = networkInspector;

            this._cacheNewHashRuntime = new Dictionary<Guid, string>();
            this._cacheExec = new Dictionary<Guid, IArtifactExternalCodeExecutor>();
            this._execCacheLocker = new ReaderWriterLockSlim();

            this._lockByArtifact = new Dictionary<Guid, Semaphore>();
            this._lock = new SemaphoreSlim(1);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<IArtifactExternalCodeExecutor> BuildAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                                         IExecutionContext executionContext,
                                                                         ILogger logger,
                                                                         CancellationToken token)
        {
            Semaphore locker;
            await this._lock.LockAsync();
            try
            {
                if (this._lockByArtifact.TryGetValue(artifactCodePackageResource.Uid, out var cachedLocker))
                {
                    locker = cachedLocker;
                }
                else
                {
                    locker = new Semaphore(1, 1, artifactCodePackageResource.Uid.ToString());
                    this._lockByArtifact.Add(artifactCodePackageResource.Uid, locker);
                }
            }
            finally
            {
                this._lock.Release();
            }

            // Create a system thread safe to ensure the resources link to the artefact are not already used.
            using (locker.Lock())
            {
                var rootPath = await GetLocalFolderPathAsync(artifactCodePackageResource, token);

                var files = artifactCodePackageResource.PackageFiles
                                                       .Select(f => new Uri(rootPath, f))
                                                       .ToArray();

                var localIsFine = await LocalDataAreUsableAsync(artifactCodePackageResource, files, token);

                if (localIsFine && Debugger.IsAttached && artifactCodePackageResource.PackageType == ArtifactPackageTypeEnum.Directory)
                {
                    var localDebugVerif = await DebugDirLocalChangeFromDeployement(artifactCodePackageResource, files, token);

                    localIsFine = localDebugVerif.isValid;

                    if (!localIsFine)
                    {
                        var str = """
                                    [HotReload {artifactName} ]

                                    We detect that code source change from deployed one, we will attempt to update the deployed one. 
                                    This may result of error due to deamon running.

                                    This action is only available on debug attached to visual studio with artifact package type Directory

                                 """;

                        logger.OptiLog(LogLevel.Warning, str, artifactCodePackageResource.DisplayName + " - " + artifactCodePackageResource.Uid);

                        this._execCacheLocker.EnterWriteLock();
                        try
                        {
                            // Store the last deployed hash to that automatically differ from the one store in the definition
                            this._cacheNewHashRuntime[artifactCodePackageResource.Uid] = localDebugVerif.sourceHash;
                        }
                        finally
                        {
                            this._execCacheLocker.ExitWriteLock();
                        }
                    }
                }

                if (localIsFine == false)
                {
                    IArtifactExternalCodeExecutor? oldOne = null;
                    this._execCacheLocker.EnterWriteLock();
                    try
                    {
                        if (this._cacheExec.TryGetValue(artifactCodePackageResource.Uid, out var executor))
                        {
                            oldOne = executor;
                            this._cacheExec.Remove(artifactCodePackageResource.Uid);
                        }
                    }
                    finally
                    {
                        this._execCacheLocker.ExitWriteLock();
                    }

                    // Dispose old one to ensure installation doesn't conflict
                    await (oldOne?.DisposeAsync() ?? ValueTask.CompletedTask);

                    await InstallInLocalAsync(artifactCodePackageResource,
                                              rootPath,
                                              files,
                                              logger,
                                              executionContext,
                                              token);
                }

                await CheckExecutor(artifactCodePackageResource, logger, token);

                this._execCacheLocker.EnterReadLock();
                try
                {
                    if (this._cacheExec.TryGetValue(artifactCodePackageResource.Uid, out var executor))
                    {
                        if (!executor.IsDisposed)
                            return executor;

                        this._cacheExec.Remove(artifactCodePackageResource.Uid);
                    }
                }
                finally
                {
                    this._execCacheLocker.ExitReadLock();
                }

                this._execCacheLocker.EnterWriteLock();
                try
                {
                    IArtifactExternalCodeExecutor? newExecutor;
                    if (this._cacheExec.TryGetValue(artifactCodePackageResource.Uid, out newExecutor))
                    {
                        if (!newExecutor.IsDisposed)
                            return newExecutor;

                        this._cacheExec.Remove(artifactCodePackageResource.Uid);
                    }

                    if (!artifactCodePackageResource.AllowPersistence)
                    {
                        newExecutor = new ExternalCodeCLIExecutor(artifactCodePackageResource,
                                                                  this._processSystemService,
                                                                  this._jsonSerializer,
                                                                  rootPath);
                    }
                    else
                    {
                        newExecutor = new ExternalCodeDeamonExecutor(artifactCodePackageResource,
                                                                     this._processSystemService,
                                                                     this._jsonSerializer,
                                                                     this._networkInspector,
                                                                     rootPath);
                    }

                    this._cacheExec.Add(artifactCodePackageResource.Uid, newExecutor);
                    return newExecutor;
                }
                finally
                {
                    this._execCacheLocker.ExitWriteLock();
                }
            }
        }

        #region Tools

        /// <summary>
        /// Gets the localIsFine folder path associate to <see cref="ArtifactExecutableDefinition"/>
        /// </summary>
        private async ValueTask<Uri> GetLocalFolderPathAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                             CancellationToken token)
        {
            var tmpRootPath = await this._fileSystemHandler.GetTemporaryFolderAsync(false, token);
            var artifactFolder = nameof(Artifacts);
            var version = artifactCodePackageResource.Version?.ToString() ?? "Unversionned";

            return new Uri(Path.Combine(tmpRootPath, artifactFolder, artifactCodePackageResource.DisplayName + " - " + artifactCodePackageResource.Uid, version) + "/");
        }

        /// <summary>
        /// Check if localIsFine values are correctly in place and not corrupt
        /// </summary>
        private async ValueTask<bool> LocalDataAreUsableAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                              IReadOnlyCollection<Uri> files,
                                                              CancellationToken token)
        {
            foreach (var file in files)
            {
                if (!this._fileSystemHandler.Exists(file))
                    return false;
            }

            var sourceHash = artifactCodePackageResource.Hash;

            if (Debugger.IsAttached && artifactCodePackageResource.PackageType == ArtifactPackageTypeEnum.Directory)
            {
                // override the hash check because i can be overrided since a change detection at runtime
                this._execCacheLocker.EnterReadLock();
                try
                {
                    if (this._cacheNewHashRuntime.TryGetValue(artifactCodePackageResource.Uid, out var overrideHash))
                        sourceHash = overrideHash;
                }
                finally
                {
                    this._execCacheLocker.ExitReadLock();
                }
            }

            var hash = await this._hashService.GetHash(files, this._fileSystemHandler, token);
            return string.Equals(hash, sourceHash);
        }

        /// <summary>
        /// Check if debug mode is source files are the same as deployed one
        /// </summary>
        private async ValueTask<(bool isValid, string sourceHash)> DebugDirLocalChangeFromDeployement(ArtifactExecutableDefinition artifactCodePackageResource,
                                                                                                      IReadOnlyCollection<Uri> deployedFiles,
                                                                                                      CancellationToken token)
        {
            var sourceUri = this._fileSystemHandler.MakeUriAbsolute(artifactCodePackageResource.PackageSource.OriginalString);

            var absSourceFiles = artifactCodePackageResource.PackageFiles
                                                            .Select(s => new Uri(sourceUri, s))
                                                            .ToArray();

            var sourceHash = await this._hashService.GetHash(absSourceFiles, this._fileSystemHandler, token);
            var deployedHash = await this._hashService.GetHash(deployedFiles, this._fileSystemHandler, token);
            return (string.Equals(sourceHash, deployedHash), sourceHash);
        }

        /// <summary>
        /// Installs in local the artifact data
        /// </summary>
        private ValueTask InstallInLocalAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                              Uri root,
                                              IReadOnlyCollection<Uri> targetFiles,
                                              ILogger logger,
                                              IExecutionContext executionContext,
                                              CancellationToken token)
        {
            if (artifactCodePackageResource.PackageType == ArtifactPackageTypeEnum.Directory)
            {
                return InstallInLocalAsync(artifactCodePackageResource,
                                           artifactCodePackageResource.PackageSource.OriginalString,
                                           root,
                                           executionContext,
                                           token);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Installs in local the artifact data from temp local folder
        /// </summary>
        private ValueTask InstallInLocalAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                              string tmpLocalResourceRootFolder,
                                              Uri root,
                                              IExecutionContext executionContext,
                                              CancellationToken token)
        {
            var sourceUri = this._fileSystemHandler.MakeUriAbsolute(tmpLocalResourceRootFolder);

            foreach (var sourceFile in artifactCodePackageResource.PackageFiles)
            {
                var source = new Uri(sourceUri, sourceFile);
                var target = new Uri(root, sourceFile);

                if (!this._fileSystemHandler.CopyFrom(source, target, true))
                    throw new ArtifactPreparationFailedException("Could not copy '{0}' from '{1}' to '{2}'".WithArguments(sourceFile, source, target), executionContext);
            }

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Checks the executor.
        /// </summary>
        private async ValueTask<bool> CheckExecutor(ArtifactExecutableDefinition artifactCodePackageResource,
                                                    ILogger logger,
                                                    CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(artifactCodePackageResource.Executor))
                return true;

            ExtractExecutorAndVersion(artifactCodePackageResource.Executor, out var exec, out var version);

            bool exists = false;

            /*  --version are allways pass in argument to prevent executor to enter in console mode */

            if (string.IsNullOrEmpty(version))
            {
                exists = await this._processSystemService.CheckExecAvailableAsync(exec, cancellationToken, s_verionsArg);
            }
            else
            {
                exists = await this._processSystemService.CheckExecAvailableAsync(exec,
                                                                                  version!,
                                                                                  cancellationToken,
                                                                                  s_verionsArg);
            }

            if (!exists)
            {
                logger.OptiLog<string, string?>(LogLevel.Critical, NodeExceptionSR.ArtifactExecutorMissing, exec, version);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts the executor and version. The information <see cref="IArtifactCodePackageResource.Excutor"/> are normally stored in format "exec:version"
        /// </summary>
        private static void ExtractExecutorAndVersion(in string executorWithVersion, out string exec, out string? version)
        {
            version = null;

            ReadOnlySpan<char> execPart = executorWithVersion;

            var splitVersion = execPart.IndexOf(":");
            if (splitVersion < 0)
            {
                exec = executorWithVersion;
                return;
            }

            exec = execPart.Slice(0, splitVersion).ToString();
            version = execPart.Slice(splitVersion + 1).ToString();
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            this._execCacheLocker.Dispose();
            base.DisposeEnd();
        }

        #endregion

        #endregion
    }
}
