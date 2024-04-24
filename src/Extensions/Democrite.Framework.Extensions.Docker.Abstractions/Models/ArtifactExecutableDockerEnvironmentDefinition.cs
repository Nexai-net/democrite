namespace Democrite.Framework.Extensions.Docker.Abstractions.Models
{
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [Immutable]
    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class ArtifactExecutableDockerEnvironmentDefinition : ArtifactExecutableEnvironmentDefinition
    {
        #region Fields

        public const string ENVIRONMENT_KEY = "docker";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutableDockerEnvironmentDefinition"/> class.
        /// </summary>
        public ArtifactExecutableDockerEnvironmentDefinition(string? minimalRequiredVersion,
                                                             string image,
                                                             string? tag,
                                                             string? gpuRequirement)
            : base(ENVIRONMENT_KEY, minimalRequiredVersion)
        {
            this.GPURequirement = gpuRequirement;
            this.Image = image;
            this.Tag = tag;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the image.
        /// </summary>
        public string Image { get; }

        /// <summary>
        /// Gets the tag.
        /// </summary>
        public string? Tag { get; }

        /// <summary>
        /// Gets the gpu requirement.
        /// </summary>
        public string? GPURequirement { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] ArtifactExecutableEnvironmentDefinition other)
        {
            return other is ArtifactExecutableDockerEnvironmentDefinition docker &&
                   string.Equals(this.Image, docker.Image, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Tag, docker.Tag, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.GPURequirement, docker.GPURequirement, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return HashCode.Combine(this.Image, this.Tag, this.GPURequirement);
        }

        #endregion
    }
}
