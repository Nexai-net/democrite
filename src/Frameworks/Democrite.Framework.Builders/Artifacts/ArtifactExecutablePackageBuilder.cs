﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Helpers;
    using Elvex.Toolbox.Services;

    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Builder used to setup information <see cref="ArtifactCodePackageResource"/>
    /// </summary>
    /// <seealso cref="IArtifactExecutablePackageResourceBuilder" />
    internal sealed class ArtifactExecutablePackageBuilder : IArtifactExecutablePackageResourceBuilder,
                                                             IArtifactCodePackageResourceBuilderFrom,
                                                             IArtifactCodePackageResourceBuilderFromSource,
                                                             IArtifactCodePackageResourceBuilderFinalizer
    {
        #region Fields

        private readonly HashSet<string> _packageFiles;
        
        private readonly HashSet<string> _packageRecurciveTemplateFiles;
        private readonly HashSet<string> _packageTemplateFiles;
        
        private readonly HashSet<string> _packageExcludeFiles;
        private readonly HashSet<string> _packageExcludeDir;
        private readonly HashSet<Regex> _packageExcludeReg;

        private readonly string? _description;
        private readonly string _displayName;
        private readonly Guid _uid;

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
        /// Initializes a new instance of the <see cref="ArtifactExecutablePackageBuilder"/> class.
        /// </summary>
        internal ArtifactExecutablePackageBuilder(string displayName, string? description, Guid? uid = null)
        {
            this._packageFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._packageRecurciveTemplateFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._packageTemplateFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            this._packageExcludeDir = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._packageExcludeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            this._packageExcludeReg = new HashSet<Regex>();

            this._displayName = displayName;
            this._description = description;
            this._uid = uid ?? Guid.NewGuid();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer Arguments(params string[] args)
        {
            this._arguments = args?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFrom ExecuteBy(string executor, Version version)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(executor);
            ArgumentNullException.ThrowIfNull(version);

            this._executor = executor;
            this._executorVersion = version;
            return this;

        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer ExecuteFile(string exec)
        {
            ArgumentNullException.ThrowIfNull(exec);
            this._executablePath = exec;

            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.AppendFiles(params string[] files)
        {
            AppendFilesImpl(files);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.AppendFiles(string fileTemplate, bool recursive)
        {
            AppendFilesTemplateImpl(fileTemplate, recursive);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.AppendFiles(params string[] files)
        {
            AppendFilesImpl(files);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.AppendFiles(string fileTemplate, bool recursive)
        {
            AppendFilesTemplateImpl(fileTemplate, recursive);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.ExcludePathRegex(Regex regex)
        {
            ExcludePathRegexImpl(regex);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.ExcludeFiles(params string[] files)
        {
            ExcludeFilesImpl(files);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.ExcludeDirectories(params string[] directories)
        {
            ExcludeDirectoriesImpl(directories);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.ExcludePathRegex(Regex regex)
        {
            ExcludePathRegexImpl(regex);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.ExcludeFiles(params string[] files)
        {
            ExcludeFilesImpl(files);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.ExcludeDirectories(params string[] directories)
        {
            ExcludeDirectoriesImpl(directories);
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFromSource From(Uri packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version? version)
        {
            ArgumentNullException.ThrowIfNull(packageUri);

            this._packageVersion = version;
            this._packageSource = packageUri;
            this._packageType = packageTypeEnum;

            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFromSource From(string packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version? version)
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
        public IArtifactCodePackageResourceBuilderFromSource From(Uri packageUri, ArtifactPackageTypeEnum packageTypeEnum, string? version = null)
        {
            return From(packageUri, packageTypeEnum, string.IsNullOrEmpty(version) ? null : Version.Parse(version));
        }
        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFromSource From(string packageUri, ArtifactPackageTypeEnum packageTypeEnum, string? version = null)
        {
            return From(packageUri, packageTypeEnum, string.IsNullOrEmpty(version) ? null : Version.Parse(version));
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer Persistent()
        {
            this._allowPersistence = true;
            return this;
        }

        /// <summary>
        /// Builds <see cref="ArtifactCodePackageResource"/>.
        /// </summary>
        public async ValueTask<ArtifactExecutableDefinition> CompileAsync(IHashService? hashService = null,
                                                                          IFileSystemHandler? fileSystemHandler = null,
                                                                          ITimeManager? timeManager = null,
                                                                          CancellationToken token = default)
        {
            fileSystemHandler ??= FileSystemHandler.Instance;
            hashService ??= HashSHA256Service.Instance;
            timeManager ??= TimeManager.Instance;

            ArgumentNullException.ThrowIfNull(this._packageSource);
            ArgumentNullException.ThrowIfNull(this._packageType);
            ArgumentNullException.ThrowIfNull(this._executablePath);

            var absolutePackageSource = fileSystemHandler.MakeUriAbsolute(this._packageSource);

            ExtractTemplateFiles(absolutePackageSource, fileSystemHandler, this._packageRecurciveTemplateFiles, true);
            ExtractTemplateFiles(absolutePackageSource, fileSystemHandler, this._packageTemplateFiles, false);

            foreach (var directory in this._packageExcludeDir)
            {
                this._packageFiles.RemoveWhere(p => p.StartsWith(directory + "/", StringComparison.OrdinalIgnoreCase) ||
                                                    p.StartsWith("./" + directory + "/", StringComparison.OrdinalIgnoreCase));
            }

            foreach (var file in this._packageExcludeFiles)
            {
                this._packageFiles.RemoveWhere(p => p.StartsWith(file, StringComparison.OrdinalIgnoreCase) ||
                                                    p.StartsWith("./" + file, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var reg in this._packageExcludeReg)
            {
                this._packageFiles.RemoveWhere(p => reg.IsMatch(p));
            }

            this._packageFiles.Add(this._executablePath);

            var hash = await hashService.GetHash(this._packageFiles.Select(s => new Uri(absolutePackageSource, s)).ToArray(),
                                                 fileSystemHandler,
                                                 token);

            return new ArtifactExecutableDefinition(this._uid,
                                                    this._displayName,
                                                    this._description,
                                                    this._packageVersion,
                                                    hash,
                                                    timeManager.UtcNow,
                                                    this._executablePath,
                                                    this._allowPersistence,
                                                    this._arguments ?? EnumerableHelper<string>.ReadOnlyArray,
                                                    (string.IsNullOrWhiteSpace(this._executor)
                                                          ? string.Empty
                                                          : this._executor + ":" + this._executorVersion),
                                                    this._packageSource,
                                                    this._packageFiles.OrderBy(p => p).ToArray(),
                                                    this._packageType.GetValueOrDefault());
        }

        #region Tools

        /// <summary>
        /// Extracts the template files.
        /// </summary>
        private void ExtractTemplateFiles(Uri absolutePackageSource, IFileSystemHandler fileSystemHandler, IEnumerable<string> fileTemplates, bool recursive)
        {
            foreach (var tpl in fileTemplates)
            {
                var files = fileSystemHandler.SearchFiles(absolutePackageSource.LocalPath, tpl, recursive);
                foreach (var file in files)
                {
                    var relative = absolutePackageSource.MakeRelativeUri(file);
                    this._packageFiles.Add(relative.OriginalString);
                }
            }
        }

        /// <inheritdoc />
        private void AppendFilesImpl(params string[] files)
        {
            foreach (var file in files)
                this._packageFiles.Add(file);
        }

        /// <inheritdoc />
        private void AppendFilesTemplateImpl(string fileTemplate, bool recursive)
        {
            ArgumentNullException.ThrowIfNull(fileTemplate);

            if (recursive)
                this._packageRecurciveTemplateFiles.Add(fileTemplate);
            else
                this._packageTemplateFiles.Add(fileTemplate);
        }

        /// <summary>
        /// Excludes all files path that correspond to following regex
        /// </summary>
        private void ExcludePathRegexImpl(Regex regex)
        {
            this._packageExcludeReg.Add(regex);
        }

        /// <summary>
        /// Excludes all files
        /// </summary>
        private void ExcludeFilesImpl(params string[] files)
        {
            foreach (var file in files)
                this._packageExcludeFiles.Add(file);
        }

        /// <summary>
        /// Excludes all files of the directories.
        /// </summary>
        private void ExcludeDirectoriesImpl(params string[] directories)
        {
            foreach (var dir in directories)
                this._packageExcludeDir.Add(dir);
        }

        #endregion

        #endregion
    }
}
