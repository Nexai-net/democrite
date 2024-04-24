// keep : Democrite.Framework.Builders
namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Artifacts;
    using Democrite.Framework.Extensions.Docker.Builders.Configurations;

    using System;

    public static class ArtifactEnvironmentDockerBuilder
    {
        /// <summary>
        /// Define docker environment to host the executable artifact
        /// </summary>
        public static IArtifactCodePackageResourceDockerEnvironmentBuilder Docker(this IArtifactCodePackageResourceEnvironmentBuilder environment, string? minialDockerVersion = null)
        {
            var inst = new ArtifactCodePackageResourceDockerEnvironmentBuilder(string.IsNullOrEmpty(minialDockerVersion) ? null : Version.Parse(minialDockerVersion!));
            environment.Builder(inst);
            return inst;
        }
    }
}
