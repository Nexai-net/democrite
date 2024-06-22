// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define an artifact that target a package
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
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
                                         Version? version,
                                         string hash,
                                         DateTime creationOn,
                                         ArtifactTypeEnum artifactType,
                                         Uri packageSource,
                                         IEnumerable<string> packageFiles,
                                         ArtifactPackageTypeEnum packageType,
                                         DefinitionMetaData? metaData) 
            : base(uid, displayName, version, hash, creationOn, artifactType, metaData)
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
        [Id(0)]
        public Uri PackageSource { get; }

        /// <summary>
        /// Gets the package files.
        /// </summary>
        [DataMember]
        [Id(1)]
        public IReadOnlyCollection<string> PackageFiles { get; }

        /// <summary>
        /// Gets the type of the package.
        /// </summary>
        [DataMember]
        [Id(2)]
        public ArtifactPackageTypeEnum PackageType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] ArtifactDefinition other)
        {
            return other is ArtifactPackageDefinition pkg &&
                   this.ArtifactType == pkg.ArtifactType &&
                   this.PackageSource == pkg.PackageSource &&
                   this.PackageFiles.SequenceEqual(pkg.PackageFiles);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.ArtifactType,
                                    this.PackageSource,
                                    this.PackageFiles.Aggregate(0, (acc, f) => acc ^ (f?.GetHashCode() ?? 0)));
        }

        #endregion
    }
}
