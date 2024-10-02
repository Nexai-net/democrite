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

    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Extensions;
    using Elvex.Toolbox.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IArtifactExecutorDedicatedFactory" />
    internal sealed class ArtifactExecutorCLIDedicatedFactory : ArtifactExecutorBaseDedicatedFactory, IArtifactExecutorDedicatedFactory
    {
        #region Fields

        private static readonly string[] s_verionsArg = new[] { "--version" };

        private readonly Dictionary<Guid, string> _cacheNewHashRuntime;
        private readonly ReaderWriterLockSlim _execCacheLocker;
        
        private readonly IProcessSystemService _processSystemService;
        private readonly IConfiguration _configuration;

        private readonly IFileSystemHandler _fileSystemHandler;
        private readonly INetworkInspector _networkInspector;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IHashService _hashService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutorCLIDedicatedFactory"/> class.
        /// </summary>
        public ArtifactExecutorCLIDedicatedFactory(IFileSystemHandler fileSystemHandler,
                                                   IHashService hashService,
                                                   IProcessSystemService processSystemService,
                                                   IJsonSerializer jsonSerializer,
                                                   INetworkInspector networkInspector,
                                                   IConfiguration configuration)
            : base(fileSystemHandler)
        {
            this._cacheNewHashRuntime = new Dictionary<Guid, string>();
            this._execCacheLocker = new ReaderWriterLockSlim();

            this._configuration = configuration;
            this._processSystemService = processSystemService;
            this._fileSystemHandler = fileSystemHandler;
            this._networkInspector = networkInspector;
            this._jsonSerializer = jsonSerializer;
            this._hashService = hashService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override async ValueTask<IArtifactExternalCodeExecutor> BuildNewExecutorAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                                                             IArtifactExternalCodeExecutor? _,
                                                                                             IExecutionContext executionContext,
                                                                                             ILogger logger,
                                                                                             CancellationToken token)
        {

            var rootPath = await GetLocalFolderPathAsync(artifactExecutableDefinition, token);

            await CheckExecutor(artifactExecutableDefinition, logger, token);

            var files = artifactExecutableDefinition.PackageFiles
                                                    .Select(f => new Uri(rootPath, f))
                                                    .ToArray();
            token.ThrowIfCancellationRequested();

            await InstallInLocalAsync(artifactExecutableDefinition,
                                      rootPath,
                                      files,
                                      logger,
                                      executionContext,
                                      token);
            
            token.ThrowIfCancellationRequested();

            IArtifactExternalCodeExecutor newExecutor;

            if (!artifactExecutableDefinition.AllowPersistence)
            {
                newExecutor = new ExternalCodeCLIExecutor(artifactExecutableDefinition,
                                                          this._processSystemService,
                                                          this._jsonSerializer,
                                                          this._configuration,
                                                          rootPath);
            }
            else
            {
                newExecutor = new ExternalCodeDeamonExecutor(artifactExecutableDefinition,
                                                             this._processSystemService,
                                                             this._jsonSerializer,
                                                             this._networkInspector,
                                                             this._configuration,
                                                             rootPath);
            }

            return newExecutor;
        }

        /// <inheritdoc />
        public override async ValueTask<bool> CheckExecutorValidityAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                                         IArtifactExternalCodeExecutor? artifact,
                                                                         IExecutionContext executionContext,
                                                                         ILogger logger,
                                                                         CancellationToken token)
        {
            if (artifact is null)
                return false;
            
            var rootPath = await GetLocalFolderPathAsync(artifactExecutableDefinition, token);

            var files = artifactExecutableDefinition.PackageFiles
                                       .Select(f => new Uri(rootPath, f))
                                       .ToArray();

            var localIsFine = await LocalDataAreUsableAsync(artifactExecutableDefinition, files, token);

            if (localIsFine && Debugger.IsAttached && artifactExecutableDefinition.PackageType == ArtifactPackageTypeEnum.Directory)
            {
                var localDebugVerif = await DebugDirLocalChangeFromDeployement(artifactExecutableDefinition, files, token);

                localIsFine = localDebugVerif.isValid;

                if (!localIsFine)
                {
                    var str = """
                              [HotReload {artifactName} ]

                              We detect that code source change from deployed one, we will attempt to update the deployed one. 
                              This may result of error due to deamon running.

                              This action is only available on debug attached to visual studio with artifact package type Directory

                              """;

                    logger.OptiLog(LogLevel.Warning, str, artifactExecutableDefinition.DisplayName + " - " + artifactExecutableDefinition.Uid);

                    this._execCacheLocker.EnterWriteLock();
                    try
                    {
                        // Store the last deployed hash to that automatically differ from the one store in the definition
                        this._cacheNewHashRuntime[artifactExecutableDefinition.Uid] = localDebugVerif.sourceHash;
                    }
                    finally
                    {
                        this._execCacheLocker.ExitWriteLock();
                    }
                }
            }

            return localIsFine;
        }

        /// <inheritdoc />
        public override bool CanManaged(ArtifactExecutableDefinition artifactExecutableDefinition)
        {
            return artifactExecutableDefinition is not null &&
                   artifactExecutableDefinition.Environment is null &&
                   artifactExecutableDefinition.PackageType != ArtifactPackageTypeEnum.EnvironmentRepository;
        }

        #region Tools

        /// <summary>
        /// Check if local data are Fine, correctly in place and not corrupt
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
        /// Extracts the executor and version. The information <see cref="IArtifactCodePackageResource.Executor"/> are normally stored in format "exec:version"
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

        #endregion

        #endregion
    }
}
