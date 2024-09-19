// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    using Democrite.Framework.Builders.Models;

    using Microsoft.Extensions.Logging;

    using System.Threading.Tasks;

    /// <summary>
    /// Compiler to transform YAML description to democrite <see cref="IDefinition"/>
    /// </summary>
    public interface IDefinitionCompiler
    {
        /// <summary>
        /// Compiles <see cref="IDefinition"/> from yaml files
        /// </summary>
        ValueTask<CompilationResult> CompileAsync(Stream content,
                                                  DefinitionParserSourceEnum sourceType = DefinitionParserSourceEnum.Yaml,
                                                  ILogger? logger = null,
                                                  CancellationToken token = default);

        /// <summary>
        /// Compiles <see cref="IDefinition"/> from yaml files
        /// </summary>
        ValueTask<CompilationResult> CompileAsync(string content,
                                                  DefinitionParserSourceEnum sourceType = DefinitionParserSourceEnum.Yaml,
                                                  ILogger? logger = null,
                                                  CancellationToken token = default);
    }
}
