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

        private readonly string _fullImageName;
        public const string ENVIRONMENT_KEY = "docker";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutableDockerEnvironmentDefinition"/> class.
        /// </summary>
        public ArtifactExecutableDockerEnvironmentDefinition(string? minimalRequiredVersion,
                                                             string image,
                                                             string? tag,
                                                             string? gpuRequirement,
                                                             bool onlyLocal = false,
                                                             string? repository = null,
                                                             string? configurationName = null,
                                                             IEnumerable<string>? extraDockerFileInstructions = null)
            : base(ENVIRONMENT_KEY, configurationName, minimalRequiredVersion)
        {
            this.Repository = repository;
            this.OnlyLocal = onlyLocal;
            this.GPURequirement = gpuRequirement;
            this.Image = image;
            this.Tag = tag;
            this.ExtraDockerFileInstructions = extraDockerFileInstructions?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;

            this._fullImageName = GetFullImageFormatedName(repository);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the image.
        /// </summary>
        [DataMember]
        public string Image { get; }

        /// <summary>
        /// Gets the tag.
        /// </summary>
        [DataMember]
        public string? Tag { get; }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        [DataMember]
        public string? Repository { get; }

        /// <summary>
        /// Gets a value indicating whether image MUST not be pull, only local value as to be used.
        /// </summary>
        [DataMember]
        public bool OnlyLocal { get; }

        /// <summary>
        /// Gets the gpu requirement.
        /// </summary>
        [DataMember]
        public string? GPURequirement { get; }

        /// <summary>
        /// Gets the extra docker file instructions.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<string> ExtraDockerFileInstructions { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the full name of the image formated.
        /// </summary>
        /// <param name="defaultConfigureRepo">The default configure repository.</param>
        public string GetFullImageFormatedName(string? defaultConfigureRepo = null)
        {
            return (this.OnlyLocal == false
                        ? ((string.IsNullOrEmpty(this.Repository)
                                 ? string.IsNullOrEmpty(defaultConfigureRepo) ? "" : defaultConfigureRepo + "/"
                                 : this.Repository + "/"))
                        : "")
                    + this.Image
                    + (string.IsNullOrEmpty(this.Tag) ? ":latest" : ":" + this.Tag);
        }

        /// <inheritdoc />
        protected override bool OnEquals([NotNull] ArtifactExecutableEnvironmentDefinition other)
        {
            return other is ArtifactExecutableDockerEnvironmentDefinition docker &&
                   string.Equals(this.Image, docker.Image, StringComparison.OrdinalIgnoreCase) &&
                   this.OnlyLocal == docker.OnlyLocal &&
                   string.Equals(this.Repository, docker.Repository, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.ConfigurationName, docker.ConfigurationName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Tag, docker.Tag, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.GPURequirement, docker.GPURequirement, StringComparison.OrdinalIgnoreCase) &&
                   this.ExtraDockerFileInstructions.SequenceEqual(docker.ExtraDockerFileInstructions);
        }

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return HashCode.Combine(this.Image,
                                    this.Tag,
                                    this.GPURequirement,
                                    this.Repository,
                                    this.ConfigurationName,
                                    this.OnlyLocal,
                                    this.ExtraDockerFileInstructions.Aggregate(0, (acc, e) => e.GetHashCode() ^ acc));
        }

        #endregion
    }
}
