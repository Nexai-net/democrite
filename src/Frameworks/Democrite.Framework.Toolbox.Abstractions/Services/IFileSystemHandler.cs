// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Services
{
    using System.Threading;

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
        IReadOnlyCollection<Uri> SearchFiles(in string root, in string searchPattern, bool recursive);

        /// <summary>
        /// Opens target file <paramref name="uri"/> in read mode
        /// </summary>
        Stream? OpenRead(Uri uri);

        /// <summary>
        /// Writes data to file 
        /// </summary>
        /// <returns>
        ///     <c>true</c> if file have file full fill; otherwise <c>false</c> if file already exist and <paramref name="override"/> is set false
        /// </returns>
        ValueTask<bool> WriteToFileAsync(Stream stream, Uri uri, bool @override = true, CancellationToken token = default);

        /// <summary>
        /// Writes data to file 
        /// </summary>
        /// <returns>
        ///     <c>true</c> if file have file full fill; otherwise <c>false</c> if file already exist and <paramref name="override"/> is set false
        /// </returns>
        ValueTask<bool> WriteToFileAsync(byte[] bytes, Uri uri, bool @override = true, CancellationToken token = default);

        /// <summary>
        /// Gets a value indicating if file target by <paramref name="uri"/> exits
        /// </summary>
        bool Exists(Uri uri);

        /// <summary>
        /// Makes <paramref name="uri"/> in absolute.
        /// </summary>
        Uri MakeUriAbsolute(Uri uri);

        /// <summary>
        /// Makes <paramref name="uri"/> in absolute.
        /// </summary>
        Uri MakeUriAbsolute(in string uri);

        /// <summary>
        /// Determines whether the specified target is a file nor a directory.
        /// </summary>
        bool IsFile(Uri target);

        /// <summary>
        /// Deletes the specified file if exist
        /// </summary>
        ValueTask<bool> Delete(Uri file);

        /// <summary>
        /// Gets a folder where temporary data could be write
        /// </summary>
        ValueTask<string> GetTemporaryFolderAsync(bool global, CancellationToken token);

        /// <summary>
        /// Copies from <paramref name="source"/> to <paramref name="target"/>
        /// </summary>
        ValueTask<bool> CopyFromAsync(Uri source, Uri target, bool @override = true, CancellationToken token = default);

        /// <summary>
        /// Copies from <paramref name="source"/> to <paramref name="target"/>
        /// </summary>
        ValueTask<bool> CopyFromAsync(Stream source, Uri target, bool @override = true, CancellationToken token = default);
    }
}
