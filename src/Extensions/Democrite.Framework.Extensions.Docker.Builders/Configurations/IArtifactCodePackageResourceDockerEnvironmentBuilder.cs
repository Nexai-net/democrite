// Keep : Democrite.Framework.Builders
namespace Democrite.Framework.Builders
{
    /// <summary>
    /// 
    /// </summary>
    public interface IArtifactCodePackageResourceDockerEnvironmentBuilder
    {
        /// <summary>
        /// Specify the image to run
        /// </summary>
        IArtifactCodePackageResourceDockerEnvironmentBuilder Image(string name, string? tag = null);

        //IArtifactCodePackageResourceDockerEnvironmentBuilder SourceRepository(Uri uri);

        /// <summary>
        /// Rquested GPU capacity and usage on the docker image
        /// </summary>
        IArtifactCodePackageResourceDockerEnvironmentBuilder UseGpu(string? gpuFilter = null);

        //IArtifactCodePackageResourceDockerEnvironmentBuilder Mount(string mountInfo);
    }
}
