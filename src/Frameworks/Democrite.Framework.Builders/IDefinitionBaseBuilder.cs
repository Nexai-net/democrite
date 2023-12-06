// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders
{
    /// <summary>
    /// Base interface of all the end part of builders
    /// </summary>
    public interface IDefinitionBaseBuilder<TDefinition>
    {
        /// <summary>
        /// Compile setup information to build <see cref="TDefinition"/>
        /// </summary>
        TDefinition Build();
    }
}
