// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using Elvex.Toolbox.Helpers;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define an artifact that target a package
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public class ArtifactPackageDefinition : ArtifactDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactPackageDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public ArtifactPackageDefinition(Guid uid,
                                         string displayName,
                                         string? description,
                                         Version? version,
                                         string hash,
                                         DateTime creationOn,
                                         ArtifactTypeEnum artifactType,
                                         Uri packageSource,
                                         IEnumerable<string> packageFiles,
                                         ArtifactPackageTypeEnum packageType) 
            : base(uid, displayName, description, version, hash, creationOn, artifactType)
        {
            this.PackageSource = packageSource;
            this.PackageFiles = packageFiles?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            this.PackageType = packageType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the package source.
        /// </summary>
        [DataMember]
        public Uri PackageSource { get; }

        /// <summary>
        /// Gets the package files.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<string> PackageFiles { get; }

        /// <summary>
        /// Gets the type of the package.
        /// </summary>
        [DataMember]
        public ArtifactPackageTypeEnum PackageType { get; }

        #endregion
    }
}
