// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    /// <summary>
    /// Data contract about an artifact resources. (ex: files, zip, pyton code, exe, images, ...)
    /// </summary>
    public interface IArtifactResource
    {
        /// <summary>
        /// Gets the unique artifact identifier.
        /// </summary>
        Guid Uid { get; }

        /// <summary>
        /// Gets the artifact type.
        /// </summary>
        ArtifactResourceTypeEnum Type { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets the content hash.
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// Gets the creation date time.
        /// </summary>
        DateTime CreationOn { get; }
    }
}
