// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using System.Text.RegularExpressions;

    /// <summary>
    /// Extension method to simplify the configurations
    /// </summary>
    public static class ArtifactExecutablePackageBuilderExtensions
    {
        /// <summary>
        /// Configure python code
        /// </summary>
        public static IArtifactCodePackageResourceBuilderFrom Python(this IArtifactExecutablePackageResourceBuilder builder, string? version = null) 
        {
            if (string.IsNullOrEmpty(version))
                version = "3.12.1";
            return builder.ExecuteBy("Python", Version.Parse(version));
        }

        /// <summary>
        /// Configure python code
        /// </summary>
        public static IArtifactCodePackageResourceBuilderFinalizer Python(this IArtifactExecutablePackageResourceBuilder builder,
                                                                          string relativeSourceDirectorPath,
                                                                          string exec,
                                                                          bool addAllPythonFiles = true,
                                                                          string? pythonVersion = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(relativeSourceDirectorPath);
            ArgumentNullException.ThrowIfNullOrEmpty(exec);

            if (string.IsNullOrEmpty(pythonVersion))
                pythonVersion = "3.12.1";

            var result = builder.ExecuteBy("Python", Version.Parse(pythonVersion))
                                .Directory(relativeSourceDirectorPath)
                                .ExecuteFile(exec)
                                .ExcludePathRegex(new Regex(".*__pycache__.*"));

            if (addAllPythonFiles)
                result = result.AppendFiles("*.py", true);

            return result;
        }

        /// <summary>
        /// Define the directory where the folder script is.
        /// </summary>
        public static IArtifactCodePackageResourceBuilderFromSource Directory(this IArtifactCodePackageResourceBuilderFrom builder,
                                                                              string packageUri,
                                                                              string? version = null,
                                                                              bool addAllDirectoryFiles = false)
        {
            var builderResult = builder.From(packageUri, ArtifactPackageTypeEnum.Directory, version);

            if (addAllDirectoryFiles)
                builderResult.AppendFiles("*", true);

            return builderResult;
        }
    }
}
