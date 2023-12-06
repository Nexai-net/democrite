// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Services
{
    using Democrite.Framework.Toolbox.Abstractions.Services;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <inheritdoc cref="IFileSystemHandler"/>
    public sealed class FileSystemHandler : IFileSystemHandler
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="FileSystemHandler"/> class.
        /// </summary>
        static FileSystemHandler()
        {
            Instance = new FileSystemHandler();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static FileSystemHandler Instance { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Stream OpenRead(Uri uri)
        {
            return File.OpenRead(uri.LocalPath);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Uri> SearchFiles(string root, string searchPattern, bool recursive)
        {
            var rootUri = new Uri(root, UriKind.Absolute);
            return Directory.GetFiles(root, searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                            .Select(s => new Uri(rootUri, s))
                            .ToArray();
        }

        /// <inheritdoc />
        public bool Exists(Uri uri)
        {
            return uri.IsFile && (File.Exists(uri.LocalPath) || Directory.Exists(uri.LocalPath));
        }

        /// <inheritdoc />

        /// <remarks>
        ///     // Add method to set working dir
        /// </remarks>
        public Uri MakeUriAbsolute(Uri uri)
        {
            if (uri.IsAbsoluteUri)
                return uri;

            var currentDir = Directory.GetCurrentDirectory();

            if (!currentDir.EndsWith("/"))
                currentDir += "/";

            return new Uri(new Uri(currentDir), uri.OriginalString);
        }

        /// <inheritdoc />
        public bool IsFile(Uri target)
        {
            var attr = File.GetAttributes(target.LocalPath);

            return attr.HasFlag(FileAttributes.Directory) == false;
        }

        #endregion
    }
}
