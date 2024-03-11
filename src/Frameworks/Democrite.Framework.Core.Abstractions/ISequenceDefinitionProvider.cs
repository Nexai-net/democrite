// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Sequence;
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;

    /// <summary>
    /// Provider used to get <see cref="SequenceDefinition"/> from <see cref="ISequenceDefinitionSourceProvider"/>
    /// </summary>
    /// <seealso cref="IProviderStrategy{SequenceDefinition, Guid}" />
    public interface ISequenceDefinitionProvider : IProviderStrategy<SequenceDefinition, Guid>
    {
    }
}
