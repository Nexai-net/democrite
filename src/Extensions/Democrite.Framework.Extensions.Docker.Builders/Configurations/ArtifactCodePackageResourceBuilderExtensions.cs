// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

// KEEP : Democrite.Framework.Builders
namespace Democrite.Framework.Builders
{
    /// <summary>
    /// 
    /// </summary>
    public static class ArtifactCodePackageResourceBuilderExtensions
    {
        /// <summary>
        /// Installs the democrite python library
        /// </summary>
        public static IArtifactCodePackageResourceDockerEnvironmentBuilder InstallDemocritePython(this IArtifactCodePackageResourceDockerEnvironmentBuilder builder)
        {
            builder.AddExtraBuildInstruction("RUN pip install democrite");
            return builder;
        }
    }
}
