// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using System;

    /// <summary>
    /// Builder in charge to produce a <see cref="ArtifactCodePackageResource"/>
    /// </summary>
    public interface IArtifactCodePackageResourceBuilder
    {
        /// <summary>
        /// Define where the artifact is and the type of package.
        /// </summary>
        IArtifactCodePackageResourceBuilder From(Uri packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version version);

        /// <summary>
        /// Define where the artifact is and the type of package.
        /// </summary>
        IArtifactCodePackageResourceBuilder From(string packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version version);

        /// <summary>
        /// Define the executor; example to run python script you need python installed.
        /// </summary>
        IArtifactCodePackageResourceBuilder ExecuteBy(string executor, Version version);

        /// <summary>
        /// Deinfe the argument to pass by the executable at start.
        /// </summary>
        IArtifactCodePackageResourceBuilder Arguments(params string[] args);

        /// <summary>
        /// Define the file to execute on the package datas.
        /// </summary>
        IArtifactCodePackageResourceBuilder ExecuteFile(string exec);

        /// <summary>
        /// Define if the executable managed to stay alive after usage.
        /// </summary>
        IArtifactCodePackageResourceBuilder Persistent();
    }
}
