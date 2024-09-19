// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Models
{
    using Democrite.Framework.Core.Abstractions;

    using Elvex.Toolbox.Abstractions.Loggers;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class DefinitionCompileOption(string? StorageConfigurationName);

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class CompilationResult(bool Success,
                                          IReadOnlyCollection<SimpleLog>? Logs,
                                          DefinitionCompileOption? CompileOption,
                                          IReadOnlyCollection<IDefinition> Definitions);
}
