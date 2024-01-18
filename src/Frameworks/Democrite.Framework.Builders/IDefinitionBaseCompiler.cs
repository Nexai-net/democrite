// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Toolbox.Abstractions.Services;
    using Democrite.Framework.Toolbox.Services;

    /// <summary>
    /// Base interface of all the end part of builders
    /// </summary>
    public interface IDefinitionBaseCompiler<TDefinition>
    {
        /// <summary>
        /// Compile setup information to compile <see cref="TDefinition"/>
        /// </summary>
        /// <param name="hashService"><see cref="HashSHA256Service"/> by default.</param>
        /// <param name="fileSystemHandler"><see cref="FileSystemHandler"/> by default.</param>
        ValueTask<TDefinition> CompileAsync(IHashService? hashService = null,
                                            IFileSystemHandler? fileSystemHandler = null,
                                            ITimeManager? timeManager = null,
                                            CancellationToken token = default);
    }
}
