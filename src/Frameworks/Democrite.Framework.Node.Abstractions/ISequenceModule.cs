// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions
{
    using Democrite.Framework.Core.Abstractions.Sequence;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Module in charge to provide <see cref="IReadOnlyCollection{SequenceDefinition}"/>
    /// </summary>
    public interface ISequenceModule
    {
        /// <summary>
        /// Gets the sequence definitions.
        /// </summary>
        ValueTask<IReadOnlyCollection<SequenceDefinition>> GetSequenceDefinitionsAsync();
    }
}
