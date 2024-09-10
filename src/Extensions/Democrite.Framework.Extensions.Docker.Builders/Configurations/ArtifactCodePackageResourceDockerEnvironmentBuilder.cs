// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker.Builders.Configurations
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Extensions.Docker.Abstractions.Models;

    using System;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IArtifactCodePackageResourceDockerEnvironmentBuilder" />
    internal sealed class ArtifactCodePackageResourceDockerEnvironmentBuilder : IArtifactCodePackageResourceDockerEnvironmentBuilder, IDefinitionBaseBuilder<ArtifactExecutableEnvironmentDefinition>
    {
        #region Fields

        private readonly Version? _minimalRequiredDockerVersion;
        private readonly List<string> _extraImageBuildInstruction;

        private string? _imageName;
        private string? _useGpu;
        private string? _tag;
        private string? _repository;
        private bool _onlyLocal;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactCodePackageResourceDockerEnvironmentBuilder"/> class.
        /// </summary>
        public ArtifactCodePackageResourceDockerEnvironmentBuilder(Version? minimalRequiredDockerVersion)
        {
            this._minimalRequiredDockerVersion = minimalRequiredDockerVersion;
            this._extraImageBuildInstruction = new List<string>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IArtifactCodePackageResourceDockerEnvironmentBuilder Image(string name, string? tag = null)
        {
            this._imageName = name;
            this._tag = tag;

            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceDockerEnvironmentBuilder UseGpu(string? gpuFilter = null)
        {
            this._useGpu = gpuFilter ?? "all";
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceDockerEnvironmentBuilder SourceRepository(string repository)
        {
            this._repository = repository;
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceDockerEnvironmentBuilder OnlyFromLocal()
        {
            this._onlyLocal = true;
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceDockerEnvironmentBuilder AddExtraBuildInstruction(params string[] buildInstruction)
        {
            this._extraImageBuildInstruction.AddRange(buildInstruction);
            return this;
        }

        /// <inheritdoc />
        public ArtifactExecutableEnvironmentDefinition Build()
        {
            ArgumentNullException.ThrowIfNullOrEmpty(this._imageName);
            return new ArtifactExecutableDockerEnvironmentDefinition(this._minimalRequiredDockerVersion?.ToString(),
                                                                     this._imageName,
                                                                     this._tag,
                                                                     this._useGpu,
                                                                     onlyLocal: this._onlyLocal,
                                                                     repository: this._repository,
                                                                     extraDockerFileInstructions: this._extraImageBuildInstruction);
        }

        #endregion
    }
}
