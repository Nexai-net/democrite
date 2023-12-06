// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Services
{
    using Democrite.Framework.Toolbox.Abstractions.Services;

    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge to provide SHA256 hashage 
    /// </summary>
    /// <seealso cref="IHashService" />
    public abstract class HashBaseService : IHashService
    {
        #region Fields

        private readonly Func<HashAlgorithm> _hashAlgorithmFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HashBaseService"/> class.
        /// </summary>
        protected HashBaseService(Func<HashAlgorithm> hashAlgorithmFactory)
        {
            this._hashAlgorithmFactory = hashAlgorithmFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<string> GetHash(string data,
                                         Encoding? encoding = null,
                                         CancellationToken token = default)
        {
            var encodingAlgo = encoding ?? Encoding.UTF8;

            return GetHash(encodingAlgo.GetBytes(data), token);
        }

        /// <inheritdoc />
        public ValueTask<string> GetHash(byte[] data,
                                         CancellationToken token = default)
        {
            using (var stream = new MemoryStream(data))
            {
                return GetHash(stream, token);
            }
        }

        /// <inheritdoc />
        public async ValueTask<string> GetHash(Stream stream,
                                               CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (stream.Length == 0)
                return string.Empty;

            using (var hasher = this._hashAlgorithmFactory())
            {
                byte[] results;

                if (stream.Length < 65536)
                {
                    results = hasher.ComputeHash(stream);
                    token.ThrowIfCancellationRequested();
                }
                else
                {
                    results = await hasher.ComputeHashAsync(stream, token);
                }

                token.ThrowIfCancellationRequested();
                return Convert.ToBase64String(results);
            }
        }

        /// <inheritdoc />
        public async ValueTask<string> GetHash(Uri target,
                                               IFileSystemHandler fileSystemHandler,
                                               bool recursive = false,
                                               CancellationToken token = default)
        {
            if (!target.IsAbsoluteUri)
                throw new InvalidOperationException(nameof(target) + " must be an absolute path.");

            if (fileSystemHandler.IsFile(target))
            {
                using (var fileStream = fileSystemHandler.OpenRead(target))
                {
                    if (fileStream == null)
                        throw new NullReferenceException(nameof(IFileSystemHandler) + "." + nameof(IFileSystemHandler.OpenRead) + " return null from " + target);

                    return await GetHash(fileStream, token);
                }
            }

            var allFiles = fileSystemHandler.SearchFiles(target.LocalPath, "*", recursive);

            var hashFilesHashages = allFiles.OrderBy(f => f.LocalPath)
                                            .Select(f => GetHash(f, fileSystemHandler, false, token).AsTask())
                                            .ToArray();

            var results = await Task.WhenAll(hashFilesHashages);

            var stringBuilder = new StringBuilder(results.Length);

            foreach (var result in results)
                stringBuilder.Append(result);

            return await GetHash(stringBuilder.ToString(), Encoding.ASCII, token);
        }

        #endregion
    }
}
