// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Services
{
    /// <summary>
    /// Handler used to search, load, open file/directory on the file system
    /// </summary>
    /// <remarks>
    ///     Provide a proxy usedfull for unit testing or other usage like caching
    /// </remarks>
    public interface IFileSystemHandler
    {
        /// <summary>
        /// Searches files from <paramref name="root"/> directory following <paramref name="searchPattern"/> pattern.
        /// </summary>
        IReadOnlyCollection<Uri> SearchFiles(string root, string searchPattern, bool recursive);

        /// <summary>
        /// Opens target file <paramref name="uri"/> in read mode
        /// </summary>
        Stream? OpenRead(Uri uri);

        /// <summary>
        /// Gets a value indicating if file target by <paramref name="uri"/> exits
        /// </summary>
        bool Exists(Uri uri);

        /// <summary>
        /// Makes <paramref name="uri"/> in absolute.
        /// </summary>
        Uri MakeUriAbsolute(Uri uri);

        /// <summary>
        /// Determines whether the specified target is a file nor a directory.
        /// </summary>
        bool IsFile(Uri target);
    }
}
