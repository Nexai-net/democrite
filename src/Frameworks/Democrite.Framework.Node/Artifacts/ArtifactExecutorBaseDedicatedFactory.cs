// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Exceptions;

    using Elvex.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Democrite.Framework.Node.Abstractions.Artifacts.IArtifactExecutorDedicatedFactory" />
    public abstract class ArtifactExecutorBaseDedicatedFactory : IArtifactExecutorDedicatedFactory
    {
        #region Fields

        private readonly IFileSystemHandler _fileSystemHandler;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutorBaseDedicatedFactory"/> class.
        /// </summary>
        protected ArtifactExecutorBaseDedicatedFactory(IFileSystemHandler fileSystemHandler)
        {
            this._fileSystemHandler = fileSystemHandler;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public abstract ValueTask<IArtifactExternalCodeExecutor> BuildNewExecutorAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                                                       IArtifactExternalCodeExecutor? previousExecutor,
                                                                                       IExecutionContext executionContext,
                                                                                       ILogger logger,
                                                                                       CancellationToken token);

        /// <inheritdoc />
        public abstract bool CanManaged(ArtifactExecutableDefinition artifactExecutableDefinition);

        /// <inheritdoc />
        public abstract ValueTask<bool> CheckExecutorValidityAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                                   IArtifactExternalCodeExecutor artifact,
                                                                   IExecutionContext executionContext,
                                                                   ILogger logger,
                                                                   CancellationToken token);

        #region Tools

        /// <summary>
        /// Gets the localIsFine folder path associate to <see cref="ArtifactExecutableDefinition"/>
        /// </summary>
        protected async ValueTask<Uri> GetLocalFolderPathAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                               CancellationToken token)
        {
            var tmpRootPath = await this._fileSystemHandler.GetTemporaryFolderAsync(false, token);
            var artifactFolder = nameof(Artifacts);
            var version = artifactCodePackageResource.Version?.ToString() ?? "Unversionned";

            return new Uri(Path.Combine(tmpRootPath,
                                        artifactFolder,
                                        artifactCodePackageResource.DisplayName + " - " + artifactCodePackageResource.Uid,
                                        version) + "/");
        }

        /// <summary>
        /// Clean up the target artifact install folder
        /// </summary>
        protected async ValueTask CleanUpLocalFolderAsync(ArtifactExecutableDefinition definition, CancellationToken token)
        {
            var rootInstallPath = await GetLocalFolderPathAsync(definition, token);

            if (rootInstallPath is not null)
                await this._fileSystemHandler.DeleteFolder(rootInstallPath, true);
        }

        /// <summary>
        /// Installs in local the artifact data
        /// </summary>
        protected ValueTask InstallInLocalAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                Uri root,
                                                IReadOnlyCollection<Uri> targetFiles,
                                                ILogger logger,
                                                IExecutionContext? executionContext,
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
        protected async ValueTask InstallInLocalAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                    string tmpLocalResourceRootFolder,
                                                    Uri root,
                                                    IExecutionContext? executionContext,
                                                    CancellationToken token)
        {
            var sourceUri = this._fileSystemHandler.MakeUriAbsolute(tmpLocalResourceRootFolder);

            foreach (var sourceFile in artifactCodePackageResource.PackageFiles)
            {
                var source = new Uri(sourceUri, sourceFile);
                var target = new Uri(root, sourceFile);

                if (!await this._fileSystemHandler.CopyFromAsync(source, target, true))
                    throw new ArtifactPreparationFailedException("Could not copy '{0}' from '{1}' to '{2}'".WithArguments(sourceFile, source, target), executionContext);
            }
        }

        #endregion

        #endregion
    }
}
