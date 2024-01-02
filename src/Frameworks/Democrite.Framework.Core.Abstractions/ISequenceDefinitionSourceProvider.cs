// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider used to access <see cref="SequenceDefinition"/>
    /// </summary>
    /// <remarks>
    ///     Use <see cref="ISequenceDefinitionProvider"/> to access <see cref="SequenceDefinition"/>
    /// </remarks>
    public interface ISequenceDefinitionSourceProvider : IProviderStrategySource<SequenceDefinition, Guid>, INodeInitService
    {
    }
}
