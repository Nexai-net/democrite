// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Helpers;

    using System;

    /// <summary>
    /// Builder used to setup information <see cref="ArtifactCodePackageResource"/>
    /// </summary>
    /// <seealso cref="IArtifactCodePackageResourceBuilder" />
    internal sealed class ArtifactCodePackageResourceBuilder : IArtifactCodePackageResourceBuilder
    {
        #region Fields

        private readonly string? _description;
        private readonly string _displayName;

        private IReadOnlyCollection<string>? _arguments;
        private ArtifactPackageTypeEnum? _packageType;
        private Version? _executorVersion;
        private string? _executablePath;
        private bool _allowPersistence;
        private string? _executor;
        private Uri? _packageSource;
        private Version? _packageVersion;

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactCodePackageResourceBuilder"/> class.
        /// </summary>
        internal ArtifactCodePackageResourceBuilder(string displayName,
                                                    string? description)
        {
            this._displayName = displayName;
            this._description = description;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilder Arguments(params string[] args)
        {
            this._arguments = args?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilder ExecuteBy(string executor, Version version)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(executor);
            ArgumentNullException.ThrowIfNull(version);

            this._executor = executor;
            this._executorVersion = version;
            return this;

        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilder ExecuteFile(string exec)
        {
            ArgumentNullException.ThrowIfNull(exec);
            this._executablePath = exec;

            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilder From(Uri packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version version)
        {
            ArgumentNullException.ThrowIfNull(packageUri);
            ArgumentNullException.ThrowIfNull(version);

            this._packageVersion = version;
            this._packageSource = packageUri;
            this._packageType = packageTypeEnum;

            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilder From(string packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version version)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(packageUri);

            packageUri = packageUri.Trim();

            // For the system directory end with an '/' and file not. Uri rule
            if (packageTypeEnum == ArtifactPackageTypeEnum.Directory && !(packageUri.EndsWith("/") || packageUri.EndsWith("\\")))
            {
                packageUri += "/";
            }

            return From(new Uri(packageUri, UriKind.RelativeOrAbsolute), packageTypeEnum, version);
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilder Persistent()
        {
            this._allowPersistence = true;
            return this;
        }

        /// <summary>
        /// Builds <see cref="ArtifactCodePackageResource"/>.
        /// </summary>
        public async ValueTask<ArtifactCodePackageResource> BuildAsync(IHashService hashService,
                                                                       IFileSystemHandler fileSystemHandler,
                                                                       CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(this._packageSource);
            ArgumentNullException.ThrowIfNull(this._packageType);
            ArgumentNullException.ThrowIfNull(this._executablePath);
            ArgumentNullException.ThrowIfNull(this._packageVersion);

            var absolutePackageSource = fileSystemHandler.MakeUriAbsolute(this._packageSource);

            var hash = await hashService.GetHash(absolutePackageSource, fileSystemHandler, true, token);

            return ArtifactCodePackageResource.Create(this._displayName,
                                                      this._description,
                                                      this._packageVersion,
                                                      this._executablePath,
                                                      this._allowPersistence,
                                                      this._arguments ?? EnumerableHelper<string>.ReadOnlyArray,
                                                      (string.IsNullOrWhiteSpace(this._executor)
                                                            ? string.Empty
                                                            : this._executor + ":" + this._executorVersion),
                                                      this._packageSource,
                                                      this._packageType.GetValueOrDefault(),
                                                      hash);
        }

        #endregion
    }
}
