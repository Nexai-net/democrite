// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Configurations;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define an executable artifact
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class ArtifactExecutableDefinition : ArtifactPackageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutableDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public ArtifactExecutableDefinition(Guid uid,
                                            string displayName,
                                            Version? version,
                                            string hash,
                                            DateTime creationOn,
                                            string executablePath,
                                            bool allowPersistence,
                                            IEnumerable<string> arguments,
                                            string? executor,
                                            Uri packageSource,
                                            IEnumerable<string> packageFiles,
                                            ArtifactPackageTypeEnum packageType,
                                            ArtifactExecutableEnvironmentDefinition? environment,
                                            DefinitionMetaData? metaData,
                                            ArtifactExecVerboseEnum verbose = ArtifactExecVerboseEnum.Minimal,
                                            IEnumerable<ConfigurationBaseDefinition>? configurations = null)
            : base(uid, displayName, version, hash, creationOn, ArtifactTypeEnum.Executable, packageSource, packageFiles, packageType, metaData)
        {
            this.ExecutablePath = executablePath;
            this.AllowPersistence = allowPersistence;
            this.Executor = executor;
            this.Arguments = arguments?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            this.Configurations = configurations?.ToArray() ?? EnumerableHelper<ConfigurationBaseDefinition>.ReadOnlyArray;
            this.Environment = environment;
            this.Verbose = verbose;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the executable path.
        /// </summary>
        [DataMember]
        [Id(0)]
        public string ExecutablePath { get; }

        /// <summary>
        /// Gets a value indicating whether the executable managed to stay alive an call multiple times.
        /// </summary>
        [DataMember]
        [Id(1)]
        public bool AllowPersistence { get; }

        /// <summary>
        /// Gets the execution arguments.
        /// </summary>
        [DataMember]
        [Id(2)]
        public IReadOnlyCollection<string> Arguments { get; }

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        [DataMember]
        [Id(3)]
        public IReadOnlyCollection<ConfigurationBaseDefinition> Configurations { get; }

        /// <summary>
        /// Gets the execution environment.
        /// </summary>
        [DataMember]
        [Id(4)]
        public ArtifactExecutableEnvironmentDefinition? Environment { get; }

        /// <summary>
        /// Gets the verbose.
        /// </summary>
        [DataMember]
        [Id(5)]
        public ArtifactExecVerboseEnum Verbose { get; }

        /// <summary>
        /// Gets the executor needed to execute the <see cref="ExecutablePath"/>.
        /// </summary>
        /// <remarks>
        ///     example: '<c>pyhton:6.0</c>', '<c>dotnet:6.0</c>', '<c>nodejs:2.0</c>', '<c>php:7.5</c>'
        /// </remarks>
        [DataMember]
        [Id(6)]
        public string? Executor { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] ArtifactDefinition other)
        {
            return other is ArtifactExecutableDefinition exec &&
                   this.ExecutablePath == exec.ExecutablePath &&
                   this.AllowPersistence == exec.AllowPersistence &&
                   this.Arguments.SequenceEqual(exec.Arguments) &&
                   this.Configurations.SequenceEqual(exec.Configurations) &&
                   (this.Environment?.Equals(exec.Environment) ?? exec.Environment is null) &&
                   this.Verbose == exec.Verbose &&
                   this.Executor == exec.Executor &&
                   base.OnEquals(other);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.ExecutablePath,
                                    this.AllowPersistence,
                                    this.Arguments.Aggregate(0, (acc, a) => acc ^ (a?.GetHashCode() ?? 0)),
                                    this.Configurations.Aggregate(0, (acc, a) => acc ^ (a?.GetHashCode() ?? 0)),
                                    this.Environment,
                                    this.Verbose,
                                    this.Executor,
                                    base.OnGetHashCode());
        }

        #endregion
    }
}
