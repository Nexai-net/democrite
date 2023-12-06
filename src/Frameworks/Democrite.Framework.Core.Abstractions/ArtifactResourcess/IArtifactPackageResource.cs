// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using System;

    /// <summary>
    /// Define an artifact that target a package
    /// </summary>
    public interface IArtifactPackageResource : IArtifactResource
    {
        #region Properties

        /// <summary>
        /// Gets the package source.
        /// </summary>
        Uri PackageSource { get; }

        /// <summary>
        /// Gets the type of the package.
        /// </summary>
        ArtifactPackageTypeEnum PackageType { get; }

        #endregion
    }
}
