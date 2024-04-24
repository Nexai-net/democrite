// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define an executable artifact
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
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
                                            string? description,
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
                                            ArtifactExecutableEnvironmentDefinition? environment)
            : base(uid, displayName, description, version, hash, creationOn, ArtifactTypeEnum.Executable, packageSource, packageFiles, packageType)
        {
            this.ExecutablePath = executablePath;
            this.AllowPersistence = allowPersistence;
            this.Executor = executor;
            this.Arguments = arguments?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            this.Environment = environment;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the executable path.
        /// </summary>
        [DataMember]
        public string ExecutablePath { get; }

        /// <summary>
        /// Gets a value indicating whether the executable managed to stay alive an call multiple times.
        /// </summary>
        [DataMember]
        public bool AllowPersistence { get; }

        /// <summary>
        /// Gets the execution arguments.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<string> Arguments { get; }

        /// <summary>
        /// Gets the execution environment.
        /// </summary>
        [DataMember]
        public ArtifactExecutableEnvironmentDefinition? Environment { get; }

        /// <summary>
        /// Gets the executor needed to execute the <see cref="ExecutablePath"/>.
        /// </summary>
        /// <remarks>
        ///     example: '<c>pyhton:6.0</c>', '<c>dotnet:6.0</c>', '<c>nodejs:2.0</c>', '<c>php:7.5</c>'
        /// </remarks>
        [DataMember]
        public string? Executor { get; }

        #endregion
    }
}
