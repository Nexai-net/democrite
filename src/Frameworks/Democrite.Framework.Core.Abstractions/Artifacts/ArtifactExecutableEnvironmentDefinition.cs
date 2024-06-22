// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public abstract class ArtifactExecutableEnvironmentDefinition : IEquatable<ArtifactExecutableEnvironmentDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutableEnvironmentDefinition"/> class.
        /// </summary>
        protected ArtifactExecutableEnvironmentDefinition(string environmentName, string? configurationName, string? minimalRequiredVersion)
        {
            this.EnvironmentName = environmentName;
            this.ConfigurationName = configurationName; 
            this.MinimalRequiredVersion = minimalRequiredVersion;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the environment.
        /// </summary>
        [DataMember]
        public string EnvironmentName { get; }

        /// <summary>
        /// Gets the name of the specific configuration.
        /// </summary>
        [DataMember]
        public string? ConfigurationName { get; }

        /// <summary>
        /// Gets the minimal required version.
        /// </summary>
        [DataMember]
        public string? MinimalRequiredVersion { get; }

        #endregion

        #region Methods

        /// <inheritdoc cref="object.Equals(object?, object?)" />
        public bool Equals(ArtifactExecutableEnvironmentDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.EnvironmentName == other.EnvironmentName &&
                   this.ConfigurationName == other.ConfigurationName &&
                   this.MinimalRequiredVersion == other.MinimalRequiredVersion &&
                   OnEquals(other);
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is ArtifactExecutableEnvironmentDefinition env)
                return Equals(env);

            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.EnvironmentName, 
                                    this.MinimalRequiredVersion,
                                    this.ConfigurationName,
                                    OnGetHashCode());
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract object OnGetHashCode();

        /// <inheritdoc cref="object.Equals(object?)" />
        protected abstract bool OnEquals([NotNull] ArtifactExecutableEnvironmentDefinition other);

        #endregion
    }
}
