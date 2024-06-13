// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Builder in charge to produce a <see cref="ArtifactCodePackageResource"/>
    /// </summary>
    public interface IArtifactExecutablePackageResourceBuilder
    {
        /// <summary>
        /// Define the executor; example to run python script you need python installed.
        /// </summary>
        IArtifactCodePackageResourceBuilderFrom ExecuteBy(string executor, Version version);
    }

    /// <summary>
    /// Builder in charge to produce a code artifact after type defined (Python, c++, ...)
    /// </summary>
    public interface IArtifactCodePackageResourceBuilderFrom
    {
        /// <summary>
        /// Define where the artifact is and the type of package.
        /// </summary>
        IArtifactCodePackageResourceBuilderFromSource From(Uri packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version? version = null);

        /// <summary>
        /// Define where the artifact is and the type of package.
        /// </summary>
        IArtifactCodePackageResourceBuilderFromSource From(Uri packageUri, ArtifactPackageTypeEnum packageTypeEnum, string? version = null);

        /// <summary>
        /// Define where the artifact is and the type of package.
        /// </summary>
        IArtifactCodePackageResourceBuilderFromSource From(string packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version? version = null);

        /// <summary>
        /// Define where the artifact is and the type of package.
        /// </summary>
        IArtifactCodePackageResourceBuilderFromSource From(string packageUri, ArtifactPackageTypeEnum packageTypeEnum, string? version = null);
    }

    /// <summary>
    /// Builder in charge to produce a code artifact after type defined (Python, c++, ...)
    /// </summary>
    public interface IArtifactCodePackageResourceBuilderFiles<TBuilder>
    {
        /// <summary>
        /// Appends files.
        /// </summary>
        TBuilder AppendFiles(params string[] files);

        /// <summary>
        /// Appends files.
        /// </summary>
        TBuilder AppendFiles(string fileTemplate, bool recursive);

        /// <summary>
        /// Excludes all files path that correspond to following regex
        /// </summary>
        TBuilder ExcludePathRegex(Regex regex);

        /// <summary>
        /// Excludes all files
        /// </summary>
        TBuilder ExcludeFiles(params string[] files);

        /// <summary>
        /// Excludes all files of the directories.
        /// </summary>
        TBuilder ExcludeDirectories(params string[] directories);
    }

    /// <summary>
    /// Builder in charge to produce a code artifact after type defined (Python, c++, ...)
    /// </summary>
    public interface IArtifactCodePackageResourceBuilderFromSource : IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>
    {
        /// <summary>
        /// Define the file to execute on the package datas.
        /// </summary>
        IArtifactCodePackageResourceBuilderFinalizer ExecuteFile(string exec);
    }

    /// <summary>
    /// Builder in charge to produce a code artifact after type defined (Python, c++, ...)
    /// </summary>
    public interface IArtifactCodePackageResourceBuilderFinalizer : IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>, IDefinitionBaseCompiler<ArtifactExecutableDefinition>
    {
        /// <summary>
        /// Define the argument to pass by the executable at start.
        /// </summary>
        IArtifactCodePackageResourceBuilderFinalizer Arguments(params string[] args);

        /// <summary>
        /// Adds a configuration to pass
        /// </summary>
        /// <param name="configSectionKey">[NOT Implemented YET] will be used to ensure configuration value is pass securly to artifact</param>
        IArtifactCodePackageResourceBuilderFinalizer AddConfiguration<TConfigType>(string configName, TConfigType configType, bool secureDataTansfer = false);

        /// <summary>
        /// Define a requireds configuration.
        /// </summary>
        /// <param name="secureDataTansfer">Define configuration section path; will be searched in the configuration system, file, service ...</param>
        /// <param name="defaultValue">Define a default value if runtime value is not founded</param>
        /// <param name="configSectionKey">[NOT Implemented YET] will be used to ensure configuration value is pass securly to artifact</param>
        IArtifactCodePackageResourceBuilderFinalizer RequiredConfiguration<TConfigType>(string configName, string configSectionKey, TConfigType? defaultValue = default, bool secureDataTansfer = false);

        /// <summary>
        /// Define if the executable managed to stay alive after usage.
        /// </summary>
        IArtifactCodePackageResourceBuilderFinalizer Verbose(ArtifactExecVerboseEnum verbose);

        /// <summary>
        /// Define if the executable managed to stay alive after usage.
        /// </summary>
        IArtifactCodePackageResourceBuilderFinalizer Persistent();

        /// <summary>
        /// Define if the executable environment docker, anaconda, ...
        /// </summary>
        IArtifactCodePackageResourceBuilderFinalizer ExecEnvironment(Action<IArtifactCodePackageResourceEnvironmentBuilder> envBuilder);
    }

    /// <summary>
    /// Build execution environment host docker, anaconda, ...
    /// </summary>
    public interface IArtifactCodePackageResourceEnvironmentBuilder
    {
        /// <summary>
        /// Builders the specified builder.
        /// </summary>
        void Builder(IDefinitionBaseBuilder<ArtifactExecutableEnvironmentDefinition> builder);
    }
}
