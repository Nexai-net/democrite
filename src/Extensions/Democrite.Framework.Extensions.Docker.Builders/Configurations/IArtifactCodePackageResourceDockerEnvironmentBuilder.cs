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

        /// <summary>
        /// Setup a specific source repository for the image, by default it use to docker public depo
        /// </summary>
        IArtifactCodePackageResourceDockerEnvironmentBuilder SourceRepository(string repository);

        /// <summary>
        /// Set a flag to not fetch the image from an external repository; Attention hte IMAGE MUST be present in local to work
        /// </summary>
        IArtifactCodePackageResourceDockerEnvironmentBuilder OnlyFromLocal();

        /// <summary>
        /// Rquested GPU capacity and usage on the docker image
        /// </summary>
        IArtifactCodePackageResourceDockerEnvironmentBuilder UseGpu(string? gpuFilter = null);

        //IArtifactCodePackageResourceDockerEnvironmentBuilder Mount(string mountInfo);

        /// <summary>
        /// Adds the extra build instruction to customize the image.
        /// </summary>
        IArtifactCodePackageResourceDockerEnvironmentBuilder AddExtraBuildInstruction(params string[] buildInstruction);
    }
}
