// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Blackboard.Builders.Templates
{
    using Democrite.Framework.Builders;
    using Democrite.Framework.Node.Blackboard.Abstractions.Models;

    /// <summary>
    /// Configuration finalizer 
    /// </summary>
    public interface IBlackboardTemplateFinalizerBuilder : IDefinitionBaseBuilder<BlackboardTemplateDefinition>
    {
        /// <summary>
        /// Configures the default storage.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder SetupDefaultStorage(string storageKey, string? storageConfiguration = null);

        /// <summary>
        /// Logicals the type configuration.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder LogicalTypeConfiguration(string logicRecordTypePattern, Action<ILogicalTypeConfiguration> cfg);

        /// <summary>
        /// Logicals the type configuration.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder AnyLogicalTypeConfiguration(Action<ILogicalTypeConfiguration>? cfg = null);
    }
}
