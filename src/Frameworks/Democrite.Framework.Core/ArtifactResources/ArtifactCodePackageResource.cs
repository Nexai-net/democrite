// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Helpers;
    using Democrite.Framework.Toolbox.Services;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Define an artifact about executable code that can be managed by a remoting vgrain
    /// </summary>
    /// <seealso cref="ArtifactBaseResource" />
    /// <seealso cref="IArtifactCodePackageResource" />
    public sealed class ArtifactCodePackageResource : ArtifactBaseResource, IArtifactCodePackageResource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactCodePackageResource"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        private ArtifactCodePackageResource(Guid id,
                                            string displayName,
                                            string? description,
                                            Version version,
                                            string executablePath,
                                            bool allowPersistence,
                                            IEnumerable<string> arguments,
                                            string? executor,
                                            Uri packageSource,
                                            ArtifactPackageTypeEnum packageType,
                                            string hash,
                                            DateTime creationOn)
            : base(id, displayName, description, version, hash, creationOn, ArtifactResourceTypeEnum.Executable)
        {
            this.PackageSource = packageSource;
            this.PackageType = packageType;
            this.Executor = executor;
            this.ExecutablePath = executablePath;
            this.AllowPersistence = allowPersistence;
            this.Arguments = arguments?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public string ExecutablePath { get; }

        /// <inheritdoc />
        public bool AllowPersistence { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Arguments { get; }

        /// <inheritdoc />
        public string? Executor { get; }

        /// <inheritdoc />
        public Uri PackageSource { get; }

        /// <inheritdoc />
        public ArtifactPackageTypeEnum PackageType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new <see cref="ArtifactCodePackageResource"/>
        /// </summary>
        internal static ArtifactCodePackageResource Create(string displayName,
                                                           string? description,
                                                           Version version,
                                                           string executablePath,
                                                           bool allowPersistence,
                                                           IEnumerable<string> arguments,
                                                           string? executor,
                                                           Uri packageSource,
                                                           ArtifactPackageTypeEnum packageType,
                                                           string hash)
        {
            return new ArtifactCodePackageResource(Guid.NewGuid(),
                                                   displayName,
                                                   description,
                                                   version,
                                                   executablePath,
                                                   allowPersistence,
                                                   arguments,
                                                   executor,
                                                   packageSource,
                                                   packageType,
                                                   hash,
                                                   DateTime.UtcNow);
        }

        /// <summary>
        /// Build a new <see cref="ArtifactCodePackageResource"/>
        /// </summary>
        public static ValueTask<ArtifactCodePackageResource> BuildAsync(string displayName,
                                                                        Action<IArtifactCodePackageResourceBuilder> builder,
                                                                        IHashService? hashService = null,
                                                                        IFileSystemHandler? fileSystemHandler = null)
        {
            return BuildAsync(displayName,
                              string.Empty,
                              builder,
                              hashService,
                              fileSystemHandler);
        }

        /// <summary>
        /// Build a new <see cref="ArtifactCodePackageResource"/>
        /// </summary>
        public static ValueTask<ArtifactCodePackageResource> BuildAsync(string displayName,
                                                                        string? description,
                                                                        Action<IArtifactCodePackageResourceBuilder> builder,
                                                                        IHashService? hashService = null,
                                                                        IFileSystemHandler? fileSystemHandler = null)
        {
            hashService ??= HashSHA256Service.Instance;
            fileSystemHandler ??= FileSystemHandler.Instance;

            var builderInst = new ArtifactCodePackageResourceBuilder(displayName, description);
            builder?.Invoke(builderInst);

            return builderInst.BuildAsync(hashService, fileSystemHandler);
        }

        #endregion
    }
}
