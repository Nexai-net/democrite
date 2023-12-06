// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Abstractions.Services
{
    using System;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge to hash datas 
    /// </summary>
    public interface IHashService
    {
        /// <summary>
        /// Gets the hash (base64) from data string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding">used encoding to transform <paramref name="data"/> into byte array; if null use UTF8.</param>
        ValueTask<string> GetHash(string data, Encoding? encoding = null, CancellationToken token = default);

        /// <summary>
        /// Gets the hash (base64) from data array
        /// </summary>
        ValueTask<string> GetHash(byte[] data, CancellationToken token = default);

        /// <summary>
        /// Gets the hash (base64) from stream data
        /// </summary>
        ValueTask<string> GetHash(Stream stream, CancellationToken token = default);

        /// <summary>
        /// Gets the hash of the <paramref name="target"/> file or folder
        /// </summary>
        ValueTask<string> GetHash(Uri target, IFileSystemHandler fileSystemHandler, bool recursive = false, CancellationToken token = default);
    }
}
