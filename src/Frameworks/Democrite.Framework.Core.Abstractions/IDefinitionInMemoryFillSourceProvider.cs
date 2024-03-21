// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    /// <summary>
    /// Provider used to store in memory definitions
    /// </summary>
    public interface IDefinitionInMemoryFillSourceProvider
    {
        /// <summary>
        /// Determines whether this instance can store the specified definition.
        /// </summary>
        bool CanStore(IDefinition definition);

        /// <summary>
        /// Tries to store the definition
        /// </summary>
        bool TryStore(IDefinition definition);
    }
}
