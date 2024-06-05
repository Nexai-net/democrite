// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Storages
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    internal static class MemoryStorageRegistryHelper
    {
        private const string CONFIG_SEPARATOR = "###";

        /// <summary>
        /// Computes the name of the registry ext.
        /// </summary>
        public static string ComputeRegistryExtName(in ReadOnlySpan<char> stateName, in ReadOnlySpan<char> storageConfig)
        {
            Span<char> fullExtName = stackalloc char[stateName.Length + 3 + storageConfig.Length];

            storageConfig.CopyTo(fullExtName);

            // Add separator
            "###".CopyTo(fullExtName.Slice(storageConfig.Length));
            
            stateName.CopyTo(fullExtName.Slice(storageConfig.Length + 3));

            return fullExtName.ToString();
        }

        /// <summary>
        /// Explodes the registry ext key.
        /// </summary>
        internal static void ExplodeRegistryExtKey(in ReadOnlySpan<char> stateNameAndStorageConfig, out string stateName, out string storageConfig)
        {
            var indx = stateNameAndStorageConfig.IndexOf(CONFIG_SEPARATOR);

            storageConfig = stateNameAndStorageConfig.Slice(0, indx).ToString();
            stateName = stateNameAndStorageConfig.Slice(indx + 3).ToString();
        }
    }
}
