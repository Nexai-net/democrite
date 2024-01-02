// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Builders.Triggers
{
    /// <summary>
    /// Configure witch input will be send when the trigger fire
    /// </summary>
    public interface ITriggerInputBuilder
    {
        /// <summary>
        /// Build input from a serializable static collection.
        /// </summary>
        ITriggerStaticCollectionInputBuilder<TTriggerOutput> StaticCollection<TTriggerOutput>(IEnumerable<TTriggerOutput> collection);
    }

    /// <summary>
    /// Configure witch input will be send when the trigger fire
    /// </summary>
    /// <typeparam name="TTriggerOutput">The type of the output message.</typeparam>
    public interface ITriggerInputBuilder<TTriggerOutput>
    {
    }
}
